using CommandLine;

namespace G3Archive
{
    public class Options
    {
        [Option('e', "extract", Required = false, HelpText = "Extracts the selected archive")]
        public FileInfo? Extract { get; set; }
        [Option('p', "pack", Required = false, HelpText = "Packages the selected folder into a PAK archive")]
        public FileInfo? Pack { get; set; }
        [Option('d', "dest", Required = false, HelpText = "Specifies path of output file")]
        public string? Destination { get; set; }
        [Option("exclude-deleted", Required = false, Default = false, HelpText = "Prevents files marked as deleted from being output")]
        public bool ExcludeDeleted { get; set; }
        [Option("overwrite", Required = false, Default = false, HelpText = "Forces overwriting of existing files")]
        public bool Overwrite { get; set; }
        [Option("quiet", Required = false, Default = false, HelpText = "Hides any output information")]
        public bool Quiet { get; set; }
    }
}
