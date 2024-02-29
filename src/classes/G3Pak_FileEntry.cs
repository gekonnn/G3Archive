namespace G3Archive
{
    public class G3Pak_FileEntry
    {
        public UInt64 Offset;
        public UInt64 Bytes;
        public UInt64 Size;
        public UInt32 Encryption;
        public UInt32 Compression;
        public G3Pak_FileString FileName;
        public G3Pak_FileString Comment;

        public G3Pak_FileEntry(ReadBinary Read, ref long offset)
        {
            Offset = Read.UInt64(ref offset);
            Bytes = Read.UInt64(ref offset);
            Size = Read.UInt64(ref offset);
            Encryption = Read.UInt32(ref offset);
            Compression = Read.UInt32(ref offset);
            FileName = new G3Pak_FileString(Read, ref offset);
            Comment = new G3Pak_FileString(Read, ref offset);
        }
    }
}
