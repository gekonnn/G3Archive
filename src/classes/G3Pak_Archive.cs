namespace G3Archive
{
    public class G3Pak_Archive
    {
        public string path = string.Empty;
        public string pak_fileName = string.Empty;
        public string pak_fileExt = string.Empty;

        public G3Pak_Header Header;

        private FileStream fs;
        private BinaryReader br;

        ReadBinary Read = new ReadBinary();

        private long currentOffset;

        // TODO: Move methods into their respective classes
        private G3Pak_FileTableEntry_Header Read_FileTableEntry_Header(long offset)
        {
            G3Pak_FileTableEntry_Header header = new G3Pak_FileTableEntry_Header();

            header.FileTime1    = Read.UInt64(fs, br, ref currentOffset);
            header.FileTime2    = Read.UInt64(fs, br, ref currentOffset);
            header.FileTime3    = Read.UInt64(fs, br, ref currentOffset);
            header.FileSizeHigh = Read.UInt32(fs, br, ref currentOffset);
            header.FileSizeLow  = Read.UInt32(fs, br, ref currentOffset);
            header.Attributes   = Read.Byte(fs, br, ref currentOffset);
            currentOffset += 3;

            return header;
        }

        private G3Pak_FileString Read_FileString(long offset)
        {
            G3Pak_FileString fileString = new G3Pak_FileString();

            fileString.Length = Read.UInt32(fs, br, ref currentOffset);

            if (fileString.Length > 0)
            {
                fileString.Data = new Char[fileString.Length];
                for (int i = 0; i < fileString.Length; i++)
                {
                    fileString.Data[i] = Read.Char(fs, br, ref currentOffset);
                }
                currentOffset++; // one empty byte after the string?
            }
            else
            {
                // assume it's root folder
                fileString.Data = "/".ToCharArray();
            }
            return fileString;
        }

        private G3Pak_FileEntry Read_FileEntry(long offset)
        {
            G3Pak_FileEntry fileEntry = new G3Pak_FileEntry();

            fileEntry.Offset        = Read.UInt64(fs, br, ref currentOffset);
            fileEntry.Bytes         = Read.UInt64(fs, br, ref currentOffset);
            fileEntry.Size          = Read.UInt64(fs, br, ref currentOffset);
            fileEntry.Encryption    = Read.UInt32(fs, br, ref currentOffset);
            fileEntry.Compression   = Read.UInt32(fs, br, ref currentOffset);
            fileEntry.FileName      = Read_FileString(currentOffset);
            fileEntry.Comment       = Read_FileString(currentOffset);

            return fileEntry;
        }

        private G3Pak_DirectoryEntry Read_DirectoryEntry(long offset)
        {
            G3Pak_DirectoryEntry directoryEntry = new G3Pak_DirectoryEntry();

            directoryEntry.FileName = Read_FileString(currentOffset);

            directoryEntry.DirCount = Read.UInt32(fs, br, ref currentOffset);
            directoryEntry.DirTable = new G3Pak_FileTableEntry[directoryEntry.DirCount];
            for (int i_dir = 0; i_dir < directoryEntry.DirCount; i_dir++)
            {
                // Not really neccessary to do that in this case,
                // we're doing that just to push the currentOffset further.
                G3Pak_FileTableEntry _fileTableEntry = Read_FileTableEntry(currentOffset);
                directoryEntry.DirTable[i_dir] = _fileTableEntry;
            }

            directoryEntry.FileCount = Read.UInt32(fs, br, ref currentOffset);
            directoryEntry.FileTable = new G3Pak_FileTableEntry[directoryEntry.FileCount];
            for (int i_file = 0; i_file < directoryEntry.FileCount; i_file++)
            {
                G3Pak_FileTableEntry _fileTableEntry = Read_FileTableEntry(currentOffset);
                directoryEntry.FileTable[i_file] = _fileTableEntry;
            }

            return directoryEntry;
        }

        private G3Pak_FileTableEntry Read_FileTableEntry(long offset)
        {
            G3Pak_FileTableEntry fileTableEntry = new G3Pak_FileTableEntry();

            fileTableEntry.Header = Read_FileTableEntry_Header(currentOffset);
            if (fileTableEntry.Header.Attributes == 16)
            {
                fileTableEntry.DirectoryEntry = Read_DirectoryEntry(currentOffset);
            }
            else
            {
                fileTableEntry.FileEntry = Read_FileEntry(currentOffset);
            }

            return fileTableEntry;
        }

        public void Read_PakHeader()
        {
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);

            currentOffset = 0;

            // Set the header
            Header = new G3Pak_Header();
            Header.Version = Read.UInt32(fs, br, ref currentOffset);
            Header.Product = Read.UInt32(fs, br, ref currentOffset);

            if (Header.Product != 810955591) // Check if file is a valid G3Pak archive (G3V0)
            {
                throw new Exception("Specified file is not an G3Pak archive.");
            }

            Header.Revision         = Read.UInt32(fs, br, ref currentOffset);
            Header.Encryption       = Read.UInt32(fs, br, ref currentOffset);
            Header.Compression      = Read.UInt32(fs, br, ref currentOffset);
            Header.Reserved         = Read.UInt32(fs, br, ref currentOffset);
            Header.OffsetToFiles    = Read.UInt64(fs, br, ref currentOffset);
            Header.OffsetToFolders  = Read.UInt64(fs, br, ref currentOffset);
            Header.OffsetToVolume   = Read.UInt64(fs, br, ref currentOffset);
        }

        public void ExtractEntry(G3Pak_FileTableEntry fileTableEntry, string Dest_Path)
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

                if (File.Exists(Dest_Path + FileName))
                {
                    File.Delete(Dest_Path + FileName);
                }

                fs.Seek(Convert.ToInt64(_fileTableEntry.FileEntry.Offset), SeekOrigin.Begin);
                int rawData_Bytes = (int)_fileTableEntry.FileEntry.Bytes;
                byte[] rawData = br.ReadBytes(rawData_Bytes);

                Console.WriteLine(string.Format("Extracting {0} ({1} bytes)", FileName, rawData_Bytes));
                using (FileStream _fs = new FileStream(Dest_Path + FileName, FileMode.CreateNew))
                {
                    if (_fileTableEntry.FileEntry.Compression == 2) // Extract the archive
                    {
                        if (rawData[0] != 120) // Make sure it has a zlib header
                        {
                            throw new Exception("Incorrect zlib header");
                        }

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

        public void ExtractArchive()
        {
            currentOffset = Convert.ToInt64(Header.OffsetToFiles);
            string dest = Directory.GetCurrentDirectory() + "\\" + pak_fileName + "\\";
            
            G3Pak_FileTableEntry RootEntry = Read_FileTableEntry(currentOffset);
            ExtractEntry(RootEntry, dest);
            while (currentOffset < Convert.ToInt64(Header.OffsetToVolume))
            {
                G3Pak_FileTableEntry fileTableEntry = Read_FileTableEntry(currentOffset);
                ExtractEntry(fileTableEntry, dest);
            }
        }
    }
}
