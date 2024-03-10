using System.Text;

namespace G3Archive
{
    public class ReadBinary
    {
        public FileStream fs;
        public BinaryReader br;
        public ReadBinary(FileStream fs)
        {
            this.fs = fs;
            br = new BinaryReader(fs, Encoding.GetEncoding("iso-8859-1"));
        }

        public byte[] Bytes(int length)
        {
            return br.ReadBytes(length);
        }

        public uint Byte()
        {
            return br.ReadByte();
        }

        public UInt32 UInt32()
        {
            return br.ReadUInt32();
        }
        
        public UInt64 UInt64()
        {
            return br.ReadUInt64();
        }
        
        public char Char()
        {
            return br.ReadChar();
        }
    }
}
