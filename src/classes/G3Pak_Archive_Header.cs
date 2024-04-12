namespace G3Archive
{
    public class G3Pak_Archive_Header
    {
        private readonly uint magic = 0x30563347;
        
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

        public G3Pak_Archive_Header(BinaryReader br)
        {
            Version = br.ReadUInt32();
            Product = br.ReadUInt32();

            // Check if file is a valid G3Pak archive (G3V0) 
            if (Product != magic) throw new("Specified file is not an G3Pak archive.");

            Revision    = br.ReadUInt32();
            Encryption  = br.ReadUInt32();
            Compression = br.ReadUInt32();
            Reserved    = br.ReadUInt32();

            Pos_OffsetToFiles   = br.BaseStream.Position;
            OffsetToFiles       = br.ReadUInt64();
            Pos_OffsetToFolders = br.BaseStream.Position;
            OffsetToFolders     = br.ReadUInt64();
            Pos_OffsetToVolume  = br.BaseStream.Position;
            OffsetToVolume      = br.ReadUInt64();
        }

        public G3Pak_Archive_Header()
        {
            this.Version        = 0;
            this.Product        = magic;
            this.Revision       = 0;
            this.Encryption     = 0;
            this.Compression    = Options.Compression > 0 ? (uint)G3Pak_Compression.Auto : 0; // Set compression to Auto if not disabled
            this.Reserved       = 0;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Version);
            bw.Write(Product);
            bw.Write(Revision);
            bw.Write(Encryption);
            bw.Write(Compression);
            bw.Write(Reserved);

            Pos_OffsetToFiles   = bw.BaseStream.Position;
            bw.Write(OffsetToFiles);
            Pos_OffsetToFolders = bw.BaseStream.Position;
            bw.Write(OffsetToFolders);
            Pos_OffsetToVolume  = bw.BaseStream.Position;
            bw.Write(OffsetToVolume);
        }

        public void WriteOffsets(BinaryWriter bw, ulong OffsetToFiles, ulong OffsetToFolders, ulong OffsetToVolume)
        {
            long OriginalOffset = bw.BaseStream.Position;

            bw.BaseStream.Seek(Pos_OffsetToFiles, SeekOrigin.Begin);
            bw.Write(OffsetToFiles);
            this.OffsetToFiles      = OffsetToFiles;
            
            bw.BaseStream.Seek(Pos_OffsetToFolders, SeekOrigin.Begin);
            bw.Write(OffsetToFolders);
            this.OffsetToFolders    = OffsetToFolders;
            
            bw.BaseStream.Seek(Pos_OffsetToVolume, SeekOrigin.Begin);
            bw.Write(OffsetToVolume);
            this.OffsetToVolume     = OffsetToVolume;

            bw.BaseStream.Seek(OriginalOffset, SeekOrigin.Begin);
        }
    }
}
