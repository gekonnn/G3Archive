namespace G3Archive
{
    public class G3Pak_Archive
    {
        public string path = string.Empty;
        public string pak_fileName = string.Empty;
        public string pak_fileExt = string.Empty;

        private long currentOffset = 0;

        private G3Pak_ArchiveHeader Header;
        private ReadBinary Read;

        public G3Pak_Archive(FileInfo file)
        {
            path = file.FullName;
            pak_fileName = file.Name;
            pak_fileExt = file.Extension;
            
            Read = new ReadBinary(new FileStream(path, FileMode.Open, FileAccess.Read));
            Header = new G3Pak_ArchiveHeader(Read, ref currentOffset);
        }

        public void ExtractArchive(string dest)
        {
            currentOffset = Convert.ToInt64(Header.OffsetToFiles);
            G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(Read, ref currentOffset);
            RootEntry.Extract(Read, dest);
        }
    }
}
