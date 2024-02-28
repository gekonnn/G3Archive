using System;
using System.IO;
using CommandLine;

namespace G3Archive
{
    class Program
    {
        public class Options
        {
            [Option("extract", Required = false, HelpText = "Extracts the selected archive")]
            public string Extract { get; set; }
        }

        static void Extract(FileInfo file)
        {
            if(!File.Exists(file.FullName))
            {
                Console.WriteLine("Error: Specified file does not exist.");
                return;
            }

            G3Pak_Archive PakFile = new G3Pak_Archive();
            PakFile.path = file.FullName;

            PakFile.pak_fileName = file.Name;
            PakFile.pak_fileExt = file.Extension;

            Console.WriteLine("Reading archive header...");
            PakFile.Read_PakHeader();
            Console.WriteLine("Extracting archive...");
            PakFile.ExtractArchive();
            Console.WriteLine(PakFile.pak_fileName + " extracted successfully.");
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                if(o.Extract != null)
                {
                    Extract(new FileInfo(o.Extract));
                }
            });
        }
    }
}