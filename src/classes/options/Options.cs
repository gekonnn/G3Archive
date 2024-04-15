namespace G3Archive
{
    public static class Options
    {
        public static string Path           = default!;
        public static string Destination    = default!;
        public static bool Extract;
        public static bool Pack;
        public static bool NoDecompress     = false;
        public static bool NoDeleted        = false;
        public static bool Overwrite        = false;
        public static bool Verbose          = false;
        public static int Compression       = 3;
    }
}
