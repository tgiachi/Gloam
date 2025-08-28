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
    private IScene? _currentScene;

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
    public IScene? CurrentScene => _currentScene;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IScene> Scenes => _scenes.AsReadOnly();

    /// <inheritdoc />
    public IReadOnlyList<ILayerRenderer> GlobalLayers => _globalLayers.AsReadOnly();

    /// <inheritdoc />
    public void RegisterScene(IScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        if (_scenes.ContainsKey(scene.Name))
        {
            throw new ArgumentException($"A scene with the name '{scene.Name}' is already registered.", nameof(scene));
        }

        _scenes[scene.Name] = scene;
    }

    /// <inheritdoc />
    public bool UnregisterScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        // If we're unregistering the current scene, deactivate it first
        if (_currentScene?.Name == sceneName)
        {
            // For sync method, we need to handle async deactivation
            var deactivateTask = _currentScene.OnDeactivateAsync();
            if (!deactivateTask.IsCompleted)
            {
                deactivateTask.AsTask().Wait();
            }
            RemoveSceneLayers(_currentScene);
            _currentScene = null;
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
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
        }

        if (!_scenes.TryGetValue(sceneName, out var newScene))
        {
            throw new ArgumentException($"Scene '{sceneName}' is not registered.", nameof(sceneName));
        }

        // Don't switch if it's already the current scene
        if (_currentScene?.Name == sceneName)
        {
            return;
        }

        // Deactivate current scene
        if (_currentScene != null)
        {
            await _currentScene.OnDeactivateAsync(ct);
            RemoveSceneLayers(_currentScene);
        }

        // Activate new scene
        _currentScene = newScene;
        AddSceneLayers(_currentScene);
        await _currentScene.OnActivateAsync(ct);
    }

    /// <inheritdoc />
    public async ValueTask UpdateCurrentSceneAsync(CancellationToken ct = default)
    {
        if (_currentScene != null && _currentScene.IsActive)
        {
            await _currentScene.UpdateAsync(ct);
        }
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