namespace Gloam.Core.Interfaces;

/// <summary>
/// Interface for managing game scenes and their lifecycle
/// </summary>
public interface ISceneManager
{
    /// <summary>
    /// Gets the currently active scene, if any
    /// </summary>
    IScene? CurrentScene { get; }

    /// <summary>
    /// Gets all registered scenes
    /// </summary>
    IReadOnlyDictionary<string, IScene> Scenes { get; }

    /// <summary>
    /// Gets the global layers that persist across scene changes
    /// </summary>
    IReadOnlyList<ILayerRenderer> GlobalLayers { get; }

    /// <summary>
    /// Gets the current scene transition, if any is active
    /// </summary>
    ISceneTransition? CurrentTransition { get; }

    /// <summary>
    /// Registers a scene with the manager
    /// </summary>
    /// <param name="scene">The scene to register</param>
    /// <exception cref="ArgumentException">Thrown when a scene with the same name already exists</exception>
    void RegisterScene(IScene scene);

    /// <summary>
    /// Unregisters a scene from the manager
    /// </summary>
    /// <param name="sceneName">The name of the scene to unregister</param>
    /// <returns>True if the scene was found and removed, false otherwise</returns>
    bool UnregisterScene(string sceneName);

    /// <summary>
    /// Adds a global layer that will persist across scene changes
    /// </summary>
    /// <param name="layer">The global layer to add</param>
    void AddGlobalLayer(ILayerRenderer layer);

    /// <summary>
    /// Removes a global layer
    /// </summary>
    /// <param name="layer">The global layer to remove</param>
    /// <returns>True if the layer was found and removed, false otherwise</returns>
    bool RemoveGlobalLayer(ILayerRenderer layer);

    /// <summary>
    /// Switches to the specified scene
    /// </summary>
    /// <param name="sceneName">The name of the scene to switch to</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the scene switch operation</returns>
    /// <exception cref="ArgumentException">Thrown when the specified scene is not found</exception>
    ValueTask SwitchToSceneAsync(string sceneName, CancellationToken ct = default);

    /// <summary>
    /// Switches to the specified scene with a transition effect
    /// </summary>
    /// <param name="sceneName">The name of the scene to switch to</param>
    /// <param name="transition">The transition effect to use</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the scene switch operation</returns>
    /// <exception cref="ArgumentException">Thrown when the specified scene is not found</exception>
    ValueTask SwitchToSceneAsync(string sceneName, ITransition? transition, CancellationToken ct = default);

    /// <summary>
    /// Updates the current scene if one is active
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the update operation</returns>
    ValueTask UpdateCurrentSceneAsync(CancellationToken ct = default);
}