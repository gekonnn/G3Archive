﻿namespace G3Archive
{
    public class G3Pak_FileTableEntry
    {   
        public G3Pak_FileTableEntry_Header Header;
        public G3Pak_DirectoryEntry DirectoryEntry = default!;
        public G3Pak_FileEntry FileEntry = default!;

        public G3Pak_FileTableEntry(BinaryReader br)
        {
            Header = new G3Pak_FileTableEntry_Header(br);
            if ((Header.Attributes & (uint)G3Pak_FileAttribute.Directory) > 0)
            {
                DirectoryEntry = new G3Pak_DirectoryEntry(br);
            }
            else
            {
                FileEntry = new G3Pak_FileEntry(br);
            }
        }

        public G3Pak_FileTableEntry(BinaryWriter bw, FileInfo File, FileInfo RootDirectory, UInt64 OffsetToFiles = 0)
        {
            Header = new G3Pak_FileTableEntry_Header(bw, File);
            if ((Header.Attributes & (uint)G3Pak_FileAttribute.Directory) > 0)
            {
                DirectoryEntry = new G3Pak_DirectoryEntry(bw, File, RootDirectory);
            }
            else
            {
                FileEntry = new G3Pak_FileEntry(bw, File, RootDirectory, OffsetToFiles);
            }
        }

        public void WriteEntry(BinaryWriter bw)
        {
            // Write header
            Header.Write(bw);

            // Write DirectoryEntry
            if ((Header.Attributes & (uint)G3Pak_FileAttribute.Directory) > 0)
            {
                DirectoryEntry.FileName.Write(bw);
                bw.Write(DirectoryEntry.DirCount);
                foreach(G3Pak_FileTableEntry entry in DirectoryEntry.DirTable)
                {
                    entry.WriteEntry(bw);
                }
                bw.Write(DirectoryEntry.FileCount);
                foreach (G3Pak_FileTableEntry entry in DirectoryEntry.FileTable)
                {
                    entry.WriteEntry(bw);
                }
            }
            else // Write FileEntry
            {
                FileEntry.WriteEntry(bw);
            }
        }

        public void WriteData(BinaryWriter bw, long Offset)
        {
            if (FileEntry.EntryFile != null)
            {
                byte[] RawData = FileEntry.ReadBytes();
                if (RawData.Length > (int)G3Pak_Compression.Threshold && !ZLibUtil.CompressionExcludedFileTypes.Contains(FileEntry.EntryFile.Extension))
                {
                    Header.Attributes += (int)G3Pak_FileAttribute.Compressed;
                    FileEntry.Compression = (int)G3Pak_Compression.Zip;

                    byte[] compressedData = ZLibUtil.Compress(RawData, FileEntry.FileName.GetString());
                    if (compressedData.Length > 0)
                    { 
                        RawData = compressedData;
                        FileEntry.Bytes = (UInt64)compressedData.Length;
                    }
                }
                
                bw.BaseStream.Seek(Offset, SeekOrigin.Begin);
                bw.Write(RawData);
            }
        }

        public async Task ExtractFile(BinaryReader br, string Dest)
        {
            string FileName = FileEntry.FileName.GetString();

            Logger.Log(string.Format("Extracting {0} ({1} bytes)", FileName, FileEntry.Size));

            br.BaseStream.Seek(Convert.ToInt64(FileEntry.Offset), SeekOrigin.Begin);
            int RawData_Bytes = (int)FileEntry.Bytes;
            byte[] RawData = br.ReadBytes(RawData_Bytes);

            using FileStream fs = new(Path.Combine(Dest, FileName), FileMode.OpenOrCreate);
            if (FileEntry.Compression == (int)G3Pak_Compression.Zip && !Options.NoDecompress)
            {
                byte[] decompressedData = await ZLibUtil.Decompress(RawData, FileEntry.FileName.GetString());
                if (decompressedData.Length > 0) RawData = decompressedData;
            }

            await fs.WriteAsync(RawData);
            await fs.FlushAsync();
        }
        
        public async Task<bool> ExtractDirectory(BinaryReader br, string Dest, bool Overwrite)
        {
            if (Directory.Exists(Dest) && !Overwrite)
            {
                throw new(string.Format("Warning: Directory named {0} already exists.\nConsider renaming the directory or using \"--overwrite\" option", 
                           Path.GetDirectoryName(Dest)));
            }

            Directory.CreateDirectory(Dest);

            foreach(G3Pak_FileTableEntry Entry in DirectoryEntry.DirTable)
            {
                string FileName = Entry.DirectoryEntry.FileName.GetString();

                // Skip "_deleted" directories if NoDeleted is enabled.
                if (Options.NoDeleted && FileName.StartsWith("_deleted")) continue;

                Directory.CreateDirectory(Path.Combine(Dest, FileName));
                await Entry.ExtractDirectory(br, Dest, true); // Extract child folders recursively
            }
            
            List<Task> tasks = new();
            foreach (G3Pak_FileTableEntry Entry in DirectoryEntry.FileTable)
            {
                // Skip "_deleted" files if NoDeleted is enabled.
                if (Options.NoDeleted && ((Header.Attributes & (uint)G3Pak_FileAttribute.Deleted) > 0 || 
                    Entry.FileEntry.FileName.GetString().StartsWith("_deleted"))) 
                continue;
                
                tasks.Add(Entry.ExtractFile(br, Dest));
            }
            await Task.WhenAll(tasks);

            return true;
        }
    }
}
