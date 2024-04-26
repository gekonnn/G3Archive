using CommandLine;
using System.Diagnostics;

namespace G3Archive
{
    class Program
    {
        static async Task Extract(string file, ArchiveOptions options)
        {
            if (!File.Exists(file))
            {
                Logger.Log("Specified file does not exist");
                return;
            }

            try
            {
                Stopwatch sw = new();
                sw.Start();
                
                using (G3Pak_Archive Archive = new(file, Options: options))
                {
                    bool success = await Archive.Extract();
                    
                    if (success)
                        Logger.Log(string.Format("{0} extracted successfully. (Time: {1})", Path.GetFileName(file), sw.Elapsed));
                }

                sw.Stop();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return;
            }
        }

        static void Pack(string DirectoryPath, ArchiveOptions options)
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Logger.Log("Specified directory does not exist");
                return;
            }

            try
            {
                Stopwatch sw = new();
                sw.Start();

                using (G3Pak_Archive Archive = new(DirectoryPath, Options: options))
                {
                    bool success = Archive.WriteArchive(DirectoryPath);
                    
                    if (success)
                        Logger.Log(string.Format("{0} packed successfully. (Time: {1})", DirectoryPath, sw.Elapsed));
                }

                sw.Stop();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                if (File.Exists(args[0]))
                {
                    args = new[] { args[0], "-e" };
                }
                else if (Directory.Exists(args[0]))
                {
                    args = new[] { args[0], "-p" };
                }
            }
            else if (args.Length == 0)
            {
                args = new[] { "--help" };
            }

            Parser.Default.ParseArguments<UnparsedOptions>(args)
            .WithParsed(o =>
            {
                if (o.Path != null)
                {
                    ArchiveOptions archiveOptions = new()
                    {
                        Destination     = Path.GetFullPath((Path.Combine(o.Destination ?? Directory.GetCurrentDirectory(), ""))),
                        Compression     = o.Compression,
                        NoDecompress    = o.NoDecompress,
                        NoDeleted       = o.NoDeleted,
                        Overwrite       = o.Overwrite,
                    };

                    Logger.Enabled = !o.Quiet;

                    if (o.Extract)
                    {
                        if (archiveOptions.Destination == Directory.GetCurrentDirectory())
                        {
                            archiveOptions.Destination = Path.Combine(archiveOptions.Destination, Path.GetFileNameWithoutExtension(o.Path));
                        };

                        Extract(o.Path, archiveOptions).Wait();
                    }

                    if (o.Pack)
                    {
                        if (archiveOptions.Destination == Directory.GetCurrentDirectory())
                        {
                            archiveOptions.Destination = Path.Combine(archiveOptions.Destination, o.Path + ".pak");
                        }

                        Pack(o.Path, archiveOptions);
                    }
                }
            });
        }
    }
}