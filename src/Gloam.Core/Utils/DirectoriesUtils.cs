namespace Gloam.Core.Utils;

public class DirectoriesUtils
{
    public static string[] GetFiles(string path, params string[] extensions)
    {
        return GetFiles(path, true, extensions);
    }

    public static string[] GetFiles(string path, bool recursive, params string[] extensions)
    {
        if (!Directory.Exists(path))
        {
            return [];
        }

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = new List<string>();

        if (extensions == null || extensions.Length == 0)
        {
            return Directory.GetFiles(path, "*", searchOption);
        }

        foreach (var extension in extensions)
        {
            files.AddRange(Directory.GetFiles(path, extension, searchOption));
        }

        return files.ToArray();
    }
}
