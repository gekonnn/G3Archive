namespace G3Archive
{
    public enum G3Pak_Compression
    {
        None    = 0x00000000,
        Auto    = 0x00000001,
        Zip     = 0x00000002,
        User    = 0x10000000,
        Invalid = 0x7FFFFFFF,
        Threshold = 4096
    };
}