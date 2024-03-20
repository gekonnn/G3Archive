﻿namespace G3Archive
{
    public static class ParsedOptions
    {
        public static FileInfo? Extract;
        public static FileInfo? Pack;
        public static string Destination    = default!;
        public static int Compression       = default!;
        public static bool NoDecompress     = default!;
        public static bool NoDeleted        = default!;
        public static bool Overwrite        = default!;
        public static bool Quiet            = default!;
    }
}
