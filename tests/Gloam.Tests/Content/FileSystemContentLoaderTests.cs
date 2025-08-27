using Gloam.Data.Content;

namespace Gloam.Tests.Content;

/// <summary>
///     Tests for FileSystemContentLoader functionality.
/// </summary>
public class FileSystemContentLoaderTests
{
    private FileSystemContentLoader _loader = null!;
    private string _tempDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _loader = new FileSystemContentLoader(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithValidPath_ShouldSucceed()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);

        try
        {
            Assert.DoesNotThrow(() => new FileSystemContentLoader(tempPath));
        }
        finally
        {
            Directory.Delete(tempPath);
        }
    }

    [Test]
    public void Constructor_WithNullPath_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => new FileSystemContentLoader(null!));
    }

    [Test]
    public void Constructor_WithEmptyPath_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FileSystemContentLoader(""));
    }

    [Test]
    public void Constructor_WithWhitespacePath_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FileSystemContentLoader("   "));
    }

    [Test]
    public void Constructor_WithNonExistentPath_ShouldThrowDirectoryNotFoundException()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Assert.Throws<DirectoryNotFoundException>(() => new FileSystemContentLoader(nonExistentPath));
    }

    #endregion

    #region ReadTextAsync Tests

    [Test]
    public async Task ReadTextAsync_WithValidFile_ShouldReturnContent()
    {
        var fileName = "test.txt";
        var content = "Hello, World!";
        var filePath = Path.Combine(_tempDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);

        var result = await _loader.ReadTextAsync(fileName);

        Assert.That(result, Is.EqualTo(content));
    }

    [Test]
    public async Task ReadTextAsync_WithSubdirectory_ShouldReturnContent()
    {
        var subDir = "subdir";
        var fileName = "test.txt";
        var content = "Subdirectory content";
        var subDirPath = Path.Combine(_tempDirectory, subDir);
        Directory.CreateDirectory(subDirPath);

        var filePath = Path.Combine(subDirPath, fileName);
        await File.WriteAllTextAsync(filePath, content);

        var result = await _loader.ReadTextAsync(Path.Combine(subDir, fileName));

        Assert.That(result, Is.EqualTo(content));
    }

    [Test]
    public void ReadTextAsync_WithNullPath_ShouldThrowArgumentException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _loader.ReadTextAsync(null!));
    }

    [Test]
    public void ReadTextAsync_WithEmptyPath_ShouldThrowArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _loader.ReadTextAsync(""));
    }

    [Test]
    public void ReadTextAsync_WithWhitespacePath_ShouldThrowArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _loader.ReadTextAsync("   "));
    }

    [Test]
    public void ReadTextAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _loader.ReadTextAsync("nonexistent.txt"));
    }

    [Test]
    public void ReadTextAsync_WithPathTraversalAttack_ShouldThrowUnauthorizedAccessException()
    {
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _loader.ReadTextAsync("../../../etc/passwd"));
    }

    [Test]
    public void ReadTextAsync_WithAbsolutePath_ShouldThrowUnauthorizedAccessException()
    {
        var absolutePath = "/etc/passwd"; // Unix absolute path
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _loader.ReadTextAsync(absolutePath));
    }

    [Test]
    public async Task ReadTextAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        var fileName = "test.txt";
        var content = "Test content";
        var filePath = Path.Combine(_tempDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _loader.ReadTextAsync(fileName, cts.Token)
        );
    }

    #endregion

    #region EnumerateFilesAsync Tests

    [Test]
    public async Task EnumerateFilesAsync_WithMatchingFiles_ShouldReturnFiles()
    {
        // Create test files
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "test1.txt"), "content1");
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "test2.txt"), "content2");
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "other.log"), "log content");

        var files = new List<string>();
        await foreach (var file in _loader.EnumerateFilesAsync(".", "*.txt"))
        {
            files.Add(file);
        }

        Assert.That(files.Count, Is.EqualTo(2));
        Assert.That(files, Contains.Item("test1.txt"));
        Assert.That(files, Contains.Item("test2.txt"));
        Assert.That(files, Does.Not.Contain("other.log"));
    }

    [Test]
    public async Task EnumerateFilesAsync_WithSubdirectories_ShouldReturnAllMatchingFiles()
    {
        // Create test files in subdirectories
        var subDir1 = Path.Combine(_tempDirectory, "sub1");
        var subDir2 = Path.Combine(_tempDirectory, "sub2");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "root.txt"), "root content");
        await File.WriteAllTextAsync(Path.Combine(subDir1, "sub1.txt"), "sub1 content");
        await File.WriteAllTextAsync(Path.Combine(subDir2, "sub2.txt"), "sub2 content");

        var files = new List<string>();
        await foreach (var file in _loader.EnumerateFilesAsync(".", "*.txt"))
        {
            files.Add(file);
        }

        Assert.That(files.Count, Is.EqualTo(3));
        Assert.That(files, Contains.Item("root.txt"));
        Assert.That(files, Contains.Item(Path.Combine("sub1", "sub1.txt")));
        Assert.That(files, Contains.Item(Path.Combine("sub2", "sub2.txt")));
    }

    [Test]
    public async Task EnumerateFilesAsync_WithSpecificSubdirectory_ShouldReturnSubdirectoryFiles()
    {
        var subDir = Path.Combine(_tempDirectory, "subdir");
        Directory.CreateDirectory(subDir);

        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "root.txt"), "root content");
        await File.WriteAllTextAsync(Path.Combine(subDir, "sub.txt"), "sub content");

        var files = new List<string>();
        await foreach (var file in _loader.EnumerateFilesAsync("subdir", "*.txt"))
        {
            files.Add(file);
        }

        Assert.That(files.Count, Is.EqualTo(1));
        Assert.That(files[0], Does.EndWith("sub.txt"));
    }

    [Test]
    public void EnumerateFilesAsync_WithNullRoot_ShouldThrowArgumentException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var _ in _loader.EnumerateFilesAsync(null!, "*.txt"))
                {
                }
            }
        );
    }

    [Test]
    public void EnumerateFilesAsync_WithEmptyRoot_ShouldThrowArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await foreach (var _ in _loader.EnumerateFilesAsync("", "*.txt"))
                {
                }
            }
        );
    }

    [Test]
    public void EnumerateFilesAsync_WithNullSearchPattern_ShouldThrowArgumentException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await foreach (var _ in _loader.EnumerateFilesAsync(".", null!))
                {
                }
            }
        );
    }

    [Test]
    public void EnumerateFilesAsync_WithEmptySearchPattern_ShouldThrowArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await foreach (var _ in _loader.EnumerateFilesAsync(".", ""))
                {
                }
            }
        );
    }

    [Test]
    public void EnumerateFilesAsync_WithNonExistentDirectory_ShouldThrowDirectoryNotFoundException()
    {
        Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            {
                await foreach (var _ in _loader.EnumerateFilesAsync("nonexistent", "*.txt"))
                {
                }
            }
        );
    }

    [Test]
    public void EnumerateFilesAsync_WithPathTraversalAttack_ShouldThrowUnauthorizedAccessException()
    {
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await foreach (var _ in _loader.EnumerateFilesAsync("../../../", "*"))
                {
                }
            }
        );
    }

    [Test]
    public async Task EnumerateFilesAsync_WithNoMatchingFiles_ShouldReturnEmpty()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "test.log"), "log content");

        var files = new List<string>();
        await foreach (var file in _loader.EnumerateFilesAsync(".", "*.txt"))
        {
            files.Add(file);
        }

        Assert.That(files, Is.Empty);
    }

    #endregion

    #region Security Tests

    [Test]
    public void GetSafePath_WithPathTraversalAttempts_ShouldThrowUnauthorizedAccessException()
    {
        // Create a subdirectory to make the path traversal attempt realistic
        var subDir = Path.Combine(_tempDirectory, "subdir", "deeper");
        Directory.CreateDirectory(subDir);
        var subLoader = new FileSystemContentLoader(subDir);

        // These paths attempt to traverse outside the subdirectory
        var dangerousPaths = new[]
        {
            "../../../etc/passwd",          // Traverse up from subdir/deeper
            "../../../../../../etc/passwd", // Traverse way up
            "../../../../../tmp/test"       // Another traversal attempt
        };

        foreach (var dangerousPath in dangerousPaths)
        {
            // These paths should trigger UnauthorizedAccessException from GetSafePath
            Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () =>
                    await subLoader.ReadTextAsync(dangerousPath),
                $"Path '{dangerousPath}' should be blocked"
            );
        }
    }

    [Test]
    public async Task GetSafePath_WithMixedPathStyles_ShouldHandleCorrectly()
    {
        var testPaths = new[]
        {
            "/etc/passwd",                       // Absolute Unix path
            "C:\\Windows\\System32\\config\\sam" // Absolute Windows path
        };

        foreach (var testPath in testPaths)
        {
            // These should either be blocked or treated as relative paths that don't exist
            Exception? exception = null;
            try
            {
                await _loader.ReadTextAsync(testPath);
                Assert.Fail($"Path '{testPath}' should have been blocked or not found");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Should be either UnauthorizedAccessException or FileNotFoundException
            Assert.That(
                exception,
                Is.InstanceOf<UnauthorizedAccessException>().Or.InstanceOf<FileNotFoundException>(),
                $"Path '{testPath}' should be handled safely, but got {exception?.GetType().Name}"
            );
        }
    }

    #endregion
}
