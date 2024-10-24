namespace CodeMindful.Imaging.Tiff;

/// <summary>Merge multiple Tiff Files into a single multipage Tiff</summary>
public interface ITiffMerge
{
    public byte[] TiffMerge(byte[][] pages);
    public byte[] TiffMerge(IEnumerable<byte[]> pages) => TiffMerge(pages.ToArray());
}