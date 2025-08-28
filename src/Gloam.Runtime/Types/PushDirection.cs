namespace Gloam.Runtime.Types;

/// <summary>
/// Push direction enumeration for scene transitions
/// </summary>
public enum PushDirection
{
    /// <summary>
    /// New scene enters from the left, pushing current scene to the right
    /// </summary>
    FromLeft,

    /// <summary>
    /// New scene enters from the right, pushing current scene to the left
    /// </summary>
    FromRight,

    /// <summary>
    /// New scene enters from the top, pushing current scene down
    /// </summary>
    FromTop,

    /// <summary>
    /// New scene enters from the bottom, pushing current scene up
    /// </summary>
    FromBottom
}