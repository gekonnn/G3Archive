﻿using System.Text;

namespace G3Archive
{
    public class G3Pak_Archive
    {
        public FileInfo? File;
        private G3Pak_ArchiveHeader Header = default!;
        private ReadBinary Read = default!;

        public void ReadArchive(FileInfo file)
        {
            File = file;
            Read = new ReadBinary(new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
            Header = new G3Pak_ArchiveHeader(Read);
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
                
                ulong OffsetToFiles = (ulong)bw.BaseStream.Position;
                ulong OffsetToFolders = OffsetToFiles;

                // Write file entries
                Logger.Log("Writing entries...");
                RootEntry.WriteEntry(bw);

                // Write offset to volume
                ulong OffsetToVolume = (ulong)bw.BaseStream.Position - 4;
                Header.WriteOffsets(bw, OffsetToFiles, OffsetToFolders, OffsetToVolume);
            }
            return 0;
        }

        public async Task<int> Extract(string dest, bool overwrite)
        {
            Read.fs.Seek((long)Header.OffsetToFiles, SeekOrigin.Begin);
            G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(Read);
            int result = RootEntry.ExtractDirectory(Read, dest, overwrite).Result;
            return result;
        }
    }
}
