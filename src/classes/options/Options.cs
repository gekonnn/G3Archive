namespace G3Archive
{
    public static class Options
    {
        public static string Path         = default!;
        public static string Destination  = default!;
        public static bool Extract;
        public static bool Pack;
        public static bool NoDecompress;
        public static bool NoDeleted;
        public static bool Overwrite;
        public static bool Quiet;
        public static int Compression;
    }
}
