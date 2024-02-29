using System;
using System.IO;
using CommandLine;

namespace G3Archive
{
    class Program
    {
        public class Options
        {
            [Option('e', "extract", Required = false, HelpText = "Extracts the selected archive")]
            public FileInfo? Extract { get; set; }
            [Option('d', "dest", Required = false, HelpText = "Specifies directory the archive will be extracted in")]
            public string? Destination { get; set; }
        }

        static void Extract(FileInfo file, string dest)
        {
            Console.WriteLine(dest);
            if(!File.Exists(file.FullName))
            {
                Console.WriteLine("Error: Specified file does not exist.");
                return;
            }

            G3Pak_Archive PakFile = new G3Pak_Archive(file);

            Console.WriteLine("Reading archive header...");
            PakFile.Read_PakHeader();
            Console.WriteLine("Extracting archive...");
            PakFile.ExtractArchive(dest + "\\" + file.Name + "\\");
            Console.WriteLine(PakFile.pak_fileName + " extracted successfully.");
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                if(o.Extract != null)
                {
                    Extract(o.Extract, o.Destination ?? Directory.GetCurrentDirectory());
                }
            });
        }
    }
}