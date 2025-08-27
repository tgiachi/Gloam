using Gloam.Runtime.Types;

namespace Gloam.Runtime.Interfaces;

/// <summary>
///     Represents the main host interface for the Gloam runtime environment.
/// </summary>
public interface IGloamHost : IAsyncDisposable, IDisposable
{
    HostState State { get; }
    ValueTask InitializeAsync(CancellationToken ct = default);
    ValueTask LoadContentAsync(string contentRoot, CancellationToken ct = default);

    Task StartAsync(CancellationToken ct = default);

    Task RunAsync(Func<bool> keepRunning, TimeSpan fixedStep, CancellationToken ct);
    Task StopAsync(CancellationToken ct = default);
}
