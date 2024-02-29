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

        public void Extract(ReadBinary Read, string Dest_Path)
        {
            Directory.CreateDirectory(Dest_Path);
            for (int i = 0; i < DirectoryEntry.DirCount; i++)
            {
                G3Pak_FileTableEntry _fileTableEntry = DirectoryEntry.DirTable[i];
                string FileName = string.Join("", _fileTableEntry.DirectoryEntry.FileName.Data);
                Directory.CreateDirectory(Dest_Path + FileName);
                DirectoryEntry.DirTable[i].Extract(Read, Dest_Path); // Extract child folders
            }
            for (int i = 0; i < DirectoryEntry.FileCount; i++)
            {
                G3Pak_FileTableEntry _fileTableEntry = DirectoryEntry.FileTable[i];
                string FileName = string.Join("", _fileTableEntry.FileEntry.FileName.Data);

                if (File.Exists(Dest_Path + FileName)) { File.Delete(Dest_Path + FileName); }

                int rawData_Bytes = (int)_fileTableEntry.FileEntry.Bytes;
                byte[] rawData = Read.Bytes(Convert.ToInt64(_fileTableEntry.FileEntry.Offset), rawData_Bytes);

                Console.WriteLine(string.Format("Extracting {0} ({1} bytes)", FileName, _fileTableEntry.FileEntry.Size));
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
    }
}
