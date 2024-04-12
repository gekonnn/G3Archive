using System.IO;
using System.Text;

namespace G3Archive
{
    public class G3Pak_Archive
    {
        public FileInfo? File;
        private BinaryReader br             = default!;
        private FileStream fs               = default!;
        private G3Pak_Archive_Header Header = default!;

        public void ReadArchive(FileInfo file)
        {
            try
            {
                File = file;
                fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                br = new BinaryReader(fs, Encoding.GetEncoding("iso-8859-1"));
                Header = new G3Pak_Archive_Header(br);
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

            if (File.DirectoryName != null) Directory.CreateDirectory(File.DirectoryName);

            try
            {
                using (FileStream _fs = new FileStream(File.FullName, FileMode.OpenOrCreate))
                {
                    Logger.Log("Writing header...");

                    BinaryWriter _bw = new BinaryWriter(_fs, Encoding.GetEncoding("iso-8859-1"));

                    Header = new G3Pak_Archive_Header();
                    Header.Write(_bw);

                    G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(_bw, Folder, Folder);

                    ulong OffsetToFiles = (ulong)_fs.Position;
                    ulong OffsetToFolders = OffsetToFiles;

                    // Write file entries
                    Logger.Log("Writing entries...");
                    RootEntry.WriteEntry(_bw);

                    ulong FileSize = (ulong)_fs.Position;
                    ulong OffsetToVolume = FileSize - 4;

                    Header.WriteOffsets(_bw, OffsetToFiles, OffsetToFolders, OffsetToVolume);

                    _fs.SetLength((long)FileSize);
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
                    fs.Seek((long)Header.OffsetToFiles, SeekOrigin.Begin);
                    G3Pak_FileTableEntry RootEntry = new G3Pak_FileTableEntry(br);
                    bool success = await RootEntry.ExtractDirectory(br, Dest, Options.Overwrite);
                    return success;
                }
                else
                {
                    throw new Exception("Archive header not assigned");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error has occured whilst extracting {this.File!.Name}: \n\"{ex.Message}\"");
            }
        }
    }
}
