using System.IO.Compression;

namespace G3Archive
{
    public class G3Pak_FileTableEntry
    {
        public G3Pak_FileTableEntry_Header Header;
        public G3Pak_DirectoryEntry DirectoryEntry = default!;
        public G3Pak_FileEntry FileEntry = default!;

        public G3Pak_FileTableEntry(ReadBinary Read)
        {
            Header = new G3Pak_FileTableEntry_Header(Read);
            if (((FileAttributes)Header.Attributes & FileAttributes.Directory) > 0)
            {
                DirectoryEntry = new G3Pak_DirectoryEntry(Read);
            }
            else
            {
                FileEntry = new G3Pak_FileEntry(Read);
            }
        }

        public G3Pak_FileTableEntry(BinaryWriter bw, FileInfo File, FileInfo RootDirectory, UInt64 OffsetToFiles = 0)
        {
            Header = new G3Pak_FileTableEntry_Header(bw, File);
            if (((FileAttributes)Header.Attributes & FileAttributes.Directory) > 0)
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
            bw.Write(Header.FileTime1);
            bw.Write(Header.FileTime2);
            bw.Write(Header.FileTime3);
            bw.Write(Header.FileSizeHigh);
            bw.Write(Header.FileSizeLow);
            bw.Write(Header.Attributes);

            // Write DirectoryEntry
            if (((FileAttributes)Header.Attributes & FileAttributes.Directory) > 0)
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
                bw.Write(FileEntry.Offset);
                bw.Write(FileEntry.Bytes);
                bw.Write(FileEntry.Size);
                bw.Write(FileEntry.Encryption);
                bw.Write(FileEntry.Compression);
                FileEntry.FileName.Write(bw);
                FileEntry.Comment.Write(bw);
            }
        }

        private byte[] Decompress(byte[] RawData)
        {
            string FileName = string.Join("", FileEntry.FileName.Data);
            
            // Ensure the file has a valid zlib header
            if (RawData[0] != 0x78) { 
                Logger.Log("Incorrect zlib header for " + FileName); 
                return new byte[0];
            }

            try
            {
                using (MemoryStream input = new MemoryStream(RawData))
                using (MemoryStream output = new MemoryStream())
                {
                    using (ZLibStream decompressionStream = new ZLibStream(input, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(output);
                    }
                    return output.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Decompression failed for {0} (Header = {1})", FileName, RawData[0].ToString("X2") + RawData[1].ToString("X2")));
                Logger.Log("Exception: " + ex);
                return new byte[0];
            }
        }

        public void ExtractFile(ReadBinary Read, string Dest)
        {
            string FileName = string.Join("", FileEntry.FileName.Data);
            Logger.Log(string.Format("Extracting {0} ({1} bytes)", FileName, FileEntry.Size));

            Read.fs.Seek(Convert.ToInt64(FileEntry.Offset), SeekOrigin.Begin);
            int rawData_Bytes = (int)FileEntry.Bytes;
            byte[] rawData = Read.Bytes(rawData_Bytes);

            using (FileStream _fs = new FileStream(Path.Combine(Dest, FileName), FileMode.OpenOrCreate))
            {
                if (FileEntry.Compression == 2) // Extract the compressed file
                {
                    byte[] decompressedData = Decompress(rawData);
                    if(rawData.Length > 0) { rawData = decompressedData; }
                }

                _fs.Write(rawData);
                _fs.Flush();
            }
        }

        public int ExtractDirectory(ReadBinary Read, string Dest, bool Overwrite)
        {
            if (Directory.Exists(Dest) && !Overwrite)
            {
                Logger.Log(string.Format("Warning: Directory named {0} already exists.\nConsider renaming the directory or using \"--overwrite\" option", 
                           Path.GetFileName(Path.GetDirectoryName(Dest))));
                return 1;
            }

            Directory.CreateDirectory(Dest);

            foreach(G3Pak_FileTableEntry Entry in DirectoryEntry.DirTable)
            {
                string FileName = string.Join("", Entry.DirectoryEntry.FileName.Data);
                Directory.CreateDirectory(Path.Combine(Dest, FileName));
                Entry.ExtractDirectory(Read, Dest, true); // Extract child folders
            }
            foreach (G3Pak_FileTableEntry Entry in DirectoryEntry.FileTable)
            {
                Entry.ExtractFile(Read, Dest);
            }

            return 0;
        }
    }
}
