using System.IO;
using System.Text;

namespace G3Archive
{
    public class G3Pak_Archive : IDisposable
    {
        public readonly FileInfo File          = default!;
        public readonly FileStream fs          = default!;
        public readonly BinaryReader Reader    = default!;
        public readonly BinaryWriter Writer    = default!;
        public G3Pak_Archive_Header Header     = default!;

        public ArchiveOptions Options = default!;

        public G3Pak_Archive() { }

        public G3Pak_Archive(string FilePath, string? Dest = null, ArchiveOptions? Options = null)
        {
            // Assign ArchiveOptions
            if (Options != null)
            {
                this.Options = Options;
            }
            else if (Dest != null)
            {
                this.Options = new() { Destination = Dest };
            }

            if (FilePath != null && Path.Exists(FilePath))
            {
                this.File = new FileInfo(FilePath);
                this.Header = new G3Pak_Archive_Header(this);

                // Determine whenever the file is a directory or a file
                // so we can initialize the FileStream based on that
                if (File.Attributes.HasFlag(FileAttributes.Directory))
                {
                    try
                    {
                        if (Path.Exists(this.Options.Destination) && !this.Options.Overwrite)
                        {
                            throw new(string.Format("File named {0} already exists.\nConsider renaming the file or using the \"--overwrite\" option.", File.Name));
                        }

                        if (FilePath == Directory.GetParent(this.Options.Destination)!.FullName)
                        {
                            throw new(string.Format("Destination path cannot be the same as packed folder's path"));
                        }

                        this.fs = new FileStream(this.Options.Destination, FileMode.OpenOrCreate, FileAccess.ReadWrite);

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
                    Header.Write();

                    // Create a root entry and write file data recursively
                    G3Pak_FileTableEntry RootEntry = new(this);
                    RootEntry.Write(Folder, Folder);

                    ulong OffsetToFiles = (ulong)fs.Position;
                    ulong OffsetToFolders = OffsetToFiles;

                    // Write file entries recursively from the root entry
                    Logger.Log("Writing entries...");
                    RootEntry.WriteEntry();

                    ulong ArchiveSize = (ulong)fs.Position;
                    ulong OffsetToVolume = ArchiveSize - 4;

                    // Finally write all offsets to the header
                    Header.WriteOffsets(OffsetToFiles, OffsetToFolders, OffsetToVolume);

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

        public async Task<bool> Extract(string? Dest = null, bool? Overwrite = null)
        {
            if (this.Reader != null)
            {
                Logger.Log("Extracting archive...");

                try
                {
                    Header.ReadFromArchive();

                    fs.Seek((long)Header.OffsetToFiles, SeekOrigin.Begin);

                    G3Pak_FileTableEntry RootEntry = new(this);
                    RootEntry.ReadFromArchive();

                    bool success = await RootEntry.ExtractDirectory(Dest ?? Options.Destination, Overwrite ?? Options.Overwrite);
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
