using CommandLine;

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

        static void Extract(FileInfo file, string dest, bool overwrite)
        {
            Logger.Log("Reading archive header...");
            G3Pak_Archive PakFile = new G3Pak_Archive();
            PakFile.ReadArchive(file);

            Logger.Log("Extracting archive...");
            int result = PakFile.Extract(dest, overwrite);
            if (result == 0) { Logger.Log(PakFile.File!.Name + " extracted successfully."); }
        }

        static void Pack(FileInfo directory, string dest, bool overwrite)
        {
            G3Pak_Archive PakFile = new G3Pak_Archive();
            int result = PakFile.WriteArchive(directory, dest, overwrite);
            if(result == 0) { Logger.Log(PakFile.File!.Name + " packed successfully."); }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                Logger.Quiet = o.Quiet;
                string Destination = o.Destination ?? Directory.GetCurrentDirectory();
                
                Destination = Path.Combine(Destination, "");

                if (o.Extract != null)
                {
                    if (!File.Exists(o.Extract.FullName)) { Logger.Log("Specified file does not exist"); return; }
                    if (Destination == Directory.GetCurrentDirectory()) { Destination = Path.Combine(Destination, Path.GetFileNameWithoutExtension(o.Extract.FullName)); }
                    Extract(o.Extract, Destination, o.Overwrite);
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