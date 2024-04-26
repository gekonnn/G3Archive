namespace G3Archive
{
    public class G3Pak_FileEntry
    {
        public readonly G3Pak_Archive Archive;

        public UInt64 Offset;
        public UInt64 Bytes;
        public UInt64 Size;
        public UInt32 Encryption;
        public UInt32 Compression;
        public G3Pak_FileString FileName = default!;
        public G3Pak_FileString Comment = default!;
        public FileInfo? EntryFile;

        public G3Pak_FileEntry(G3Pak_Archive Archive)
        {
            this.Archive = Archive;
        }

        public void ReadFromArchive()
        {
            Offset      = Archive.Reader.ReadUInt64();
            Bytes       = Archive.Reader.ReadUInt64();
            Size        = Archive.Reader.ReadUInt64();
            Encryption  = Archive.Reader.ReadUInt32();
            Compression = Archive.Reader.ReadUInt32();
            FileName    = new G3Pak_FileString(Archive);
            FileName.ReadFromArchive();
            Comment     = new G3Pak_FileString(Archive);
            Comment.ReadFromArchive(); 
        }

        public void ReadFromFile(FileInfo file, FileInfo RootDirectory, UInt64 OffsetToFiles, uint Compression = 0, string Comment = "")
        {
            this.Offset         = OffsetToFiles;
            this.Bytes          = (uint)file.Length;
            this.Size           = (uint)File.ReadAllBytes(file.FullName).Length;
            this.Encryption     = 0;
            this.Compression    = Compression;
            this.FileName       = new G3Pak_FileString(Archive, Path.GetRelativePath(RootDirectory.FullName, file.FullName));
            this.Comment        = new G3Pak_FileString(Archive, Comment);
            this.EntryFile      = file;
        }

        public byte[] ReadFileBytes()
        {
            if (EntryFile != null)
            {
                return File.ReadAllBytes(EntryFile.FullName);
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        public void Write()
        {
            Archive.Writer.Write(Offset);
            Archive.Writer.Write(Bytes);
            Archive.Writer.Write(Size);
            Archive.Writer.Write(Encryption);
            Archive.Writer.Write(Compression);
            FileName.Write();
            Comment.Write();
        }
    }
}
