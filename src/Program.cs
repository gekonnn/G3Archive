using CommandLine;
using System.Diagnostics;

namespace G3Archive
{
    class Program
    {
        static async Task Extract(string file)
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
                
                using (G3Pak_Archive Archive = new(file))
                {
                    bool success = await Archive.Extract(Options.Destination);
                    
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

        static void Pack(string DirectoryPath)
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

                using (G3Pak_Archive Archive = new(DirectoryPath, Options.Destination))
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
            .WithParsed<UnparsedOptions>(o =>
            {
                if (o.Path != null)
                {
                    Options.Path = o.Path;
                    Options.Extract = o.Extract;
                    Options.Pack = o.Pack;
                    Options.Destination = Path.GetFullPath((Path.Combine(o.Destination ?? Directory.GetCurrentDirectory(), "")));
                    Options.Compression = o.Compression;
                    Options.NoDecompress = o.NoDecompress;
                    Options.NoDeleted = o.NoDeleted;
                    Options.Overwrite = o.Overwrite;
                    Options.Verbose = !o.Quiet;

                    Logger.Enabled = Options.Verbose;

                    if (o.Extract)
                    {
                        if (Options.Destination == Directory.GetCurrentDirectory())
                        {
                            Options.Destination = Path.Combine(Options.Destination, Path.GetFileNameWithoutExtension(o.Path));
                        };

                        Extract(o.Path).Wait();
                    }

                    if (o.Pack)
                    {
                        if (Options.Destination == Directory.GetCurrentDirectory())
                        {
                            Options.Destination = Path.Combine(Options.Destination, o.Path + ".pak");
                        }

                        Pack(o.Path);
                    }
                }
            });
        }
    }
}