using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CodeMindful.Imaging.Tests.TiffTests;

[DebuggerStepThrough]
public abstract class TiffBase
{
    public const string FileSpecTiff = "*.tif*";
    public const string FileSpecTiffMultiPage = "*PgX*.tif*";

    protected DirectoryInfo TiffFiles { get; } = new(Path.Combine(Environment.CurrentDirectory, "TestFiles", "Tiff"));


    protected ITiffMerge TiffMerger { get; set; } 
    protected ITiffSplit TiffSplitter { get; set; } 

    protected TiffBase(ITiffSplit tiffSplitter, ITiffMerge tiffMerger)
    {
        TiffSplitter = tiffSplitter;
        TiffMerger = tiffMerger;
    }

    protected void TestEachFile(Func<FileInfo, bool> test, string searchPattern, string? excludeContains)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNullOrEmpty(searchPattern, nameof(searchPattern));

        var files = TiffFiles.GetFiles(searchPattern, SearchOption.AllDirectories);
        var errors = new List<string>();

        Func<FileInfo, bool> filter =
            string.IsNullOrWhiteSpace(excludeContains) ?
            (file) => file.Exists : 
            (file) => file.Exists && !file.Name.Contains(excludeContains, StringComparison.CurrentCultureIgnoreCase);


        foreach (FileInfo file in files)
        {
            try
            {
                if (filter(file))
                {
                    bool passed = test.Invoke(file);
                    if (!passed)
                    {
                        errors.Add(file.FullName + " failed the test");  
                    }

                }

            }
            catch (NotImplementedException ex)
            {
                Assert.Inconclusive(ex.Message);
            }
            catch (Exception ex)
            {
                errors.Add($"{file.FullName}: ({ex.GetType().Name}) {ex.Message}");
            }
        }
        if (errors.Count > 0)
        {
            var errmsg = string.Join(Environment.NewLine, errors);
            Assert.Fail(errmsg);
        }
    }


    /// <summary>Build a file name including the Test Class Prefix</summary>
    protected static string CreateFileName(FileInfo file, string suffix)
    {
        string fileNameNoExt = Path.GetFileNameWithoutExtension(file.Name);

        string fileName = $"{fileNameNoExt}_{suffix}{file.Extension}";
        return fileName;
    }

    /// <summary>Write a file to the TestResults directory</summary>
    /// <returns>Full path to filename</returns>
    protected string WriteFile(string name, byte[] bytes, [CallerMemberName] string? methodName = null)
    {
        var outFolder = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "TestResults", GetType().Name, methodName ?? string.Empty));
        if (!outFolder.Exists)
        {
            outFolder.Create();
        }

        // strip any path information.
        name = Path.GetFileName(name);

        var outFile = Path.Combine(outFolder.FullName, name);
        File.Delete(outFile);
        File.WriteAllBytes(outFile, bytes);
        return outFile;
    }
}