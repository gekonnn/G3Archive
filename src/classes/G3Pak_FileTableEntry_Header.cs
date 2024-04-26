namespace G3Archive
{
    public class G3Pak_FileTableEntry_Header
    {
        public readonly G3Pak_Archive Archive;

        public UInt64 FileTime1;
        public UInt64 FileTime2;
        public UInt64 FileTime3;
        public UInt32 FileSizeHigh;
        public UInt32 FileSizeLow;
        public UInt32 Attributes;

        public G3Pak_FileTableEntry_Header(G3Pak_Archive Archive)
        {
            this.Archive = Archive;
        }

        public void ReadFromArchive()
        {
            if (Archive != null)
            {
                FileTime1       = Archive.Reader.ReadUInt64();
                FileTime2       = Archive.Reader.ReadUInt64();
                FileTime3       = Archive.Reader.ReadUInt64();
                FileSizeHigh    = Archive.Reader.ReadUInt32();
                FileSizeLow     = Archive.Reader.ReadUInt32();
                Attributes      = Archive.Reader.ReadUInt32();
            }
            else
            {
                throw new InvalidOperationException("Archive not assigned");
            }
        }

        public void ReadFromFile(FileInfo File)
        {
            FileTime1       = (ulong)File.CreationTime.ToFileTimeUtc();
            FileTime2       = (ulong)File.LastAccessTime.ToFileTimeUtc();
            FileTime3       = (ulong)File.LastWriteTime.ToFileTimeUtc();
            FileSizeHigh    = 0;
            FileSizeLow     = 0;
            Attributes      = (uint)File.Attributes + (uint)G3Pak_FileAttribute.Packed; // Add G3PakFileAttribute_Packed attribute
        }

        public void Write()
        {
            Archive.Writer.Write(FileTime1);
            Archive.Writer.Write(FileTime2);
            Archive.Writer.Write(FileTime3);
            Archive.Writer.Write(FileSizeHigh);
            Archive.Writer.Write(FileSizeLow);
            Archive.Writer.Write(Attributes);
        }
    }
}
