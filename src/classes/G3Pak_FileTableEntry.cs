namespace G3Archive
{
    public class G3Pak_FileTableEntry
    {
        public readonly G3Pak_Archive Archive = default!;

        public G3Pak_FileTableEntry_Header Header = default!;
        public G3Pak_DirectoryEntry DirectoryEntry = default!;
        public G3Pak_FileEntry FileEntry = default!;

        public G3Pak_FileTableEntry(G3Pak_Archive Archive)
        {
            this.Archive = Archive;
        }

        public void ReadFromArchive()
        {   
            Header = new G3Pak_FileTableEntry_Header(Archive);
            Header.ReadFromArchive();

            if ((Header.Attributes & (uint)G3Pak_FileAttribute.Directory) > 0)
            {
                DirectoryEntry = new G3Pak_DirectoryEntry(Archive);
                DirectoryEntry.ReadFromArchive();
            }
            else
            {
                FileEntry = new G3Pak_FileEntry(Archive);
                FileEntry.ReadFromArchive();
            }
        }

        public void Write(FileInfo File, FileInfo RootDirectory, UInt64 OffsetToFiles = 0)
        {
            Header = new G3Pak_FileTableEntry_Header(Archive);
            Header.ReadFromFile(File);

            if ((Header.Attributes & (uint)G3Pak_FileAttribute.Directory) > 0)
            {
                DirectoryEntry = new G3Pak_DirectoryEntry(Archive);
                DirectoryEntry.Write(File, RootDirectory);
            }
            else
            {
                FileEntry = new G3Pak_FileEntry(Archive);
                FileEntry.ReadFromFile(File, RootDirectory, OffsetToFiles);
                FileEntry.Write();
            }
        }

        public void WriteEntry()
        {
            // Write header
            Header.Write();

            // Write DirectoryEntry
            if ((Header.Attributes & (uint)G3Pak_FileAttribute.Directory) > 0)
            {
                DirectoryEntry.FileName.Write();
                Archive.Writer.Write(DirectoryEntry.DirCount);
                foreach(G3Pak_FileTableEntry entry in DirectoryEntry.DirTable)
                {
                    entry.WriteEntry();
                }
                Archive.Writer.Write(DirectoryEntry.FileCount);
                foreach (G3Pak_FileTableEntry entry in DirectoryEntry.FileTable)
                {
                    entry.WriteEntry();
                }
            }
            else // Write FileEntry
            {
                FileEntry.Write();
            }
        }

        public void WriteData(long Offset)
        {
            if (FileEntry.EntryFile != null)
            {
                byte[] RawData = FileEntry.ReadFileBytes();
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
                
                Archive.Writer.BaseStream.Seek(Offset, SeekOrigin.Begin);
                Archive.Writer.Write(RawData);
            }
        }

        public async Task ExtractFile(string Dest)
        {
            string FileName = FileEntry.FileName.GetString();

            Logger.Log(string.Format("Extracting {0} ({1} bytes)", FileName, FileEntry.Size));

            Archive.fs.Seek(Convert.ToInt64(FileEntry.Offset), SeekOrigin.Begin);
            int RawData_Bytes = (int)FileEntry.Bytes;
            byte[] RawData = Archive.Reader.ReadBytes(RawData_Bytes);

            using FileStream fs = new(Path.Combine(Dest, FileName), FileMode.OpenOrCreate);
            if (FileEntry.Compression == (int)G3Pak_Compression.Zip && !Archive.Options.NoDecompress)
            {
                byte[] decompressedData = await ZLibUtil.Decompress(RawData, FileEntry.FileName.GetString());
                if (decompressedData.Length > 0) RawData = decompressedData;
            }

            await fs.WriteAsync(RawData);
            await fs.FlushAsync();
        }
        
        public async Task<bool> ExtractDirectory(string Dest, bool Overwrite)
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
                if (Archive.Options.NoDeleted && FileName.StartsWith("_deleted")) continue;

                Directory.CreateDirectory(Path.Combine(Dest, FileName));
                await Entry.ExtractDirectory(Dest, true); // Extract child folders recursively
            }
            
            List<Task> tasks = new();
            foreach (G3Pak_FileTableEntry Entry in DirectoryEntry.FileTable)
            {
                // Skip "_deleted" files if NoDeleted is enabled.
                if (Archive.Options.NoDeleted && ((Header.Attributes & (uint)G3Pak_FileAttribute.Deleted) > 0 || 
                    Entry.FileEntry.FileName.GetString().StartsWith("_deleted"))) 
                continue;
                
                tasks.Add(Entry.ExtractFile(Dest));
            }
            await Task.WhenAll(tasks);

            return true;
        }
    }
}
