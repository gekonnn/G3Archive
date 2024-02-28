namespace G3Archive
{
    public struct G3Pak_FileTableEntry_Header
    {
        public UInt64 FileTime1;
        public UInt64 FileTime2;
        public UInt64 FileTime3;
        public UInt32 FileSizeHigh;
        public UInt32 FileSizeLow;
        public UInt32 Attributes;
    }
}
