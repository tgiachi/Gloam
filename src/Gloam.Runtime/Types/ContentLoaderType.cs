namespace Gloam.Runtime.Types;

/// <summary>
///     Specifies the type of content loader to use for game asset loading
/// </summary>
public enum ContentLoaderType
{
    /// <summary>Load content from the file system</summary>
    FileSystem,

    /// <summary>Load content from embedded resources in assemblies</summary>
    EmbeddedResource,

    /// <summary>Load content from network sources</summary>
    Network,

    /// <summary>Load content from ZIP archive files</summary>
    ZipArchive
}
