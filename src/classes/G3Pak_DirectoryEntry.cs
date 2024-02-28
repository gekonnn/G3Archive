namespace G3Archive
{
    public struct G3Pak_DirectoryEntry
    {
        public G3Pak_FileString FileName;
        public G3Pak_FileTableEntry[] DirTable;
        public UInt32 DirCount;
        public G3Pak_FileTableEntry[] FileTable;
        public UInt32 FileCount;
    }
}
