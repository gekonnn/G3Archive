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
        public FileInfo? EntryFile;

        public G3Pak_FileEntry(BinaryReader br)
        {
            Offset      = br.ReadUInt64();
            Bytes       = br.ReadUInt64();
            Size        = br.ReadUInt64();
            Encryption  = br.ReadUInt32();
            Compression = br.ReadUInt32();
            FileName    = new G3Pak_FileString(br);
            Comment     = new G3Pak_FileString(br);
        }

        public G3Pak_FileEntry(BinaryWriter bw, FileInfo file, FileInfo RootDirectory, UInt64 OffsetToFiles, uint Compression = 0, string Comment = "")
        {
            this.Offset         = OffsetToFiles;
            this.Bytes          = (uint)file.Length;
            this.Size           = (uint)File.ReadAllBytes(file.FullName).Length;
            this.Encryption     = 0;
            this.Compression    = Compression;
            this.FileName       = new G3Pak_FileString(bw, Path.GetRelativePath(RootDirectory.FullName, file.FullName));
            this.Comment        = new G3Pak_FileString(bw, Comment);
            this.EntryFile      = file;
        }

        public byte[] ReadBytes()
        {
            if (EntryFile != null)
            {
                return File.ReadAllBytes(EntryFile.FullName);
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        public void WriteEntry(BinaryWriter bw)
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
