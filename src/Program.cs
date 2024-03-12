using CommandLine;
using System.Diagnostics;

namespace G3Archive
{
    class Program
    {
        public class Options
        {
            [Option('e', "extract", Required = false, HelpText = "Extracts the selected archive")]
            public FileInfo? Extract { get; set; }
            [Option('p', "pack", Required = false, HelpText = "Packages the selected folder into a PAK archive")]
            public FileInfo? Pack { get; set; }
            [Option('d', "dest", Required = false, HelpText = "Specifies path of output file")]
            public string? Destination { get; set; }
            [Option("overwrite", Required = false, Default = false, HelpText = "Forces overwriting of existing files")]
            public bool Overwrite { get; set; }
            [Option("quiet", Required = false, Default = false, HelpText = "Hides any output information")]
            public bool Quiet { get; set; }
        }

        static async Task Extract(FileInfo file, string dest, bool overwrite)
        {
            if (!File.Exists(file.FullName)) { Logger.Log("Specified file does not exist"); return; }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Logger.Log("Reading archive header...");
            G3Pak_Archive PakFile = new G3Pak_Archive();
            PakFile.ReadArchive(file);

            Logger.Log("Extracting archive...");
            bool success = await PakFile.Extract(dest, overwrite);

            sw.Stop();
            if (success) { Logger.Log(string.Format("{0} extracted successfully. (Time: {1})", PakFile.File!.Name, sw.Elapsed)); }
        }

        static void Pack(FileInfo directory, string dest, bool overwrite)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            G3Pak_Archive PakFile = new G3Pak_Archive();
            bool success = PakFile.WriteArchive(directory, dest, overwrite);

            sw.Stop();
            if (success) { Logger.Log(string.Format("{0} packed successfully. (Time: {1})", PakFile.File!.Name, sw.Elapsed)); }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new[] { "--help" };
            }
            
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                Logger.Quiet = o.Quiet;
                string Destination = o.Destination ?? Directory.GetCurrentDirectory();
                
                Destination = Path.Combine(Destination, "");

                if (o.Extract != null)
                {
                    if (Destination == Directory.GetCurrentDirectory()) { Destination = Path.Combine(Destination, Path.GetFileNameWithoutExtension(o.Extract.FullName)); }
                    Extract(o.Extract, Destination, o.Overwrite).Wait();
                }
                if (o.Pack != null)
                {
                    if (Destination == Directory.GetCurrentDirectory()) { Destination = Path.Combine(Destination, o.Pack.Name + ".pak"); }
                    Pack(o.Pack, Destination, o.Overwrite);
                }
            });
        }
    }
}