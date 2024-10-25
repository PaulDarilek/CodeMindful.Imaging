using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using CodeMindful.Imaging.Tiff.SystemDrawing;

namespace CodeMindful.Imaging.Tests.TiffTests;


[SupportedOSPlatform("windows6.2")]
public class WindowsGdiTiffTests : TiffBase

{
    /// <summary>Files with "XWin" in the name don't work with built in Windows TIFF encoders/decoders and Windows GDI.</summary>
    private const string FileExclude = "XWin";

    /// <summary>File Search Pattern expected to get GDI errors</summary>
    private const string FileSpecTiffFails = $"*{FileExclude}{FileSpecTiff}";

    public WindowsGdiTiffTests() : base(new TiffSplitterGDI(), new TiffMergerGDI(logger: null)) { }

    /// <summary>Test will split a multi-page document, and ensure that the output file can load with Image.FromFile()</summary>
    [Test]
    public void TiffSplitter_TiffSplit()
    {
        if (!TiffSplitterGDI.IsSupportedPlatform)
            Assert.Inconclusive("Unsupported Platform");

        TestEachFile(Test, FileSpecTiff, FileExclude);

        bool Test(FileInfo file)
        {
            var bytes = File.ReadAllBytes(file.FullName);
            var split = TiffSplitter.TiffSplit(bytes).ToList();
            Assert.That(split, Is.Not.Null);

            if (split.Count > 1) // write split files only if it was a multipage file.
            {
                for (int i = 0; i < split.Count; i++)
                {
                    string outFile = CreateFileName(file, $"Page-{i + 1}");
                    var fullPath = WriteFile(outFile, split[i]);

                    // expect the split file to be able to load successfully and is a single page.
                    using var image = Image.FromFile(fullPath);
                    Assert.That(image.GetFrameCount(FrameDimension.Page), Is.EqualTo(1));
                }
            }
            
            return true;
        }
    }

    /// <summary>Test will split a multi-page document, then merge it again to test the merge function.</summary>
    [Test]
    public void TiffMerger_TiffMerge()
    {
        if (!TiffSplitterGDI.IsSupportedPlatform)
            Assert.Inconclusive("Unsupported Platform");

        TestEachFile(Test, FileSpecTiffMultiPage, FileExclude);

        bool Test(FileInfo file)
        {
            var bytes = File.ReadAllBytes(file.FullName);
            var split = TiffSplitter.TiffSplit(bytes).ToList();
            Assert.That(split, Is.Not.Null);
            Assert.That(split, Has.Count.GreaterThanOrEqualTo(1));

            if (split.Count > 1) // write split files only if it was a multipage file.
            {
                byte[] mergedBytes = TiffMerger.TiffMerge(split);

                // write to same name but in TestResults directory.
                string outFile = CreateFileName(file, $"Pages-{split.Count}");
                var fullPath = WriteFile(outFile, mergedBytes);

                // expect the merged file page count equals the split page count.
                using var image = Image.FromFile(fullPath);
                Assert.That(image.GetFrameCount(FrameDimension.Page), Is.EqualTo(split.Count));
            }

            return true;
        }
    }

    /// <summary>Test will try using built in Windows System.Drawing.Image.LoadFile to verify that properly named files will load</summary>
    [Test]
    public void ImageLoad_IsSuccessful_FileTests()
    {
        if (!TiffSplitterGDI.IsSupportedPlatform)
            Assert.Inconclusive("Unsupported Platform");

        TestEachFile(Test, FileSpecTiff, FileExclude);

        static bool Test(FileInfo file)
        {
            using var image = Image.FromFile(file.FullName);
            Assert.That(image, Is.Not.Null);
            var pages = image.GetFrameCount(FrameDimension.Page);
            Assert.That(pages, Is.AtLeast(1));
            return true;
        }
    }

    /// <summary>Test will try using built in Windows System.Drawing.Image.LoadFile to verify that properly named files that have GDI issues will NOT load</summary>
    [Test]
    public void ImageLoad_ThrowsException_FileTests()
    {
        if (!TiffSplitterGDI.IsSupportedPlatform)
            Assert.Inconclusive("Unsupported Platform");

        TestEachFile(Test, FileSpecTiffFails, null);

        static bool Test(FileInfo file)
        {
            try
            {
                using var image = Image.FromFile(file.FullName);
                var pages = image.GetFrameCount(FrameDimension.Page);
                Assert.That(pages, Is.GreaterThanOrEqualTo(1));

                // expected an OutOfMemoryException, so throw this error instead.
                throw new InvalidDataException($"Text Expects Windows GDI failure (consider renaming file unlike {FileSpecTiffFails})");
            }
            catch (OutOfMemoryException)
            {
                // Expectation: Files that are marked as not supported by Windows will get an OutOfMemoryException
                // This was expected... other errors are failures.
                Debug.WriteLine($"OutOfMemoryException: {file.Name}");
                
                return true; // Expected this exception.
            }
        }
    }


}
