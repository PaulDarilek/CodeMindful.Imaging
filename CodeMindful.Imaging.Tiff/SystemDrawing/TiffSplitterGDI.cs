using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace CodeMindful.Imaging.Tiff.SystemDrawing;


public class TiffSplitterGDI : ITiffSplit
{
    public static bool IsSupportedPlatform => OperatingSystem.IsWindows();


    [SupportedOSPlatform("windows6.1")]
    public IEnumerable<byte[]> TiffSplit(byte[] data)
    {
        if (!IsSupportedPlatform)
            throw new PlatformNotSupportedException($"Windows Only: {Environment.OSVersion} is not supported.");

        using Stream stream = new MemoryStream(data);
        using Image fileImage = Image.FromStream(stream);

        int frameCount = fileImage.GetFrameCount(FrameDimension.Page);
        if (frameCount == 1)
        {
            yield return data;
            yield break;
        }

        TypeConverter converter = TypeDescriptor.GetConverter(typeof(Image));

        for (int f = 0; f < frameCount; f++)
        {
            fileImage.SelectActiveFrame(FrameDimension.Page, f);
            using (Image image = new Bitmap(fileImage, fileImage.Width, fileImage.Height))
            {
                object? value = converter.ConvertTo(image, typeof(byte[]));
                if (value != null)
                    yield return (byte[])value;
            }
        }

    }

}
