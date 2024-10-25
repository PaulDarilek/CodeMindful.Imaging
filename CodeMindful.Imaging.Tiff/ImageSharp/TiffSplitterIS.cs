using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CodeMindful.Imaging.Tiff.ImageSharp;

/// <summary>Split a multipage TIFF file using ImageSharp NuGet package</summary>
public class TiffSplitterIS : ITiffSplit
{

    /// <summary>Split a multipage Tiff into multiple single page tiff files</summary>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<byte[]> TiffSplit(byte[] data)
    {
        using var imageStream = new MemoryStream(data);

        TiffDecoder decoder = TiffDecoder.Instance;
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

        foreach (byte[] pageBytes in TiffSplit((Image<Rgba32>)image, data.Length))
        {
            yield return pageBytes;
        }
    }


    /// <summary>Actual Implementation of splitting a multipage TIFF into separate TIFF single-pages</summary>
    /// <exception cref="NotImplementedException"></exception>
    private IEnumerable<byte[]> TiffSplit<TPixel>(Image<TPixel> image, int dataLength) where TPixel : unmanaged, IPixel<TPixel>
    {
        throw new NotImplementedException();

        int bytesPerPixel = image.PixelType.BitsPerPixel / 8;
        foreach (var frame in image.Frames)
        {
            //int totalBytes = 1024 + frame.Height * frame.Width * bytesPerPixel;
            //byte[] buffer = new byte[totalBytes];
            //Span<byte> span = buffer.AsSpan();
            var span = new Span<byte>();
            frame.CopyPixelDataTo(span);
            Debug.Assert(span.Length > 1);
            yield return span.ToArray();
        }
    }



    private void ImageDebug(Image image)
    {
        //var pixelType = image.PixelType;

        foreach (ImageFrame frame in image.Frames)
        {
            TiffFrameMetadata meta = frame.Metadata.GetTiffMetadata();
            TiffCompression? compress = meta.Compression ?? TiffCompression.Invalid;

            Debug.WriteLine($"Frame ({frame.Width},{frame.Height}) bpp={meta.BitsPerPixel} compress={compress} ");
        }
    }


}
