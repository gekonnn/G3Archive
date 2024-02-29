using System.Reflection.PortableExecutable;

namespace G3Archive
{
    public class G3Pak_ArchiveHeader
    {
        public UInt32 Version;
        public UInt32 Product;
        public UInt32 Revision;
        public UInt32 Encryption;
        public UInt32 Compression;
        public UInt32 Reserved;
        public UInt64 OffsetToFiles;
        public UInt64 OffsetToFolders;
        public UInt64 OffsetToVolume;

        public G3Pak_ArchiveHeader(ReadBinary Read, ref long offset)
        {
            // Set the header
            Version = Read.UInt32(ref offset);
            Product = Read.UInt32(ref offset);

            // Check if file is a valid G3Pak archive (G3V0) 
            if (Product != 810955591) { throw new Exception("Specified file is not an G3Pak archive."); }

            Revision = Read.UInt32(ref offset);
            Encryption = Read.UInt32(ref offset);
            Compression = Read.UInt32(ref offset);
            Reserved = Read.UInt32(ref offset);
            OffsetToFiles = Read.UInt64(ref offset);
            OffsetToFolders = Read.UInt64(ref offset);
            OffsetToVolume = Read.UInt64(ref offset);
        }
    }
}
