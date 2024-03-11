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

        public G3Pak_FileEntry(ReadBinary Read)
        {
            Offset = Read.UInt64();
            Bytes = Read.UInt64();
            Size = Read.UInt64();
            Encryption = Read.UInt32();
            Compression = Read.UInt32();
            FileName = new G3Pak_FileString(Read);
            Comment = new G3Pak_FileString(Read);
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

        public void Write(BinaryWriter bw)
        {
            bw.Write(Offset);
            bw.Write(Bytes);
            bw.Write(Size);
            bw.Write(Encryption);
            bw.Write(Compression);
            FileName.Write(bw);
            Comment.Write(bw);
        }
    }
}
