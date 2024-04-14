using CommandLine;
using System.Diagnostics;

namespace G3Archive
{
    class Program
    {
        static async Task Extract(FileInfo file)
        {
            if (!File.Exists(file.FullName))
            {
                Logger.Log("Specified file does not exist");
                return;
            }

            try
            {
                Stopwatch sw = new();
                sw.Start();

                Logger.Log("Reading archive header...");
                G3Pak_Archive PakFile = new(file);
                Logger.Log("Extracting archive...");
                bool success = await PakFile.Extract(Options.Destination);
                PakFile.Close();

                sw.Stop();
                if (success)
                    Logger.Log(string.Format("{0} extracted successfully. (Time: {1})", PakFile.File!.Name, sw.Elapsed));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return;
            }
        }

        static void Pack(FileInfo directory)
        {
            if (!Directory.Exists(directory.FullName))
            {
                Logger.Log("Specified directory does not exist");
                return;
            }

            try
            {
                Stopwatch sw = new();
                sw.Start();

                G3Pak_Archive PakFile = new(directory, Options.Destination);
                bool success = PakFile.WriteArchive(directory);
                PakFile.Close();

                sw.Stop();
                if (success)
                    Logger.Log(string.Format("{0} packed successfully. (Time: {1})", PakFile.File!.Name, sw.Elapsed));
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
                    Options.Quiet = o.Quiet;

                    Logger.Enabled = !Options.Quiet;

                    if (o.Extract)
                    {
                        if (Options.Destination == Directory.GetCurrentDirectory())
                        {
                            Options.Destination = Path.Combine(Options.Destination, Path.GetFileNameWithoutExtension(o.Path.FullName));
                        };

                        Extract(o.Path).Wait();
                    }

                    if (o.Pack)
                    {
                        if (Options.Destination == Directory.GetCurrentDirectory())
                        {
                            Options.Destination = Path.Combine(Options.Destination, o.Path.Name + ".pak");
                        }

                        Pack(o.Path);
                    }
                }
            });
        }
    }
}