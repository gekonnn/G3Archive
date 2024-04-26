namespace G3Archive
{
    public class G3Pak_FileString
    {
        public readonly G3Pak_Archive Archive;

        public UInt32 Length;
        public char[] Data = Array.Empty<char>();

        public G3Pak_FileString(G3Pak_Archive Archive, string String)
        {
            this.Archive = Archive;

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

        public G3Pak_FileString(G3Pak_Archive Archive)
        {
            this.Archive = Archive;
        }

        public void ReadFromArchive()
        {
            Length = Archive.Reader.ReadUInt32();

            if (Length > 0)
            {
                Data = new char[Length];
                for (int i = 0; i < Length; i++)
                {
                    Data[i] = Archive.Reader.ReadChar();
                }
                Archive.Reader.ReadByte(); // one empty byte after the string
            }
            else
            {
                // assume it's root folder
                Data = "/".ToCharArray();
            }
        }

        public string GetString()
        {
            return string.Join("", Data);
        }

        public void Write()
        {
            Archive.Writer.Write(Length);
            if (Length > 0)
            {
                Archive.Writer.Write(Data);
                Archive.Writer.Write(new byte[] { 0 });
            }
        }
    }
}
