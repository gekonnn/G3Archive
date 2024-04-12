using Ionic.Zlib;

namespace G3Archive
{
    public class ZLibUtil
    {
        public static List<string> CompressionExcludedFileTypes = new List<string> { ".dds", ".xlip", ".ogg", ".wav" };
        private static CompressionLevel[] compressionLevels = {
            Ionic.Zlib.CompressionLevel.Level0,
            Ionic.Zlib.CompressionLevel.Level1,
            Ionic.Zlib.CompressionLevel.Level2,
            Ionic.Zlib.CompressionLevel.Level3,
            Ionic.Zlib.CompressionLevel.Level4,
            Ionic.Zlib.CompressionLevel.Level5,
            Ionic.Zlib.CompressionLevel.Level6,
            Ionic.Zlib.CompressionLevel.Level7,
            Ionic.Zlib.CompressionLevel.Level8,
            Ionic.Zlib.CompressionLevel.Level9
        };

        public static async Task<byte[]> Decompress(byte[] RawData, string FileName = "")
        {
            // Ensure the file has a valid zlib header
            if (RawData[0] != 0x78)
            {
                Logger.Log("Incorrect zlib header for " + FileName);
                return Array.Empty<byte>();
            }

            try
            {
                using (MemoryStream input = new MemoryStream(RawData))
                using (MemoryStream output = new MemoryStream())
                {
                    using (ZlibStream decompressionStream = new ZlibStream(input, Ionic.Zlib.CompressionMode.Decompress))
                    {
                        await decompressionStream.CopyToAsync(output);
                    }
                    return output.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Decompression failed for {0} (Header = {1})", FileName, RawData[0].ToString("X2") + RawData[1].ToString("X2")));
                Logger.Log("Exception: " + ex);
                return Array.Empty<byte>();
            }
        }

        public static byte[] Compress(byte[] RawData, string FileName = "")
        {
            try
            {
                CompressionLevel compressionLevel = compressionLevels[Math.Min(Math.Max(Options.Compression, 0), compressionLevels.Length - 1)];
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (ZlibStream compressionStream = new ZlibStream(outputStream, Ionic.Zlib.CompressionMode.Compress, compressionLevel))
                    {
                        compressionStream.Write(RawData, 0, RawData.Length);
                    }
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("Compression failed for {0}", FileName));
                Logger.Log("Exception: " + ex);
                return Array.Empty<byte>();
            }
        }
    }
}
