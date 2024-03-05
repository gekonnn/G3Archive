﻿namespace G3Archive
{
    public class G3Pak_FileTableEntry_Header
    {
        public UInt64 FileTime1;
        public UInt64 FileTime2;
        public UInt64 FileTime3;
        public UInt32 FileSizeHigh;
        public UInt32 FileSizeLow;
        public UInt32 Attributes;

        public G3Pak_FileTableEntry_Header(ReadBinary Read, ref long offset)
        {
            FileTime1 = Read.UInt64(ref offset);
            FileTime2 = Read.UInt64(ref offset);
            FileTime3 = Read.UInt64(ref offset);
            FileSizeHigh = Read.UInt32(ref offset);
            FileSizeLow = Read.UInt32(ref offset);
            Attributes = Read.Byte(ref offset);
            offset += 3;
        }

        public G3Pak_FileTableEntry_Header(BinaryWriter bw, FileInfo File)
        {
            FileTime1 = (uint)File.CreationTime.ToFileTimeUtc();
            FileTime2 = (uint)File.LastAccessTime.ToFileTimeUtc();
            FileTime3 = (uint)File.LastWriteTime.ToFileTimeUtc();
            FileSizeHigh = 0;
            FileSizeLow = 0;
            Attributes = (uint)File.Attributes;
        }
    }
}
