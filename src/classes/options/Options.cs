using CommandLine;

namespace G3Archive
{
    public class Options
    {
        [Value(0)]
        public FileInfo? Path { get; set; }
        [Option('e', "extract", Required = false, HelpText = "Extracts the selected archive")]
        public bool Extract { get; set; }
        [Option('p', "pack", Required = false, HelpText = "Packages the selected folder into a PAK archive")]
        public bool Pack { get; set; }
        [Option('d', "dest", Required = false, HelpText = "Specifies path of output file")]
        public string? Destination { get; set; }
        [Option("compression", Required = false, Default = 3, HelpText = "Specifies the compression level for compressed files (0-9)")]
        public int Compression { get; set; }
        [Option("no-decompress", Required = false, Default = false, HelpText = "Prevents files from decompressing when extracting")]
        public bool NoDecompress { get; set; }
        [Option("no-deleted", Required = false, Default = false, HelpText = "Prevents files marked as deleted from being output")]
        public bool NoDeleted { get; set; }
        [Option("overwrite", Required = false, Default = false, HelpText = "Forces overwriting of existing files")]
        public bool Overwrite { get; set; }
        [Option("quiet", Required = false, Default = false, HelpText = "Hides any output information")]
        public bool Quiet { get; set; }
    }
}
