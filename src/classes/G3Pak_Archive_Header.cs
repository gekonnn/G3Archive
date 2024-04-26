namespace G3Archive
{
    public class G3Pak_Archive_Header
    {
        private static readonly uint magic = 0x30563347;

        private readonly G3Pak_Archive Archive;

        public UInt32 Version       = 0;
        public UInt32 Product       = magic;
        public UInt32 Revision      = 0;
        public UInt32 Encryption    = 0;
        public UInt32 Compression   = 0;
        public UInt32 Reserved      = 0;
        public UInt64 OffsetToFiles;
        public UInt64 OffsetToFolders;
        public UInt64 OffsetToVolume;

        public long Pos_OffsetToFiles;
        public long Pos_OffsetToFolders;
        public long Pos_OffsetToVolume;

        public G3Pak_Archive_Header(G3Pak_Archive archive)
        {
            this.Archive = archive;
        }

        public void ReadFromArchive()
        {
            Version = Archive.Reader.ReadUInt32();
            Product = Archive.Reader.ReadUInt32();

            // Check if file is a valid G3Pak archive (G3V0) 
            if (Product != magic) throw new("Specified file is not an G3Pak archive.");

            Revision    = Archive.Reader.ReadUInt32();
            Encryption  = Archive.Reader.ReadUInt32();
            Compression = Archive.Reader.ReadUInt32();
            Reserved    = Archive.Reader.ReadUInt32();

            Pos_OffsetToFiles   = Archive.Reader.BaseStream.Position;
            OffsetToFiles       = Archive.Reader.ReadUInt64();
            Pos_OffsetToFolders = Archive.Reader.BaseStream.Position;
            OffsetToFolders     = Archive.Reader.ReadUInt64();
            Pos_OffsetToVolume  = Archive.Reader.BaseStream.Position;
            OffsetToVolume      = Archive.Reader.ReadUInt64();
        }

        public void Write()
        {
            // Set compression to Auto if not disabled;
            Compression = Archive.Options.Compression > 0 ? (uint)G3Pak_Compression.Auto : 0;

            Archive.Writer.Write(Version);
            Archive.Writer.Write(Product);
            Archive.Writer.Write(Revision);
            Archive.Writer.Write(Encryption);
            Archive.Writer.Write(Compression);
            Archive.Writer.Write(Reserved);

            // Probably not neccessary as the positions are
            // always static, just to be safe though.
            Pos_OffsetToFiles   = Archive.fs.Position;
            Archive.Writer.Write(OffsetToFiles);
            Pos_OffsetToFolders = Archive.fs.Position;
            Archive.Writer.Write(OffsetToFolders);
            Pos_OffsetToVolume  = Archive.fs.Position;
            Archive.Writer.Write(OffsetToVolume);
        }

        public void WriteOffsets(ulong OffsetToFiles, ulong OffsetToFolders, ulong OffsetToVolume)
        {
            long OriginalOffset = Archive.fs.Position;

            // Seeking might not be actually neccessary in this case
            Archive.fs.Seek(Pos_OffsetToFiles, SeekOrigin.Begin);
            Archive.Writer.Write(OffsetToFiles);
            this.OffsetToFiles      = OffsetToFiles;

            Archive.fs.Seek(Pos_OffsetToFolders, SeekOrigin.Begin);
            Archive.Writer.Write(OffsetToFolders);
            this.OffsetToFolders    = OffsetToFolders;

            Archive.fs.Seek(Pos_OffsetToVolume, SeekOrigin.Begin);
            Archive.Writer.Write(OffsetToVolume);
            this.OffsetToVolume     = OffsetToVolume;

            // Return to the original offset before method call
            Archive.fs.Seek(OriginalOffset, SeekOrigin.Begin);
        }
    }
}
