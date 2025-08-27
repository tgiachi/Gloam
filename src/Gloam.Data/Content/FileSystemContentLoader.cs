using Gloam.Data.Interfaces.Content;

namespace Gloam.Data.Content;

/// <summary>
/// Implementation of IContentLoader that loads content from the file system with security restrictions
/// </summary>
public class FileSystemContentLoader : IContentLoader
{
    private readonly string _basePath;

    public FileSystemContentLoader(string basePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        _basePath = Path.GetFullPath(basePath);

        if (!Directory.Exists(_basePath))
        {
            throw new DirectoryNotFoundException($"Base directory '{_basePath}' does not exist.");
        }
    }

    public async ValueTask<string> ReadTextAsync(string relativePath, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        // Resolve the full path and ensure it's within the base directory
        var fullPath = GetSafePath(relativePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File '{relativePath}' not found.");
        }

        return await File.ReadAllTextAsync(fullPath, ct).ConfigureAwait(false);
    }

    public IAsyncEnumerable<string> EnumerateFilesAsync(string root, string searchPattern, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        // Resolve the root path safely
        var rootPath = GetSafePath(root);

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Directory '{root}' does not exist.");
        }

        var files = Directory.EnumerateFiles(rootPath, searchPattern, SearchOption.AllDirectories);

        // Convert absolute paths back to relative paths and return as async enumerable
        return ConvertToAsyncEnumerable(files.Select(file => Path.GetRelativePath(_basePath, file)));
    }

    private string GetSafePath(string relativePath)
    {
        // Normalize the path and combine with base path
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath));

        // Security check: ensure the resolved path is within the base directory
        return !fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase)
            ? throw new UnauthorizedAccessException(
                $"Access to path '{relativePath}' is not allowed outside the base directory."
            )
            : fullPath;
    }

    private static async IAsyncEnumerable<string> ConvertToAsyncEnumerable(IEnumerable<string> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield(); // Allow other async operations to proceed
        }
    }
}
