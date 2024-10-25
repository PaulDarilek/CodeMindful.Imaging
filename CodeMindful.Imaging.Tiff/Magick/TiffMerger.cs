using ImageMagick;
using ImageMagick.Formats;

namespace CodeMindful.Imaging.Tiff.Magick
{
    public class TiffMerger : ITiffMerge
    {
        public byte[] TiffMerge(byte[][] pages)
        {
            using var images = new MagickImageCollection();

            for (int i = 0; i < pages.Length; i++)
            {
                // skip using on the image to avoid GC before the end.
                var tiff = new MagickImage(pages[i]);
                tiff.Format = MagickFormat.Tiff;
                tiff.SetCompression(CompressionMethod.Group4);
                images.Add(tiff);
            }

            var defines = new TiffWriteDefines
            {
                PreserveCompression = true,
            };

            int sizeEstimate = pages.Sum(x => x.Length);
            using var stream = new MemoryStream(sizeEstimate);
            images.Write(stream, defines);
            try
            {
                return stream.ToArray();
            }
            finally
            {
                foreach (var image in images)
                {
                    image.Dispose();
                }
            }
        }

    }
}
