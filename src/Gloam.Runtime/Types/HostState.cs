namespace Gloam.Runtime.Types;

/// <summary>
///     Represents the different states of the Gloam runtime host lifecycle
/// </summary>
public enum HostState
{
    /// <summary>Host has been created but not yet initialized</summary>
    Created,

    /// <summary>Host has been initialized and is ready for content loading</summary>
    Initialized,

    /// <summary>Content has been loaded and validated</summary>
    ContentLoaded,

    /// <summary>Game session has been created</summary>
    SessionCreated,

    /// <summary>Host is actively running the game loop</summary>
    Running,

    /// <summary>Host is paused but can be resumed</summary>
    Paused,

    /// <summary>Host has been stopped</summary>
    Stopped,

    /// <summary>Host has been disposed and cannot be reused</summary>
    Disposed
}
