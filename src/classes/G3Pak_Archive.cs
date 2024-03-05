using System.Text;

namespace G3Archive
{
    public class G3Pak_Archive
    {
        public FileInfo? File;
        private long currentOffset = 0;

        private G3Pak_ArchiveHeader Header = default!;
        private ReadBinary Read = default!;

        public void ReadArchive(FileInfo file)
        {
            File = file;
            Read = new ReadBinary(new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
            Header = new G3Pak_ArchiveHeader(Read, ref currentOffset);
        }

        public int WriteArchive(FileInfo file, string dest, bool overwrite)
        {
            File = new FileInfo(dest);

            if (File.Exists && !overwrite)
            {
                Logger.Log(string.Format("Warning: File named {0} already exists.\nConsider renaming the file or using the \"--overwrite\" option.", File.Name));
                return 1;
            }

            if (File.DirectoryName != null) { Directory.CreateDirectory(File.DirectoryName); }
            using (FileStream fs = new FileStream(File.FullName, FileMode.OpenOrCreate))
            {
                Logger.Log("Writing header...");
                BinaryWriter bw = new BinaryWriter(fs, Encoding.GetEncoding("iso-8859-1"));
                Header = new G3Pak_ArchiveHeader(bw);
                
                G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(bw, file, file);
                
                uint OffsetToFiles = (uint)bw.BaseStream.Position;
                uint OffsetToFolders = OffsetToFiles;

                // Write file entries
                Logger.Log("Writing entries...");
                RootEntry.WriteEntry(bw);

                // Write offset to volume
                uint OffsetToVolume = (uint)bw.BaseStream.Position - 4;
                Header.WriteOffsets(bw, OffsetToFiles, OffsetToFolders, OffsetToVolume);
            }
            return 0;
        }

        public int Extract(string dest, bool overwrite)
        {
            currentOffset = Convert.ToInt64(Header.OffsetToFiles);
            G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(Read, ref currentOffset);
            int result = RootEntry.Extract(Read, dest, overwrite);
            return result;
        }
    }
}
