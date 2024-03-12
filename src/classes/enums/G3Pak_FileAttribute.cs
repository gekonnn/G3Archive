namespace G3Archive
{
    public enum G3Pak_FileAttribute : uint
    {
        ReadOnly    = 0x00000001,
        Hidden      = 0x00000002,
        System      = 0x00000004,
        Directory   = 0x00000010,
        Archive     = 0x00000020,
        Normal      = 0x00000080,
        Temporary   = 0x00000100,
        Compressed  = 0x00000800,
        Encrypted   = 0x00004000,
        Deleted     = 0x00008000,
        Virtual     = 0x00010000,
        Packed      = 0x00020000,
        Stream      = 0x00040000,
        Invalid     = 0xFFFFFFFF
    };
}