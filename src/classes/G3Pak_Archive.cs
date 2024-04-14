using System.IO;
using System.Text;

namespace G3Archive
{
    public class G3Pak_Archive
    {
        public FileInfo? File;
        private FileStream fs               = default!;
        private BinaryReader Reader         = default!;
        private BinaryWriter Writer         = default!;
        private G3Pak_Archive_Header Header = default!;

        public void ReadArchive(FileInfo file)
        {
            try
            {
                File = file;
                fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                Reader = new BinaryReader(fs, Encoding.GetEncoding("iso-8859-1"));
                Header = new G3Pak_Archive_Header();
            }
            catch (Exception ex)
            {
                throw new($"An error has occured whilst reading {file.Name}: \n\"{ex.Message}\"");
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
                this.fs = new(File.FullName, FileMode.OpenOrCreate);
                this.Writer = new(fs, Encoding.GetEncoding("iso-8859-1"));

                Logger.Log("Writing header...");

                Header = new G3Pak_Archive_Header();
                Header.Write(Writer);

                G3Pak_FileTableEntry RootEntry = new(Writer, Folder, Folder);

                ulong OffsetToFiles = (ulong)fs.Position;
                ulong OffsetToFolders = OffsetToFiles;

                // Write file entries
                Logger.Log("Writing entries...");
                RootEntry.WriteEntry(Writer);

                ulong FileSize = (ulong)fs.Position;
                ulong OffsetToVolume = FileSize - 4;

                Header.WriteOffsets(Writer, OffsetToFiles, OffsetToFolders, OffsetToVolume);
                fs.SetLength((long)FileSize);

                return true;
            }
            catch (Exception ex)
            {
                throw new($"An error has occured whilst writing to {this.File!.Name}: \n\"{ex.Message}\"");
            }
            
        }

        public async Task<bool> Extract(string Dest)
        {
            try
            {
                if (File != null)
                {
                    Header.Read(Reader);
                    fs.Seek((long)Header.OffsetToFiles, SeekOrigin.Begin);
                    G3Pak_FileTableEntry RootEntry = new(Reader);
                    bool success = await RootEntry.ExtractDirectory(Reader, Dest, Options.Overwrite);
                    return success;
                }
                else
                {
                    throw new("Archive header not assigned");
                }
            }
            catch (Exception ex)
            {
                throw new($"An error has occured whilst extracting {this.File!.Name}: \n\"{ex.Message}\"");
            }
        }
    }
}
