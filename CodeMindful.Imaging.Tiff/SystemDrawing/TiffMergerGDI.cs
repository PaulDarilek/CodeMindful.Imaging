using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace CodeMindful.Imaging.Tiff.SystemDrawing;

public class TiffMergerGDI : ITiffMerge
{
    private ILogger? Logger { get; }
    public string TemporaryFolder { get; set; } = Environment.GetEnvironmentVariable("Temp") ?? Environment.CurrentDirectory;

    public static bool IsSupportedPlatform => OperatingSystem.IsWindowsVersionAtLeast(6, 2);

    [SupportedOSPlatform("windows6.1")]
    private static ImageCodecInfo[] TiffCodecs { get; } = ImageCodecInfo.GetImageEncoders().Where(x => x.MimeType == "image/tiff" || x.MimeType == "image/tif").ToArray();

    public TiffMergerGDI(ILogger? logger)
    {
        Logger = logger;
    }


    [SupportedOSPlatform("windows6.2")]
    public byte[] TiffMerge(byte[][] files)
    {
        if (!IsSupportedPlatform)
            throw new PlatformNotSupportedException($"Windows Only: {Environment.OSVersion} is not supported.");

        var destinationPath = Path.Combine(TemporaryFolder, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".tif");
        Logger?.LogDebug("tiff destinationPath: {destinationPath}", destinationPath);

        using var firstImageStream = new MemoryStream(files[0]);
        firstImageStream.Position = 0;
        using Image fileImage = Image.FromStream(firstImageStream, true, false);

        EncoderParameters encoderParameters = new(SupportsLzwCompression(files[0]) ? 1 : 2);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
        if (encoderParameters.Param.Length > 1)
        {
            encoderParameters.Param[1] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
        }
        //reset the stream to the beginning
        Logger?.LogDebug("assembling page 1 of {total}", files.Length);

        ImageCodecInfo encoderInfo = TiffCodecs.First();
        fileImage.Save(destinationPath, encoderInfo, encoderParameters);
        Logger?.LogDebug("page 1 added.");

        // Set Secondary Pages (FrameDimensionPage)
        encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
        if (encoderParameters.Param.Length > 1)
        {
            encoderParameters.Param[1] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);
        }
        for (var i = 1; i < files.Length; i++)
        {

            try
            {
                Logger?.LogDebug($"assembling page {i + 1} of {files.Length}");

                using var stream = new MemoryStream(files[i]);
                using Image img = Image.FromStream(stream, true, false);

                fileImage.SaveAdd(new Bitmap(img), encoderParameters);
                Logger?.LogDebug($"page {i + 1} added.");

            }
            catch (Exception ex)
            {
                Logger?.LogError(ex.ToString());
                //if we get an error assembling the tif, just return what we have
            }
        }
        encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
        fileImage.SaveAdd(encoderParameters);
        byte[] fileBytes = File.ReadAllBytes(destinationPath);
        try
        {
            File.Delete(destinationPath);
        }
        catch (Exception)
        {

            Logger?.LogWarning($"Temp file: {destinationPath} failed to delete.");
            //eat it, doesn't matter that the temp file won't delete.
        }
        return fileBytes;
    }

    [SupportedOSPlatform("windows6.2")]
    private static bool SupportsLzwCompression(byte[] bytes)
    {
        using Stream stream = new MemoryStream(bytes);
        using Image image = Image.FromStream(stream);

        int compressionTagIndex = Array.IndexOf(image.PropertyIdList, 0x103);
        if (compressionTagIndex != -1)
        {
            PropertyItem compressionTag = image.PropertyItems[compressionTagIndex];
            var tag = compressionTag.Value != null ? BitConverter.ToInt16(compressionTag.Value, 0) : 0;

            // old JPEG compression 
            if (tag == 6)
            {
                return false;
            }
        }

        return true;
    }

}
