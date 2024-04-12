using System.IO;
using System.Text;

namespace G3Archive
{
    public class G3Pak_Archive
    {
        public FileInfo? File;
        private G3Pak_Archive_Header Header = default!;
        private ReadBinary Read = default!;

        public void ReadArchive(FileInfo file)
        {
            try
            {
                File = file;
                Read = new ReadBinary(new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
                Header = new G3Pak_Archive_Header(Read);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error has occured whilst reading {file.Name}: \n\"{ex.Message}\"");
            }

        }

        public bool WriteArchive(FileInfo Folder, string Dest)
        {
            File = new FileInfo(Dest);

            if (File.FullName == Folder.FullName)
            {
                Logger.Log(string.Format("Warning: Destination path cannot be the same as packed folder's path"));
                return false;
            }

            if (File.Exists && !Options.Overwrite)
            {
                Logger.Log(string.Format("Warning: File named {0} already exists.\nConsider renaming the file or using the \"--overwrite\" option.", File.Name));
                return false;
            }

            if (File.DirectoryName != null) { Directory.CreateDirectory(File.DirectoryName); }
            try
            {
                using (FileStream fs = new FileStream(File.FullName, FileMode.OpenOrCreate))
                {
                    Logger.Log("Writing header...");
                    BinaryWriter bw = new BinaryWriter(fs, Encoding.GetEncoding("iso-8859-1"));

                    Header = new G3Pak_Archive_Header();
                    Header.Write(bw);

                    G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(bw, Folder, Folder);

                    ulong OffsetToFiles = (ulong)bw.BaseStream.Position;
                    ulong OffsetToFolders = OffsetToFiles;

                    // Write file entries
                    Logger.Log("Writing entries...");
                    RootEntry.WriteEntry(bw);

                    ulong FileSize = (ulong)bw.BaseStream.Position;
                    ulong OffsetToVolume = FileSize - 4; // Write offset to volume

                    Header.WriteOffsets(bw, OffsetToFiles, OffsetToFolders, OffsetToVolume);

                    fs.SetLength((long)FileSize);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error has occured whilst writing to {this.File!.Name}: \n\"{ex.Message}\"");
            }
            
        }

        public async Task<bool> Extract(string Dest)
        {
            try
            {
                if (File != null)
                {
                    Read.fs.Seek((long)Header.OffsetToFiles, SeekOrigin.Begin);
                    G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(Read);
                    bool success = await RootEntry.ExtractDirectory(Read, Dest, Options.Overwrite);
                    return success;
                }
                else
                {
                    throw new ArgumentNullException("Archive header not assigned");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error has occured whilst extracting {this.File!.Name}: \n\"{ex.Message}\"");
            }
        }
    }
}
