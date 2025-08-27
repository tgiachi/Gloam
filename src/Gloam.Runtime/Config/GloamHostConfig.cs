using Gloam.Runtime.Types;

namespace Gloam.Runtime.Config;

public class GloamHostConfig
{
    public string RootDirectory { get; set; }

    public ContentLoaderType LoaderType { get; set; } = ContentLoaderType.FileSystem;
}
