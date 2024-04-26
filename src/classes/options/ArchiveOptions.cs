namespace G3Archive
{
    public class ArchiveOptions
    {
        public required string Destination  { get; set; }
        public bool NoDecompress    { get; set; } = false;
        public bool NoDeleted       { get; set; } = false;
        public bool Overwrite       { get; set; } = true;
        public int Compression      { get; set; } = 3;
    }
}
