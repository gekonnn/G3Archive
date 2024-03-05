using System.IO.Compression;

namespace G3Archive
{
    public class G3Pak_FileTableEntry
    {
        public G3Pak_FileTableEntry_Header Header;
        public G3Pak_DirectoryEntry DirectoryEntry = default!;
        public G3Pak_FileEntry FileEntry = default!;

        public G3Pak_FileTableEntry(ReadBinary Read, ref long offset)
        {
            Header = new G3Pak_FileTableEntry_Header(Read, ref offset);
            if (Header.Attributes == 16)
            {
                DirectoryEntry = new G3Pak_DirectoryEntry(Read, ref offset);
            }
            else
            {
                FileEntry = new G3Pak_FileEntry(Read, ref offset);
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

        public int Extract(ReadBinary Read, string dest, bool overwrite)
        {
            if (Directory.Exists(dest) && !overwrite)
            {
                Logger.Log(string.Format("Warning: Directory named {0} already exists.\nConsider renaming the directory or using \"--overwrite\" option", Path.GetFileName(Path.GetDirectoryName(dest))));
                return 1;
            }

            Directory.CreateDirectory(dest);
            
            for (int i = 0; i < DirectoryEntry.DirCount; i++)
            {
                G3Pak_FileTableEntry _fileTableEntry = DirectoryEntry.DirTable[i];
                string FileName = string.Join("", _fileTableEntry.DirectoryEntry.FileName.Data);
                Directory.CreateDirectory(Path.Combine(dest, FileName));
                DirectoryEntry.DirTable[i].Extract(Read, dest, true); // Extract child folders
            }
            for (int i = 0; i < DirectoryEntry.FileCount; i++)
            {
                G3Pak_FileTableEntry _fileTableEntry = DirectoryEntry.FileTable[i];
                string FileName = string.Join("", _fileTableEntry.FileEntry.FileName.Data);

                int rawData_Bytes = (int)_fileTableEntry.FileEntry.Bytes;
                byte[] rawData = Read.Bytes(Convert.ToInt64(_fileTableEntry.FileEntry.Offset), rawData_Bytes);
                
                Logger.Log(string.Format("Extracting {0} ({1} bytes)", FileName, _fileTableEntry.FileEntry.Size));
                using (FileStream _fs = new FileStream(Path.Combine(dest, FileName), FileMode.OpenOrCreate))
                {
                    if (_fileTableEntry.FileEntry.Compression == 2) // Extract the compressed file
                    {
                        // Ensure the file has a valid zlib header
                        if (rawData[0] != 0x78) { throw new Exception("Incorrect zlib header"); }

                        using (MemoryStream input = new MemoryStream(rawData))
                        using (MemoryStream output = new MemoryStream())
                        using (ZLibStream decompressionStream = new ZLibStream(input, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(output);
                            rawData = output.ToArray();
                        }
                    }

                    _fs.Write(rawData);
                    _fs.Flush();
                }
            }
            return 0;
        }
    }
}
