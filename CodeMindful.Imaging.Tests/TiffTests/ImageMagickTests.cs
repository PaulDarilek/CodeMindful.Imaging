using CodeMindful.Imaging.Tiff.Magick;
using ImageMagick;
using System.Diagnostics;

namespace CodeMindful.Imaging.Tests.TiffTests;

public class ImageMagickTests :TiffBase
{
    /// <summary>Files names wint Img# have something not supported by ImageSharp library</summary>
    private const string FileExclude = "Magic";
    private const string FileSpecTiffFails = $"*{FileExclude}{FileSpecTiff}";

    public ImageMagickTests() : base(new TiffSplitter(), new TiffMerger())
    {
    }

    /// <summary>Test will split a multi-page document, and ensure that the output file can load with Image.Load()</summary>
    [Test]
    public void TiffSplitter_TiffSplit()
    {
        int count = TestEachFile(Test, FileSpecTiffMultiPage, FileExclude);
        Assert.Pass($"Files Processed: {count}");

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
                    using var images = new MagickImageCollection(fullPath);
                    Assert.That(images, Has.Count.EqualTo(1));
                }
            }
            return split.Count > 1;
        }
    }


    /// <summary>Test will split a multi-page document, then merge it again to test the merge function.</summary>
    [Test]
    public void TiffMerger_TiffMerge()
    {
        int count = TestEachFile(Test, FileSpecTiffMultiPage, FileExclude);
        Assert.Pass($"Files Processed: {count}");

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
                using var images = new MagickImageCollection(fullPath);
                Assert.That(images, Has.Count.EqualTo(split.Count));
            }

            return true;
        }
    }


    [Test]
    public void ImageLoad_IsSuccessful_FileTests()
    {
        int count = TestEachFile(Test, FileSpecTiff, FileExclude);
        Assert.Pass($"Files Processed: {count}");

        static bool Test(FileInfo file)
        {
            using var images = new MagickImageCollection(file.FullName);
            Assert.That(images, Is.Not.Null);
            Assert.That(images.All(img => img.Format == MagickFormat.Tiff || img.Format == MagickFormat.Tif));
            foreach (var image in images)
            {
                Debug.WriteLine($"{file.Name}: {image.Compression} {image.Comment}");
            }
            Assert.That(images, Has.Count.AtLeast(1));
            return true;
        }
    }


    [Test]
    public void ImageLoad_ThrowsException_FileTests()
    {
        int count = TestEachFile(Test, FileSpecTiffFails, null);
        Assert.Pass($"Files Processed: {count}");

        bool Test(FileInfo file)
        {
            try
            {
                using var images = new MagickImageCollection(file.FullName);
                Assert.That(images, Is.Not.Null);
                Assert.That(images, Has.Count.AtLeast(1));
                Trace.WriteLine($"{file.Name}: Loaded correctly... consider renaming.");
                throw new InvalidDataException("Expected file to throw NotSupportedException or ImageFormatException (consider renaming file)");
            }
            catch (MagickCoderErrorException ex)
            {
                Debug.WriteLine($"Error {file.Name}: {ex.Message}");
                WriteFile(file.Name, File.ReadAllBytes(file.FullName));
                // Expected it to Fail...
                return true;
            }
        }
    }


}
