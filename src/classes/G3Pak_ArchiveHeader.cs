namespace G3Archive
{
    public class G3Pak_ArchiveHeader
    {
        private readonly int magic = 0x30563347;
        public UInt32 Version;
        public UInt32 Product;
        public UInt32 Revision;
        public UInt32 Encryption;
        public UInt32 Compression;
        public UInt32 Reserved;
        public UInt64 OffsetToFiles;
        public UInt64 OffsetToFolders;
        public UInt64 OffsetToVolume;

        public long Pos_OffsetToFiles;
        public long Pos_OffsetToFolders;
        public long Pos_OffsetToVolume;

        public G3Pak_ArchiveHeader(ReadBinary Read)
        {
            Version = Read.UInt32();
            Product = Read.UInt32();

            // Check if file is a valid G3Pak archive (G3V0) 
            if (Product != magic) { throw new Exception("Specified file is not an G3Pak archive."); }

            Revision = Read.UInt32();
            Encryption = Read.UInt32();
            Compression = Read.UInt32();
            Reserved = Read.UInt32();

            Pos_OffsetToFiles = Read.fs.Position;
            OffsetToFiles = Read.UInt64();
            Pos_OffsetToFolders = Read.fs.Position;
            OffsetToFolders = Read.UInt64();
            Pos_OffsetToVolume = Read.fs.Position;
            OffsetToVolume = Read.UInt64();
        }

        public G3Pak_ArchiveHeader(BinaryWriter bw, uint Version = 0, uint Revision = 0, uint Encryption = 0, uint Compression = 0, uint Reserved = 0)
        {
            this.Version = Version;
            this.Product = (uint)magic;
            this.Revision = Revision;
            this.Encryption = Encryption;
            this.Compression = Compression;
            this.Reserved = Reserved;

            bw.Write(Version);
            bw.Write(Product);
            bw.Write(Revision);
            bw.Write(Encryption);
            bw.Write(Compression);
            bw.Write(Reserved);

            Pos_OffsetToFiles = bw.BaseStream.Position;
            bw.Write(OffsetToFiles);
            Pos_OffsetToFolders = bw.BaseStream.Position;
            bw.Write(OffsetToFolders);
            Pos_OffsetToVolume = bw.BaseStream.Position;
            bw.Write(OffsetToVolume);
        }

        public void WriteOffsets(BinaryWriter bw, ulong OffsetToFiles, ulong OffsetToFolders, ulong OffsetToVolume)
        {
            long OriginalOffset = bw.BaseStream.Position;

            bw.Seek((int)Pos_OffsetToFiles, SeekOrigin.Begin);
            bw.Write(OffsetToFiles);
            this.OffsetToFiles = OffsetToFiles;
            bw.Seek((int)Pos_OffsetToFolders, SeekOrigin.Begin);
            bw.Write(OffsetToFolders);
            this.OffsetToFolders = OffsetToFolders;
            bw.Seek((int)Pos_OffsetToVolume, SeekOrigin.Begin);
            bw.Write(OffsetToVolume);
            this.OffsetToVolume = OffsetToVolume;

            bw.Seek((int)OriginalOffset, SeekOrigin.Begin);
        }
    }
}
