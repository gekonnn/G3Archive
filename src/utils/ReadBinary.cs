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

        public byte[] Bytes(ref long offset, int length)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += length;
            return br.ReadBytes(length);
        }

        public byte[] Bytes(long offset, int length)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            return br.ReadBytes(length);
        }

        public uint Byte(ref long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 1;
            return br.ReadByte();
        }

        public uint Byte(long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            return br.ReadByte();
        }

        public UInt32 UInt32(ref long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 4;
            return br.ReadUInt32();
        }
        
        public UInt32 UInt32(long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            return br.ReadUInt32();
        }
        
        public UInt64 UInt64(ref long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 8;
            return br.ReadUInt64();
        }
        
        public UInt64 UInt64(long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            return br.ReadUInt64();
        }
        
        public char Char(ref long offset)
        {
            //Console.WriteLine("Reading char at " + offset);
            fs.Seek(offset, SeekOrigin.Begin);
            offset += 1;
            return br.ReadChar();
        }
        
        public char Char(long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);
            return br.ReadChar();
        }
    }
}
