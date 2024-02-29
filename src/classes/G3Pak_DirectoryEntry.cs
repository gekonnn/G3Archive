namespace G3Archive
{
    public class G3Pak_DirectoryEntry
    {
        public G3Pak_FileString FileName;
        public G3Pak_FileTableEntry[] DirTable;
        public UInt32 DirCount;
        public G3Pak_FileTableEntry[] FileTable;
        public UInt32 FileCount;

        public G3Pak_DirectoryEntry(ReadBinary Read, ref long offset)
        {
            FileName = new G3Pak_FileString(Read, ref offset);

            DirCount = Read.UInt32(ref offset);
            DirTable = new G3Pak_FileTableEntry[DirCount];
            for (int i_dir = 0; i_dir < DirCount; i_dir++)
            {
                // Not really neccessary to do that in this case,
                // we're doing that just to push the offset further.
                G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(Read, ref offset);
                DirTable[i_dir] = _fileTableEntry;
            }

            FileCount = Read.UInt32(ref offset);
            FileTable = new G3Pak_FileTableEntry[FileCount];
            for (int i_file = 0; i_file < FileCount; i_file++)
            {
                G3Pak_FileTableEntry _fileTableEntry = new G3Pak_FileTableEntry(Read, ref offset);
                FileTable[i_file] = _fileTableEntry;

            }
        }
    }
}
