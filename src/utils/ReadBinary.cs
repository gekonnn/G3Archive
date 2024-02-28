namespace G3Archive
{
    public class ReadBinary
    {
        public uint Byte(FileStream fs, BinaryReader br, ref long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 1;
            return br.ReadByte();
        }
        public UInt32 UInt32(FileStream fs, BinaryReader br, ref long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 4;
            return br.ReadUInt32();
        }
        public UInt64 UInt64(FileStream fs, BinaryReader br, ref long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 8;
            return br.ReadUInt64();
        }
        public Char Char(FileStream fs, BinaryReader br, ref    long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 1;
            return br.ReadChar();
        }
    }
}
