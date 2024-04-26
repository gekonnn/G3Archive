namespace G3Archive
{
    public class G3Pak_DirectoryEntry
    {
        public readonly G3Pak_Archive Archive;

        public G3Pak_FileString FileName        = default!;
        public G3Pak_FileTableEntry[] DirTable  = default!;
        public UInt32 DirCount                  = default!;
        public G3Pak_FileTableEntry[] FileTable = default!;
        public UInt32 FileCount                 = default!;

        public G3Pak_DirectoryEntry(G3Pak_Archive Archive)
        {
            this.Archive = Archive;
        }

        public void ReadFromArchive()
        {
            FileName = new G3Pak_FileString(Archive);
            FileName.ReadFromArchive();

            DirCount = Archive.Reader.ReadUInt32();
            DirTable = new G3Pak_FileTableEntry[DirCount];
            for (int i_dir = 0; i_dir < DirCount; i_dir++)
            {
                G3Pak_FileTableEntry _fileTableEntry = new(Archive);
                _fileTableEntry.ReadFromArchive();

                DirTable[i_dir] = _fileTableEntry;
            }

            FileCount = Archive.Reader.ReadUInt32();
            FileTable = new G3Pak_FileTableEntry[FileCount];
            for (int i_file = 0; i_file < FileCount; i_file++)
            {
                G3Pak_FileTableEntry _fileTableEntry = new(Archive);
                _fileTableEntry.ReadFromArchive();

                FileTable[i_file] = _fileTableEntry;
            }
        }

        public void Write(FileInfo file, FileInfo RootDirectory)
        {
            string RelPath = Path.GetRelativePath(RootDirectory.FullName, file.FullName);
            if (RelPath != ".")
                FileName = new G3Pak_FileString(Archive, RelPath + "/");
            else
                FileName = new G3Pak_FileString(Archive, "");

            if ((file.Attributes & FileAttributes.Directory) > 0)
            {
                string[] FileEntries;
                string[] DirectoryEntries;

                if (Archive.Options.NoDeleted)
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
                    FileInfo Dir = new(DirectoryEntries[i]);

                    G3Pak_FileTableEntry _fileTableEntry = new(Archive);
                    _fileTableEntry.Write(Dir, RootDirectory);
                    DirTable[i] = _fileTableEntry;
                }

                for (int i = 0; i < FileCount; i++)
                {
                    FileInfo _File = new(FileEntries[i]);
                    Logger.Log(string.Format("Writing data for {0}", _File.Name));
                    uint OffsetToData = (uint)Archive.fs.Position;
                    
                    G3Pak_FileTableEntry _fileTableEntry = new(Archive);
                    _fileTableEntry.Write(_File, RootDirectory, OffsetToData);
                    _fileTableEntry.WriteData(OffsetToData);
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
