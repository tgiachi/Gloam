using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Core.Interfaces.Base;

namespace Gloam.Console.Render.Layers;

/// <summary>
/// Layer renderer for scene transitions - renders on top of everything
/// </summary>
public sealed class TransitionLayer : BaseLayerRenderer
{
    private readonly ISceneManager _sceneManager;

    /// <summary>
    /// Initializes a new transition layer
    /// </summary>
    /// <param name="sceneManager">The scene manager to get transition from</param>
    public TransitionLayer(ISceneManager sceneManager)
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
    }

    /// <inheritdoc />
    public override int Priority => 1000; // Render on top of everything

    /// <inheritdoc />
    public override string Name => "Transition";

    /// <inheritdoc />
    protected override async ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        // Only render if there's an active transition with an effect
        var currentTransition = _sceneManager.CurrentTransition;
        if (currentTransition?.IsActive == true && currentTransition.Transition != null)
        {
            await currentTransition.Transition.RenderAsync(context, ct);
        }
    }
}