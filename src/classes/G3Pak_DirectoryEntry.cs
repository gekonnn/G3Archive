﻿namespace G3Archive
{
    public class G3Pak_DirectoryEntry
    {
        public G3Pak_FileString FileName;
        public G3Pak_FileTableEntry[] DirTable;
        public UInt32 DirCount;
        public G3Pak_FileTableEntry[] FileTable;
        public UInt32 FileCount;

        public G3Pak_DirectoryEntry(ReadBinary Read, ref long offset)
        {
            FileName = new G3Pak_FileString(Read, ref offset);

            DirCount = Read.UInt32(ref offset);
            DirTable = new G3Pak_FileTableEntry[DirCount];
            for (int i_dir = 0; i_dir < DirCount; i_dir++)
            {
                G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(Read, ref offset);
                DirTable[i_dir] = _fileTableEntry;
            }

            FileCount = Read.UInt32(ref offset);
            FileTable = new G3Pak_FileTableEntry[FileCount];
            for (int i_file = 0; i_file < FileCount; i_file++)
            {
                G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(Read, ref offset);
                FileTable[i_file] = _fileTableEntry;
            }
        }

        public G3Pak_DirectoryEntry(BinaryWriter bw, FileInfo file, FileInfo RootDirectory)
        {
            string RelPath = Path.GetRelativePath(RootDirectory.FullName, file.FullName);
            if (RelPath != ".")
            {
                FileName = new G3Pak_FileString(bw, RelPath);
            }
            else
            {
                FileName = new G3Pak_FileString(bw, "");
            }

            if ((file.Attributes & FileAttributes.Directory) > 0)
            {
                string[] FileEntries = Directory.GetFiles(file.FullName);
                string[] DirectoryEntries = Directory.GetDirectories(file.FullName);
                
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
                    bw.Write(File.ReadAllBytes(_File.FullName));
                    
                    G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(bw, _File, RootDirectory, OffsetToData);
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
