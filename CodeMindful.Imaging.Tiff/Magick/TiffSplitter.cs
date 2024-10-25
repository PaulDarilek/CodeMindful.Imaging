using ImageMagick.Formats;
using ImageMagick;

namespace CodeMindful.Imaging.Tiff.Magick;

public class TiffSplitter : ITiffSplit
{
    public IEnumerable<byte[]> TiffSplit(byte[] data)
    {
        using var images = new MagickImageCollection(data);
        if(images.Count == 1)
        {
            yield return data;
            yield break;
        }
        var defines = new TiffWriteDefines
        {
            PreserveCompression = true,
        };
        foreach (var image in images)
        {
            var page = image.ToByteArray(defines);
            yield return page;  
        }
    }
}
