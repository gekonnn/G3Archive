namespace G3Archive
{
    public class G3Pak_FileString
    {
        public UInt32 Length;
        public char[] Data = Array.Empty<char>();

        public G3Pak_FileString(BinaryReader br)
        {
            Length = (uint)br.ReadInt32();

            if (Length > 0)
            {
                Data = new char[Length];
                for (int i = 0; i < Length; i++)
                {
                    Data[i] = br.ReadChar();
                }
                br.ReadByte(); // one empty byte after the string
            }
            else
            {
                // assume it's root folder
                Data = "/".ToCharArray();
            }
        }
        public G3Pak_FileString(BinaryWriter bw, string String)
        {
            Length = (uint)String.Length;

            if (Length > 0)
            {
                Data = new char[Length];
                Data = String.Replace("\\", "/").ToArray();
            }
            else
            {
                Data = Array.Empty<char>();
            }
        }

        public string GetString()
        {
            return string.Join("", Data);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(Length);
            if (Length > 0)
            {
                bw.Write(Data);
                bw.Write(new byte[] { 0 });
            }
        }
    }
}
