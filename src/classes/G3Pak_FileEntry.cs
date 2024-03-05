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

        public G3Pak_FileEntry(BinaryWriter bw, FileInfo file, FileInfo RootDirectory, UInt64 OffsetToFiles, string Comment = "")
        {
            this.Offset = OffsetToFiles;
            this.Bytes = (uint)file.Length;
            this.Size = (uint)File.ReadAllBytes(file.FullName).Length;
            this.Encryption = 0;
            this.Compression = 0; // TODO: compression support
            this.FileName = new G3Pak_FileString(bw, Path.GetRelativePath(RootDirectory.FullName, file.FullName));
            this.Comment = new G3Pak_FileString(bw, Comment);
        }
    }
}
