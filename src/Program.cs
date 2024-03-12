using CommandLine;
using System.Diagnostics;

namespace G3Archive
{
    class Program
    {
        static async Task Extract(FileInfo file)
        {
            if (!File.Exists(file.FullName)) { Logger.Log("Specified file does not exist"); return; }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Logger.Log("Reading archive header...");
            G3Pak_Archive PakFile = new G3Pak_Archive();
            PakFile.ReadArchive(file);

            Logger.Log("Extracting archive...");
            bool success = await PakFile.Extract(ParsedOptions.Destination);

            sw.Stop();
            if (success) { Logger.Log(string.Format("{0} extracted successfully. (Time: {1})", PakFile.File!.Name, sw.Elapsed)); }
        }

        static void Pack(FileInfo directory)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            G3Pak_Archive PakFile = new G3Pak_Archive();
            bool success = PakFile.WriteArchive(directory, ParsedOptions.Destination);

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
                string Destination = Path.Combine(o.Destination ?? Directory.GetCurrentDirectory(), "");
                
                // Store options in a separate static class
                ParsedOptions.Extract = o.Extract;
                ParsedOptions.Pack = o.Pack;
                ParsedOptions.Destination = Destination;
                ParsedOptions.Overwrite = o.Overwrite;
                ParsedOptions.Quiet = o.Quiet;

                Logger.Quiet = o.Quiet;

                if (o.Extract != null)
                {
                    if (Destination == Directory.GetCurrentDirectory()) { Destination = Path.Combine(Destination, Path.GetFileNameWithoutExtension(o.Extract.FullName)); }
                    Extract(o.Extract).Wait();
                }
                if (o.Pack != null)
                {
                    if (Destination == Directory.GetCurrentDirectory()) { Destination = Path.Combine(Destination, o.Pack.Name + ".pak"); }
                    Pack(o.Pack);
                }
            });
            
            
        }
    }
}