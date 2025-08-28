using Gloam.Core.Interfaces;

namespace Gloam.Runtime.Services;

/// <summary>
/// Manages game scenes and their layer renderers with support for global layers
/// </summary>
public sealed class SceneManager : ISceneManager
{
    private readonly ILayerRenderingManager _layerRenderingManager;
    private readonly Dictionary<string, IScene> _scenes;
    private readonly List<ILayerRenderer> _globalLayers;
    private SceneTransition? _currentTransition;

    /// <summary>
    /// Initializes a new SceneManager
    /// </summary>
    /// <param name="layerRenderingManager">The layer rendering manager to use</param>
    public SceneManager(ILayerRenderingManager layerRenderingManager)
    {
        _layerRenderingManager = layerRenderingManager ?? throw new ArgumentNullException(nameof(layerRenderingManager));
        _scenes = new Dictionary<string, IScene>();
        _globalLayers = new List<ILayerRenderer>();
    }

    /// <inheritdoc />
    public IScene? CurrentScene { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IScene> Scenes => _scenes.AsReadOnly();

    /// <inheritdoc />
    public IReadOnlyList<ILayerRenderer> GlobalLayers => _globalLayers.AsReadOnly();

    /// <inheritdoc />
    public ISceneTransition? CurrentTransition => _currentTransition;

    /// <inheritdoc />
    public void RegisterScene(IScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        if (!_scenes.TryAdd(scene.Name, scene))
        {
            throw new ArgumentException($"A scene with the name '{scene.Name}' is already registered.", nameof(scene));
        }
    }

    /// <inheritdoc />
    public bool UnregisterScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        // If we're unregistering the current scene, deactivate it first
        if (CurrentScene?.Name == sceneName)
        {
            // For sync method, we need to handle async deactivation
            var deactivateTask = CurrentScene.OnDeactivateAsync();
            if (!deactivateTask.IsCompleted)
            {
                deactivateTask.AsTask().Wait();
            }
            RemoveSceneLayers(CurrentScene);
            CurrentScene = null;
        }

        return _scenes.Remove(sceneName);
    }

    /// <inheritdoc />
    public void AddGlobalLayer(ILayerRenderer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        if (!_globalLayers.Contains(layer))
        {
            _globalLayers.Add(layer);
            _layerRenderingManager.AddLayerRenderer(layer);
        }
    }

    /// <inheritdoc />
    public bool RemoveGlobalLayer(ILayerRenderer layer)
    {
        if (layer == null)
        {
            return false;
        }

        var removed = _globalLayers.Remove(layer);
        if (removed)
        {
            _layerRenderingManager.RemoveLayerRenderer(layer);
        }

        return removed;
    }

    /// <inheritdoc />
    public async ValueTask SwitchToSceneAsync(string sceneName, CancellationToken ct = default)
    {
        await SwitchToSceneAsync(sceneName, null, ct);
    }

    /// <inheritdoc />
    public async ValueTask SwitchToSceneAsync(string sceneName, ITransition? transition, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
        }

        if (!_scenes.TryGetValue(sceneName, out var newScene))
        {
            throw new ArgumentException($"Scene '{sceneName}' is not registered.", nameof(sceneName));
        }

        // Don't switch if it's already the current scene
        if (CurrentScene?.Name == sceneName)
        {
            return;
        }

        // Create scene transition
        _currentTransition = new SceneTransition(CurrentScene, newScene, transition);
        await _currentTransition.StartAsync(ct);

        // If there's no transition effect, switch immediately
        if (transition == null)
        {
            await PerformSceneSwitchAsync(ct);
        }
        // Otherwise, the switch will happen when transition completes (in UpdateCurrentSceneAsync)
    }

    /// <inheritdoc />
    public async ValueTask UpdateCurrentSceneAsync(CancellationToken ct = default)
    {
        // Update active transition if present
        if (_currentTransition?.IsActive == true)
        {
            await _currentTransition.UpdateAsync(TimeSpan.FromMilliseconds(16), ct); // Assume ~60 FPS for now

            // Check if transition completed and we need to perform the actual scene switch
            if (_currentTransition.IsComplete)
            {
                await PerformSceneSwitchAsync(ct);
            }
        }

        // Update current scene if active
        if (CurrentScene != null && CurrentScene.IsActive)
        {
            await CurrentScene.UpdateAsync(ct);
        }
    }

    /// <summary>
    /// Performs the actual scene switch (deactivate old, activate new)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    private async ValueTask PerformSceneSwitchAsync(CancellationToken ct = default)
    {
        if (_currentTransition == null)
        {
            return;
        }

        var targetScene = _currentTransition.TargetScene;

        // Deactivate current scene
        if (CurrentScene != null)
        {
            await CurrentScene.OnDeactivateAsync(ct);
            RemoveSceneLayers(CurrentScene);
        }

        // Activate new scene
        CurrentScene = targetScene;
        AddSceneLayers(CurrentScene);
        await CurrentScene.OnActivateAsync(ct);

        // Clear the transition
        _currentTransition = null;
    }

    private void AddSceneLayers(IScene scene)
    {
        foreach (var layer in scene.Layers)
        {
            _layerRenderingManager.AddLayerRenderer(layer);
        }
    }

    private void RemoveSceneLayers(IScene scene)
    {
        foreach (var layer in scene.Layers)
        {
            _layerRenderingManager.RemoveLayerRenderer(layer);
        }
    }
}
