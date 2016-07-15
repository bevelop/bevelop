using System.IO;
using System.IO.Compression;
using System.Text;

namespace Bevelop.VSClient.Services
{
    public class Zipper : IZipper
    {
        public byte[] Zip(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            using (var inStream = new MemoryStream(bytes))
            using (var outStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(outStream, CompressionMode.Compress))
                {
                    CopyTo(inStream, zipStream);
                }

                return outStream.ToArray();
            }
        }

        public string Unzip(byte[] bytes)
        {
            using (var inStream = new MemoryStream(bytes))
            using (var outStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(inStream, CompressionMode.Decompress))
                {
                    CopyTo(zipStream, outStream);
                }

                return Encoding.UTF8.GetString(outStream.ToArray());
            }
        }

        static void CopyTo(Stream src, Stream dest)
        {
            var bytes = new byte[4096];

            int count;

            while ((count = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, count);
            }
        }
    }
}