namespace G3Archive
{
    public class G3Pak_DirectoryEntry
    {
        public G3Pak_FileString FileName;
        public G3Pak_FileTableEntry[] DirTable;
        public UInt32 DirCount;
        public G3Pak_FileTableEntry[] FileTable;
        public UInt32 FileCount;

        public G3Pak_DirectoryEntry(ReadBinary Read)
        {
            FileName = new G3Pak_FileString(Read);

            DirCount = Read.UInt32();
            DirTable = new G3Pak_FileTableEntry[DirCount];
            for (int i_dir = 0; i_dir < DirCount; i_dir++)
            {
                G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(Read);
                DirTable[i_dir] = _fileTableEntry;
            }

            FileCount = Read.UInt32();
            FileTable = new G3Pak_FileTableEntry[FileCount];
            for (int i_file = 0; i_file < FileCount; i_file++)
            {
                G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(Read);
                FileTable[i_file] = _fileTableEntry;
            }
        }

        public G3Pak_DirectoryEntry(BinaryWriter bw, FileInfo file, FileInfo RootDirectory)
        {
            string RelPath = Path.GetRelativePath(RootDirectory.FullName, file.FullName);
            if (RelPath != ".")
            {
                FileName = new G3Pak_FileString(bw, RelPath + "/");
            }
            else
            {
                FileName = new G3Pak_FileString(bw, "");
            }

            if ((file.Attributes & FileAttributes.Directory) > 0)
            {
                string[] FileEntries;
                string[] DirectoryEntries;

                if (Options.NoDeleted)
                {
                    FileEntries = Directory.GetFiles(file.FullName).Where(x => !x.Contains("_deleted")).ToArray();
                    DirectoryEntries = Directory.GetDirectories(file.FullName).Where(x => !x.Contains("_deleted")).ToArray();
                }
                else
                {
                    FileEntries = Directory.GetFiles(file.FullName);
                    DirectoryEntries = Directory.GetDirectories(file.FullName);
                }
                
                DirCount = (uint)DirectoryEntries.Length;
                DirTable = new G3Pak_FileTableEntry[DirCount];
                
                FileCount = (uint)FileEntries.Length;
                FileTable = new G3Pak_FileTableEntry[FileCount];

                for(int i = 0; i < DirCount; i++)
                {
                    FileInfo Dir = new FileInfo(DirectoryEntries[i]);
                    G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(bw, Dir, RootDirectory);
                    DirTable[i] = _fileTableEntry;
                }

                for (int i = 0; i < FileCount; i++)
                {
                    FileInfo _File = new FileInfo(FileEntries[i]);
                    Logger.Log(string.Format("Writing data for {0}", _File.Name));
                    uint OffsetToData = (uint)bw.BaseStream.Position;
                    
                    G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(bw, _File, RootDirectory, OffsetToData);
                    _fileTableEntry.WriteData(bw, OffsetToData);
                    FileTable[i] = _fileTableEntry;
                }
            }
            else
            {
                DirCount = 0;
                DirTable = new G3Pak_FileTableEntry[DirCount];
                FileCount = 0;
                FileTable = new G3Pak_FileTableEntry[FileCount];
            }
        }
    }
}
