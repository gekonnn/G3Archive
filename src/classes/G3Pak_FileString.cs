namespace G3Archive
{
    public class G3Pak_FileString
    {
        public UInt32 Length;
        public char[] Data = new Char[0];

        public G3Pak_FileString(ReadBinary Read, ref long offset)
        {
            Length = Read.UInt32(ref offset);

            if (Length > 0)
            {
                Data = new Char[Length];
                for (int i = 0; i < Length; i++)
                {
                    Data[i] = Read.Char(ref offset);
                }
                offset++; // one empty byte after the string?
            }
            else
            {
                // assume it's root folder
                Data = "/".ToCharArray();
            }
        }
    }
}
