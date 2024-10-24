using CodeMindful.Imaging.Tiff.ImageSharp;
using SixLabors.ImageSharp;
using System.Diagnostics;

namespace CodeMindful.Imaging.Tests.TiffTests;

public class ImageSharpTests : TiffBase
{
    /// <summary>Files names wint Img# have something not supported by ImageSharp library</summary>
    private const string FileExclude = "Img#";
    private const string FileSpecTiffFails = $"*{FileExclude}{FileSpecTiff}";

    public ImageSharpTests() : base(new TiffSplitterIS(), new TiffMergerIS())
    {
    }


    /// <summary>Test will split a multi-page document, and ensure that the output file can load with Image.Load()</summary>
    [Test]
    public void TiffSplitter_TiffSplit()
    {
        TestEachFile(Test, FileSpecTiffMultiPage, FileExclude);

        bool Test(FileInfo file)
        {
            var bytes = File.ReadAllBytes(file.FullName);

            var split = TiffSplitter.TiffSplit(bytes).ToList();
            Assert.That(split, Is.Not.Null);

            if (split.Count > 1) // write pages if 
            {
                for (int i = 0; i < split.Count; i++)
                {
                    string outFile = CreateFileName(file, $"Pg{i + 1}");
                    var fullPath = WriteFile(outFile, split[i]);

                    // expect the split file to be able to load successfully and is a single page.
                    using var image = Image.Load(fullPath);
                    Assert.That(image.Frames, Has.Count.EqualTo(1));
                }
            }
            return split.Count > 1;
        }
    }


    /// <summary>Test will split a multi-page document, then merge it again to test the merge function.</summary>
    [Test]
    public void TiffMerger_TiffMerge()
    {
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
                string outFile = CreateFileName(file, $"{split.Count}-Pages");
                var fullPath = WriteFile(outFile, mergedBytes);

                // expect the merged file page count equals the split page count.
                using var image = Image.Load(fullPath);
                Assert.That(image.Frames, Has.Count.EqualTo(split.Count));
            }

            return true;
        }
    }


    [Test]
    public void ImageLoad_IsSuccessful_FileTests()
    {
        TestEachFile(Test, FileSpecTiff, FileExclude);

        static bool Test(FileInfo file)
        {
            using var image = Image.Load(file.FullName);
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Frames, Has.Count.AtLeast(1));
            return true;
        }
    }


    [Test]
    public void ImageLoad_ThrowsException_FileTests()
    {

        TestEachFile(Test, FileSpecTiffFails, null);

        static bool Test(FileInfo file)
        {
            try
            {
                var image = Image.Load(file.FullName);
                Trace.WriteLine($"{file.Name}: Loaded correctly... consider renaming.");
                throw new InvalidDataException("Expected file to throw NotSupportedException or ImageFormatException (consider renaming file)");
            }
            catch (NotSupportedException ex)
            {
                switch (ex.Message)
                {
                    case "Only 8 bits per channel is supported for CMYK images.":
                    case "Invalid color type: Cmyk":
                    case "The specified TIFF compression format 'ThunderScan' is not supported":
                    case "Missing SOI marker offset for tiff with old jpeg compression":
                    case "Images with different sizes are not supported":
                    case "ImageSharp only supports the UnsignedInteger and Float SampleFormat.":
                        Debug.WriteLine($"{file.Name}: {ex.Message}");
                        return true;
                    default:
                        throw;
                }

                // Expected it to Fail...
            }
            catch (ImageFormatException ex)
            {
                switch (ex.Message)
                {
                    case "Corrupted TIFF LZW: code 256 (table size: 258)":
                        Debug.WriteLine($"{file.Name}: {ex.Message}");
                        return true;
                    default:
                        throw;
                }

            }
        }
    }



}
