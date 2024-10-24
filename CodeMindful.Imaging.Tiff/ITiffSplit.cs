namespace CodeMindful.Imaging.Tiff;

//See also: https://github.com/yigolden/TiffLibrary 


/// <summary>Split a multipage Tiff file into separate Tiff image pages</summary>
public interface ITiffSplit
{
    public IEnumerable<byte[]> TiffSplit(byte[] data);
}
