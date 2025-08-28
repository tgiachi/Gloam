using Gloam.Core.Interfaces;
using Gloam.Runtime.Types;

namespace Gloam.Runtime.Interfaces;

/// <summary>
///     Represents the main host interface for the Gloam runtime environment.
/// </summary>
public interface IGloamHost : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the current state of the host
    /// </summary>
    HostState State { get; }

    /// <summary>
    /// Initializes the host asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the initialization operation</returns>
    ValueTask InitializeAsync(CancellationToken ct = default);

    /// <summary>
    /// Loads content from the specified root directory
    /// </summary>
    /// <param name="contentRoot">The root directory for content loading</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A ValueTask representing the content loading operation</returns>
    ValueTask LoadContentAsync(string contentRoot, CancellationToken ct = default);

    /// <summary>
    /// Starts the host and begins the game loop
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the start operation</returns>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs the game loop with the specified timing and condition
    /// </summary>
    /// <param name="keepRunning">Function that returns true while the game should continue running</param>
    /// <param name="fixedStep">Fixed time step for the game loop</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the game loop execution</returns>
    Task RunAsync(Func<bool> keepRunning, TimeSpan fixedStep, CancellationToken ct);

    /// <summary>
    /// Stops the host and game loop gracefully
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A Task representing the stop operation</returns>
    Task StopAsync(CancellationToken ct = default);

    void SetRenderer(IRenderer renderer);

    void SetInputDevice(IInputDevice inputDevice);
}
