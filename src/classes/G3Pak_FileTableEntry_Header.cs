namespace G3Archive
{
    public class G3Pak_FileTableEntry_Header
    {
        public UInt64 FileTime1;
        public UInt64 FileTime2;
        public UInt64 FileTime3;
        public UInt32 FileSizeHigh;
        public UInt32 FileSizeLow;
        public UInt32 Attributes;

        public G3Pak_FileTableEntry_Header(ReadBinary Read)
        {
            FileTime1 = Read.UInt64();
            FileTime2 = Read.UInt64();
            FileTime3 = Read.UInt64();
            FileSizeHigh = Read.UInt32();
            FileSizeLow = Read.UInt32();
            Attributes = Read.UInt32();
        }

        public G3Pak_FileTableEntry_Header(BinaryWriter bw, FileInfo File)
        {
            FileTime1 = (ulong)File.CreationTime.ToFileTimeUtc();
            FileTime2 = (ulong)File.LastAccessTime.ToFileTimeUtc();
            FileTime3 = (ulong)File.LastWriteTime.ToFileTimeUtc();
            FileSizeHigh = 0;
            FileSizeLow = 0;
            // Add G3PakFileAttribute_Packed attribute
            Attributes = (uint)File.Attributes + (uint)G3Pak_FileAttribute.Packed;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(FileTime1);
            bw.Write(FileTime2);
            bw.Write(FileTime3);
            bw.Write(FileSizeHigh);
            bw.Write(FileSizeLow);
            bw.Write(Attributes);
        }
    }
}
