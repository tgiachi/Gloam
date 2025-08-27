namespace Gloam.Data.Interfaces.Content;

/// <summary>
///     Provides functionality for accessing content from various sources.
/// </summary>
public interface IContentLoader
{
    /// <summary>
    ///     Asynchronously reads text content from a file at the specified relative path.
    /// </summary>
    /// <param name="relativePath">The relative path to the file to read.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A ValueTask containing the text content of the file.</returns>
    ValueTask<string> ReadTextAsync(string relativePath, CancellationToken ct = default);

    /// <summary>
    ///     Asynchronously enumerates files matching the search pattern in the specified root directory.
    /// </summary>
    /// <param name="root">The root directory to search in.</param>
    /// <param name="searchPattern">The pattern to match files against.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>An async enumerable of file paths.</returns>
    IAsyncEnumerable<string> EnumerateFilesAsync(string root, string searchPattern, CancellationToken ct = default);
}
