using System.IO.Compression;

namespace G3Archive
{
    public class G3Pak_Archive
    {
        public string path = string.Empty;
        public string pak_fileName = string.Empty;
        public string pak_fileExt = string.Empty;

        private long currentOffset = 0;

        private G3Pak_Header Header;
        private ReadBinary Read;

        public G3Pak_Archive(FileInfo file)
        {
            path = file.FullName;
            pak_fileName = file.Name;
            pak_fileExt = file.Extension;
            Read = new ReadBinary(new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        // TODO: Move methods into their respective classes
        private G3Pak_FileTableEntry_Header Read_FileTableEntry_Header(ref long offset)
        {
            G3Pak_FileTableEntry_Header header = new G3Pak_FileTableEntry_Header();

            header.FileTime1    = Read.UInt64(ref offset);
            header.FileTime2    = Read.UInt64(ref offset);
            header.FileTime3    = Read.UInt64(ref offset);
            header.FileSizeHigh = Read.UInt32(ref offset);
            header.FileSizeLow  = Read.UInt32(ref offset);
            header.Attributes   = Read.Byte(ref offset);
            offset += 3;

            return header;
        }

        private G3Pak_FileString Read_FileString(ref long offset)
        {
            G3Pak_FileString fileString = new G3Pak_FileString();

            fileString.Length = Read.UInt32(ref offset);

            if (fileString.Length > 0)
            {
                fileString.Data = new Char[fileString.Length];
                for (int i = 0; i < fileString.Length; i++)
                {
                    fileString.Data[i] = Read.Char(ref offset);
                }
                offset++; // one empty byte after the string?
            }
            else
            {
                // assume it's root folder
                fileString.Data = "/".ToCharArray();
            }
            return fileString;
        }

        private G3Pak_FileEntry Read_FileEntry(ref long offset)
        {
            G3Pak_FileEntry fileEntry = new G3Pak_FileEntry();

            fileEntry.Offset        = Read.UInt64(ref offset);
            fileEntry.Bytes         = Read.UInt64(ref offset);
            fileEntry.Size          = Read.UInt64(ref offset);
            fileEntry.Encryption    = Read.UInt32(ref offset);
            fileEntry.Compression   = Read.UInt32(ref offset);
            fileEntry.FileName      = Read_FileString(ref offset);
            fileEntry.Comment       = Read_FileString(ref offset);

            return fileEntry;
        }

        private G3Pak_DirectoryEntry Read_DirectoryEntry(ref long offset)
        {
            G3Pak_DirectoryEntry directoryEntry = new G3Pak_DirectoryEntry();

            directoryEntry.FileName = Read_FileString(ref offset);

            directoryEntry.DirCount = Read.UInt32(ref offset);
            directoryEntry.DirTable = new G3Pak_FileTableEntry[directoryEntry.DirCount];
            for (int i_dir = 0; i_dir < directoryEntry.DirCount; i_dir++)
            {
                // Not really neccessary to do that in this case,
                // we're doing that just to push the currentOffset further.
                G3Pak_FileTableEntry _fileTableEntry = Read_FileTableEntry(ref offset);
                directoryEntry.DirTable[i_dir] = _fileTableEntry;
            }

            directoryEntry.FileCount = Read.UInt32(ref offset);
            directoryEntry.FileTable = new G3Pak_FileTableEntry[directoryEntry.FileCount];
            for (int i_file = 0; i_file < directoryEntry.FileCount; i_file++)
            {
                G3Pak_FileTableEntry _fileTableEntry = Read_FileTableEntry(ref offset);
                directoryEntry.FileTable[i_file] = _fileTableEntry;

            }

            return directoryEntry;
        }

        private G3Pak_FileTableEntry Read_FileTableEntry(ref long offset)
        {
            G3Pak_FileTableEntry fileTableEntry = new G3Pak_FileTableEntry();

            fileTableEntry.Header = Read_FileTableEntry_Header(ref offset);
            if (fileTableEntry.Header.Attributes == 16)
            {
                fileTableEntry.DirectoryEntry = Read_DirectoryEntry(ref offset);
            }
            else
            {
                fileTableEntry.FileEntry = Read_FileEntry(ref offset);
            }

            return fileTableEntry;
        }

        public void Read_PakHeader()
        {
            currentOffset = 0;

            // Set the header
            Header = new G3Pak_Header();
            Header.Version = Read.UInt32(ref currentOffset);
            Header.Product = Read.UInt32(ref currentOffset);

            // Check if file is a valid G3Pak archive (G3V0) 
            if (Header.Product != 810955591) { throw new Exception("Specified file is not an G3Pak archive."); }

            Header.Revision         = Read.UInt32(ref currentOffset);
            Header.Encryption       = Read.UInt32(ref currentOffset);
            Header.Compression      = Read.UInt32(ref currentOffset);
            Header.Reserved         = Read.UInt32(ref currentOffset);
            Header.OffsetToFiles    = Read.UInt64(ref currentOffset);
            Header.OffsetToFolders  = Read.UInt64(ref currentOffset);
            Header.OffsetToVolume   = Read.UInt64(ref currentOffset);
        }

        private void ExtractEntry(G3Pak_FileTableEntry fileTableEntry, string Dest_Path)
        {
            Directory.CreateDirectory(Dest_Path);
            for (int i = 0; i < fileTableEntry.DirectoryEntry.DirCount; i++)
            {
                G3Pak_FileTableEntry _fileTableEntry = fileTableEntry.DirectoryEntry.DirTable[i];
                string FileName = string.Join("", _fileTableEntry.DirectoryEntry.FileName.Data);
                Directory.CreateDirectory(Dest_Path + FileName);
                ExtractEntry(fileTableEntry.DirectoryEntry.DirTable[i], Dest_Path); // Extract child folders
            }
            for (int i = 0; i < fileTableEntry.DirectoryEntry.FileCount; i++)
            {
                G3Pak_FileTableEntry _fileTableEntry = fileTableEntry.DirectoryEntry.FileTable[i];
                string FileName = string.Join("", _fileTableEntry.FileEntry.FileName.Data);

                if (File.Exists(Dest_Path + FileName)) { File.Delete(Dest_Path + FileName); }

                int rawData_Bytes = (int)_fileTableEntry.FileEntry.Bytes;
                byte[] rawData = Read.Bytes(Convert.ToInt64(_fileTableEntry.FileEntry.Offset), rawData_Bytes);

                Console.WriteLine(string.Format("Extracting {0} ({1} bytes)", FileName, rawData_Bytes));
                using (FileStream _fs = new FileStream(Dest_Path + FileName, FileMode.CreateNew))
                {
                    if (_fileTableEntry.FileEntry.Compression == 2) // Extract the archive
                    {
                        // Make sure it has a zlib header
                        if (rawData[0] != 120) { throw new Exception("Incorrect zlib header"); }

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
        }

        public void ExtractArchive(string dest)
        {
            currentOffset = Convert.ToInt64(Header.OffsetToFiles);

            G3Pak_FileTableEntry RootEntry = Read_FileTableEntry(ref currentOffset);
            ExtractEntry(RootEntry, dest);
            while (currentOffset < Convert.ToInt64(Header.OffsetToVolume))
            {
                G3Pak_FileTableEntry fileTableEntry = Read_FileTableEntry(ref currentOffset);
                ExtractEntry(fileTableEntry, dest);
            }
        }
    }
}
