using System.IO;
using System.Text;

namespace G3Archive
{
    public class G3Pak_Archive : IDisposable
    {
        public  readonly FileInfo File          = default!;
        private readonly FileStream fs          = default!;
        private readonly BinaryReader Reader    = default!;
        private readonly BinaryWriter Writer    = default!;
        private G3Pak_Archive_Header Header     = default!;

        public G3Pak_Archive() { }

        public G3Pak_Archive(string FilePath, string? Dest = null)
        {
            if (FilePath != null && Path.Exists(FilePath))
            {
                this.File = new FileInfo(FilePath);
                this.Header = new G3Pak_Archive_Header();

                // Determine whenever the file is a directory or a file
                // so we can initialize the FileStream based on that
                if (File.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (Dest == null) return;
                    try
                    {
                        if (Path.Exists(Dest) && !Options.Overwrite)
                        {
                            throw new(string.Format("File named {0} already exists.\nConsider renaming the file or using the \"--overwrite\" option.", File.Name));
                        }

                        if (FilePath == Directory.GetParent(Dest)!.FullName)
                        {
                            throw new(string.Format("Destination path cannot be the same as packed folder's path"));
                        }

                        this.fs = new FileStream(Dest, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    }
                    catch (Exception ex)
                    {
                        throw new($"An error has occured creating archive file:\n{ex.Message}");
                    }
                }
                else
                {
                    this.fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
                }

                this.Reader = new BinaryReader(this.fs, Encoding.GetEncoding("iso-8859-1"));
                this.Writer = new BinaryWriter(this.fs, Encoding.GetEncoding("iso-8859-1"));
            }
        }

        public bool WriteArchive(string FolderPath)
        {
            if (this.Writer != null)
            {
                FileInfo Folder = new(FolderPath);

                Logger.Log("Writing header...");

                try
                {
                    Header = new G3Pak_Archive_Header();
                    Header.Write(Writer);

                    // Create a root entry and write file data recursively
                    G3Pak_FileTableEntry RootEntry = new(Writer, Folder, Folder);

                    ulong OffsetToFiles = (ulong)fs.Position;
                    ulong OffsetToFolders = OffsetToFiles;

                    // Write file entries recursively from the root entry
                    Logger.Log("Writing entries...");
                    RootEntry.WriteEntry(Writer);

                    ulong ArchiveSize = (ulong)fs.Position;
                    ulong OffsetToVolume = ArchiveSize - 4;

                    // Finally write all offsets to the header
                    Header.WriteOffsets(Writer, OffsetToFiles, OffsetToFolders, OffsetToVolume);

                    fs.SetLength((long)ArchiveSize);  // Crop the file to the current position (effective only when overwriting)

                    return true;
                }
                catch (Exception ex)
                {
                    throw new($"An error has occured whilst writing to {this.File!.Name}: \n\"{ex.Message}\"");
                }
            }
            else
            {
                throw new("Archive writer not assigned");
            }
        }

        public async Task<bool> Extract(string Dest)
        {
            if (this.Reader != null)
            {
                Logger.Log("Extracting archive...");

                try
                {
                    Header.Read(Reader);

                    fs.Seek((long)Header.OffsetToFiles, SeekOrigin.Begin);

                    G3Pak_FileTableEntry RootEntry = new(Reader);
                    bool success = await RootEntry.ExtractDirectory(Reader, Dest, Options.Overwrite);
                    return success;
                }
                catch (Exception ex)
                {
                    throw new($"An error has occured whilst extracting {this.File!.Name}: \n\"{ex.Message}\"");
                }
            }
            else
            {
                throw new("Archive reader not assigned");
            }
        }

        public void Dispose()
        {
            if (fs != null) // Ensure we even have anything to close
            {
                this.fs.Flush();
                this.fs.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
