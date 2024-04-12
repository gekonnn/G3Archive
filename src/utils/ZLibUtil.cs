using Ionic.Zlib;

namespace G3Archive
{
    public class ZLibUtil
    {
        public static List<string> CompressionExcludedFileTypes = new() { ".dds", ".xlip", ".ogg", ".wav" };
        private static readonly CompressionLevel[] compressionLevels = {
            CompressionLevel.Level0,
            CompressionLevel.Level1,
            CompressionLevel.Level2,
            CompressionLevel.Level3,
            CompressionLevel.Level4,
            CompressionLevel.Level5,
            CompressionLevel.Level6,
            CompressionLevel.Level7,
            CompressionLevel.Level8,
            CompressionLevel.Level9
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
                using MemoryStream input = new(RawData);
                using MemoryStream output = new();
                using (ZlibStream decompressionStream = new(input, CompressionMode.Decompress))
                {
                    await decompressionStream.CopyToAsync(output);
                }

                return output.ToArray();
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
                using MemoryStream outputStream = new();
                using (ZlibStream compressionStream = new(outputStream, CompressionMode.Compress, compressionLevel))
                {
                    compressionStream.Write(RawData, 0, RawData.Length);
                }
                return outputStream.ToArray();
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
