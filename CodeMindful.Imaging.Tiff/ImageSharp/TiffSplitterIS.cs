using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;
using System.Diagnostics;

namespace CodeMindful.Imaging.Tiff.ImageSharp;

/// <summary>Split a multipage TIFF file using ImageSharp NuGet package</summary>
public class TiffSplitterIS : ITiffSplit
{

    /// <summary>Split a multipage Tiff into multiple single page tiff files</summary>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<byte[]> TiffSplit(byte[] data)
    {
        using var imageStream = new MemoryStream(data);

        var decoder = TiffDecoder.Instance;
        var options = new DecoderOptions()
        {
            SkipMetadata = false,
        };
        using Image image = decoder.Decode(options, imageStream);

        ImageDebug(image);

        // Single Page Nothing to Merge...
        if (image.Frames.Count == 1)
        {
            yield return data;
            yield break;
        }

        foreach (byte[] pageBytes in TiffSplit(image))
        {
            yield return pageBytes;
        }
    }

    /// <summary>Actual Implementation of splitting a multipage TIFF into separate TIFF single-pages</summary>
    /// <exception cref="NotImplementedException"></exception>
    private IEnumerable<byte[]> TiffSplit(Image image)
    {
        throw new NotImplementedException();
    }

    private void ImageDebug(Image image)
    {
        foreach (ImageFrame frame in image.Frames)
        {
            TiffFrameMetadata meta = frame.Metadata.GetTiffMetadata();
            TiffCompression? compress = meta.Compression ?? TiffCompression.Invalid;

            Debug.WriteLine($"Frame ({frame.Width},{frame.Height}) bpp={meta.BitsPerPixel} compress={compress} ");
        }
    }


}
