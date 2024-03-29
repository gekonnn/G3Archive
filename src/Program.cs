﻿using CommandLine;
using System.Diagnostics;

namespace G3Archive
{
    class Program
    {
        static async Task Extract(FileInfo file)
        {
            if (!File.Exists(file.FullName)) { Logger.Log("Specified file does not exist"); return; }

            try
            {
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
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return;
            }
        }

        static void Pack(FileInfo directory)
        {
            if (!Directory.Exists(directory.FullName)) { Logger.Log("Specified directory does not exist"); return; }
            
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                G3Pak_Archive PakFile = new G3Pak_Archive();
                bool success = PakFile.WriteArchive(directory, ParsedOptions.Destination);

                sw.Stop();
                if (success) { Logger.Log(string.Format("{0} packed successfully. (Time: {1})", PakFile.File!.Name, sw.Elapsed)); }
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

            if (args.Length == 0)
            {
                args = new[] { "--help" };
            }

            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                if(o.Path != null)
                {
                    string Destination = Path.Combine(o.Destination ?? Directory.GetCurrentDirectory(), "");

                    // Store options in a separate static class
                    ParsedOptions.Path = o.Path;
                    ParsedOptions.Extract = o.Extract;
                    ParsedOptions.Pack = o.Pack;
                    ParsedOptions.Destination = Destination;
                    ParsedOptions.Compression = o.Compression;
                    ParsedOptions.NoDecompress = o.NoDecompress;
                    ParsedOptions.NoDeleted = o.NoDeleted;
                    ParsedOptions.Overwrite = o.Overwrite;
                    ParsedOptions.Quiet = o.Quiet;

                    Logger.Quiet = o.Quiet;

                    if (o.Extract)
                    {
                        if (Destination == Directory.GetCurrentDirectory()) { ParsedOptions.Destination = Path.Combine(Destination, Path.GetFileNameWithoutExtension(o.Path.FullName)); }
                        Extract(o.Path).Wait();
                    }
                    if (o.Pack)
                    {
                        if (Destination == Directory.GetCurrentDirectory()) { ParsedOptions.Destination = Path.Combine(Destination, o.Path.Name + ".pak"); }
                        Pack(o.Path);
                    }
                }
            });
            
            
        }
    }
}