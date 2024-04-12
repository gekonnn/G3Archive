namespace G3Archive
{
    public static class Options
    {
        public static FileInfo Path         = default!;
        public static bool Extract;
        public static bool Pack;
        public static string Destination    = default!;
        public static int Compression;
        public static bool NoDecompress;
        public static bool NoDeleted;
        public static bool Overwrite;
        public static bool Quiet;
    }
}
