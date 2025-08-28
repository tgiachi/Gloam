using System.Diagnostics;
using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Core.Interfaces.Base;
using Gloam.Core.Primitives;
using Moq;

namespace Gloam.Tests.Interfaces.Base;

// Test implementation of the abstract BaseLayerRenderer
public class TestLayerRenderer : BaseLayerRenderer
{
    public TestLayerRenderer(int priority, string name)
    {
        Priority = priority;
        Name = name;
    }

    public override int Priority { get; }
    public override string Name { get; }

    public bool PreRenderCalled { get; private set; }
    public bool PostRenderCalled { get; private set; }
    public bool RenderLayerCalled { get; private set; }
    public int RenderCallCount { get; private set; }

    public bool ShouldThrowInPreRender { get; set; }
    public bool ShouldThrowInRenderLayer { get; set; }
    public bool ShouldThrowInPostRender { get; set; }

    public TimeSpan PreRenderDelay { get; set; } = TimeSpan.Zero;
    public TimeSpan RenderLayerDelay { get; set; } = TimeSpan.Zero;
    public TimeSpan PostRenderDelay { get; set; } = TimeSpan.Zero;

    public void Reset()
    {
        PreRenderCalled = false;
        PostRenderCalled = false;
        RenderLayerCalled = false;
        RenderCallCount = 0;
        ShouldThrowInPreRender = false;
        ShouldThrowInRenderLayer = false;
        ShouldThrowInPostRender = false;
        PreRenderDelay = TimeSpan.Zero;
        RenderLayerDelay = TimeSpan.Zero;
        PostRenderDelay = TimeSpan.Zero;
    }

    protected override async ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (ShouldThrowInRenderLayer)
        {
            throw new InvalidOperationException("RenderLayer error");
        }

        if (RenderLayerDelay > TimeSpan.Zero)
        {
            await Task.Delay(RenderLayerDelay, ct);
        }

        RenderLayerCalled = true;
        RenderCallCount++;
    }

    protected override async ValueTask OnPreRenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (ShouldThrowInPreRender)
        {
            throw new InvalidOperationException("PreRender error");
        }

        if (PreRenderDelay > TimeSpan.Zero)
        {
            await Task.Delay(PreRenderDelay, ct);
        }

        PreRenderCalled = true;
    }

    protected override async ValueTask OnPostRenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (ShouldThrowInPostRender)
        {
            throw new InvalidOperationException("PostRender error");
        }

        if (PostRenderDelay > TimeSpan.Zero)
        {
            await Task.Delay(PostRenderDelay, ct);
        }

        PostRenderCalled = true;
    }
}

public class TestMinimalLayerRenderer : BaseLayerRenderer
{
    public override int Priority => 0;
    public override string Name => "MinimalRenderer";

    public bool RenderLayerCalled { get; private set; }

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        RenderLayerCalled = true;
        return ValueTask.CompletedTask;
    }
}

public class BaseLayerRendererTests
{
    private RenderLayerContext _context = null!;
    private Mock<IInputDevice> _mockInputDevice = null!;
    private Mock<IRenderer> _mockRenderer = null!;
    private Mock<IRenderSurface> _mockSurface = null!;
    private TestLayerRenderer _renderer = null!;

    [SetUp]
    public void SetUp()
    {
        _renderer = new TestLayerRenderer(1, "TestRenderer");

        // Create real context for testing
        _mockRenderer = new Mock<IRenderer>();
        _mockSurface = new Mock<IRenderSurface>();
        _mockInputDevice = new Mock<IInputDevice>();

        _mockRenderer.Setup(r => r.Surface).Returns(_mockSurface.Object);
        _mockSurface.Setup(s => s.Width).Returns(800);
        _mockSurface.Setup(s => s.Height).Returns(600);

        var frameInfo = new FrameInfo(1, TimeSpan.FromMilliseconds(16.67), TimeSpan.FromSeconds(1), 60f);
        _context = new RenderLayerContext(_mockRenderer.Object, _mockInputDevice.Object, frameInfo, new Size(800, 600));
    }

    [Test]
    public void Properties_ShouldReturnCorrectValues()
    {
        Assert.That(_renderer.Priority, Is.EqualTo(1));
        Assert.That(_renderer.Name, Is.EqualTo("TestRenderer"));
    }

    [Test]
    public async Task RenderAsync_ShouldCallAllPhases()
    {
        await _renderer.RenderAsync(_context);

        Assert.That(_renderer.PreRenderCalled, Is.True);
        Assert.That(_renderer.RenderLayerCalled, Is.True);
        Assert.That(_renderer.PostRenderCalled, Is.True);
    }

    [Test]
    public async Task RenderAsync_ShouldCallPhasesInCorrectOrder()
    {
        var callOrder = new List<string>();
        var renderer = new TestOrderedLayerRenderer(callOrder);

        await renderer.RenderAsync(_context);

        Assert.That(callOrder, Has.Count.EqualTo(3));
        Assert.That(callOrder[0], Is.EqualTo("PreRender"));
        Assert.That(callOrder[1], Is.EqualTo("RenderLayer"));
        Assert.That(callOrder[2], Is.EqualTo("PostRender"));
    }

    [Test]
    public async Task RenderAsync_WithCancellationToken_ShouldPassToAllPhases()
    {
        using var cts = new CancellationTokenSource();

        await _renderer.RenderAsync(_context, cts.Token);

        Assert.That(_renderer.PreRenderCalled, Is.True);
        Assert.That(_renderer.RenderLayerCalled, Is.True);
        Assert.That(_renderer.PostRenderCalled, Is.True);
    }

    [Test]
    public async Task RenderAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _renderer.RenderAsync(_context, cts.Token)
        );
    }

    [Test]
    public async Task RenderAsync_PreRenderThrows_ShouldNotCallSubsequentPhases()
    {
        _renderer.ShouldThrowInPreRender = true;

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _renderer.RenderAsync(_context)
        );

        Assert.That(_renderer.PreRenderCalled, Is.False); // Never set to true due to exception
        Assert.That(_renderer.RenderLayerCalled, Is.False);
        Assert.That(_renderer.PostRenderCalled, Is.False);
    }

    [Test]
    public async Task RenderAsync_RenderLayerThrows_ShouldNotCallPostRender()
    {
        _renderer.ShouldThrowInRenderLayer = true;

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _renderer.RenderAsync(_context)
        );

        Assert.That(_renderer.PreRenderCalled, Is.True);
        Assert.That(_renderer.RenderLayerCalled, Is.False); // Never set to true due to exception
        Assert.That(_renderer.PostRenderCalled, Is.False);
    }

    [Test]
    public async Task RenderAsync_PostRenderThrows_ShouldPropagateException()
    {
        _renderer.ShouldThrowInPostRender = true;

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _renderer.RenderAsync(_context)
        );

        Assert.That(_renderer.PreRenderCalled, Is.True);
        Assert.That(_renderer.RenderLayerCalled, Is.True);
        Assert.That(_renderer.PostRenderCalled, Is.False); // Never set to true due to exception
    }

    [Test]
    public async Task RenderAsync_MultipleCallsWithoutReset_ShouldIncrementCallCount()
    {
        await _renderer.RenderAsync(_context);
        await _renderer.RenderAsync(_context);
        await _renderer.RenderAsync(_context);

        Assert.That(_renderer.RenderCallCount, Is.EqualTo(3));
    }

    [Test]
    public async Task RenderAsync_WithDelays_ShouldWaitForCompletion()
    {
        _renderer.PreRenderDelay = TimeSpan.FromMilliseconds(10);
        _renderer.RenderLayerDelay = TimeSpan.FromMilliseconds(10);
        _renderer.PostRenderDelay = TimeSpan.FromMilliseconds(10);

        var stopwatch = Stopwatch.StartNew();
        await _renderer.RenderAsync(_context);
        stopwatch.Stop();

        Assert.That(_renderer.PreRenderCalled, Is.True);
        Assert.That(_renderer.RenderLayerCalled, Is.True);
        Assert.That(_renderer.PostRenderCalled, Is.True);
        Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(25)); // At least 30ms total
    }

    [Test]
    public async Task RenderAsync_WithCancellationDuringPreRender_ShouldStopExecution()
    {
        _renderer.PreRenderDelay = TimeSpan.FromMilliseconds(50);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _renderer.RenderAsync(_context, cts.Token)
        );

        // May or may not have been called depending on timing
        Assert.That(_renderer.RenderLayerCalled, Is.False);
        Assert.That(_renderer.PostRenderCalled, Is.False);
    }

    [Test]
    public async Task RenderAsync_WithCancellationDuringRenderLayer_ShouldStopExecution()
    {
        _renderer.RenderLayerDelay = TimeSpan.FromMilliseconds(50);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _renderer.RenderAsync(_context, cts.Token)
        );

        Assert.That(_renderer.PreRenderCalled, Is.True);
        Assert.That(_renderer.PostRenderCalled, Is.False);
    }

    [Test]
    public void IsFirstFrame_WithFrameNumberZero_ShouldReturnTrue()
    {
        var frameInfo = new FrameInfo(0, TimeSpan.FromMilliseconds(16.67), TimeSpan.FromSeconds(1), 60f);
        var context = new RenderLayerContext(_mockRenderer.Object, _mockInputDevice.Object, frameInfo, new Size(800, 600));

        Assert.That(TestFirstFrameRenderer.TestIsFirstFrame(context), Is.True);
    }

    [Test]
    public void IsFirstFrame_WithFrameNumberGreaterThanZero_ShouldReturnFalse()
    {
        var frameInfo = new FrameInfo(1, TimeSpan.FromMilliseconds(16.67), TimeSpan.FromSeconds(1), 60f);
        var context = new RenderLayerContext(_mockRenderer.Object, _mockInputDevice.Object, frameInfo, new Size(800, 600));

        Assert.That(TestFirstFrameRenderer.TestIsFirstFrame(context), Is.False);
    }

    [Test]
    public void GetFramesPerSecond_ShouldReturnCorrectFPS()
    {
        var frameInfo = new FrameInfo(1, TimeSpan.FromMilliseconds(16.67), TimeSpan.FromSeconds(1), 60f);
        var context = new RenderLayerContext(_mockRenderer.Object, _mockInputDevice.Object, frameInfo, new Size(800, 600));

        Assert.That(TestFPSRenderer.TestGetFramesPerSecond(context), Is.EqualTo(60f));
    }

    [Test]
    public void GetFramesPerSecond_WithZeroFPS_ShouldReturnZero()
    {
        var frameInfo = new FrameInfo(0, TimeSpan.FromMilliseconds(16.67), TimeSpan.FromSeconds(1), 0f);
        var context = new RenderLayerContext(_mockRenderer.Object, _mockInputDevice.Object, frameInfo, new Size(800, 600));

        Assert.That(TestFPSRenderer.TestGetFramesPerSecond(context), Is.EqualTo(0f));
    }

    [Test]
    public async Task MinimalImplementation_ShouldWork()
    {
        var minimalRenderer = new TestMinimalLayerRenderer();

        await minimalRenderer.RenderAsync(_context);

        Assert.That(minimalRenderer.RenderLayerCalled, Is.True);
    }

    [Test]
    public async Task DefaultPreRender_ShouldCompleteWithoutError()
    {
        var minimalRenderer = new TestMinimalLayerRenderer();

        // Default implementation should not throw
        Assert.DoesNotThrowAsync(async () => await minimalRenderer.RenderAsync(_context));
    }

    [Test]
    public async Task DefaultPostRender_ShouldCompleteWithoutError()
    {
        var minimalRenderer = new TestMinimalLayerRenderer();

        // Default implementation should not throw
        Assert.DoesNotThrowAsync(async () => await minimalRenderer.RenderAsync(_context));
    }

    [Test]
    public void Priority_ShouldBeAbstractAndRequireImplementation()
    {
        // Test that different implementations can have different priorities
        var renderer1 = new TestLayerRenderer(10, "High");
        var renderer2 = new TestLayerRenderer(1, "Low");

        Assert.That(renderer1.Priority, Is.EqualTo(10));
        Assert.That(renderer2.Priority, Is.EqualTo(1));
        Assert.That(renderer1.Priority, Is.GreaterThan(renderer2.Priority));
    }

    [Test]
    public void Name_ShouldBeAbstractAndRequireImplementation()
    {
        var renderer1 = new TestLayerRenderer(1, "Renderer1");
        var renderer2 = new TestLayerRenderer(1, "Renderer2");

        Assert.That(renderer1.Name, Is.EqualTo("Renderer1"));
        Assert.That(renderer2.Name, Is.EqualTo("Renderer2"));
        Assert.That(renderer1.Name, Is.Not.EqualTo(renderer2.Name));
    }

    [Test]
    public async Task ConcurrentRenderCalls_ShouldBeIndependent()
    {
        var renderer1 = new TestLayerRenderer(1, "Renderer1");
        var renderer2 = new TestLayerRenderer(2, "Renderer2");

        renderer1.RenderLayerDelay = TimeSpan.FromMilliseconds(20);
        renderer2.RenderLayerDelay = TimeSpan.FromMilliseconds(20);

        var task1 = renderer1.RenderAsync(_context);
        var task2 = renderer2.RenderAsync(_context);

        await Task.WhenAll(task1.AsTask(), task2.AsTask());

        Assert.That(renderer1.RenderLayerCalled, Is.True);
        Assert.That(renderer2.RenderLayerCalled, Is.True);
    }

    [Test]
    public async Task ResetBetweenCalls_ShouldWorkCorrectly()
    {
        await _renderer.RenderAsync(_context);

        Assert.That(_renderer.RenderCallCount, Is.EqualTo(1));

        _renderer.Reset();

        Assert.That(_renderer.PreRenderCalled, Is.False);
        Assert.That(_renderer.RenderLayerCalled, Is.False);
        Assert.That(_renderer.PostRenderCalled, Is.False);
        Assert.That(_renderer.RenderCallCount, Is.EqualTo(0));

        await _renderer.RenderAsync(_context);

        Assert.That(_renderer.RenderCallCount, Is.EqualTo(1));
    }
}

// Helper test renderers to access protected methods
public class TestFirstFrameRenderer : BaseLayerRenderer
{
    public override int Priority => 0;
    public override string Name => "TestFirstFrameRenderer";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public static bool TestIsFirstFrame(RenderLayerContext context)
    {
        return IsFirstFrame(context);
    }
}

public class TestFPSRenderer : BaseLayerRenderer
{
    public override int Priority => 0;
    public override string Name => "TestFPSRenderer";

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        return ValueTask.CompletedTask;
    }

    public static float TestGetFramesPerSecond(RenderLayerContext context)
    {
        return GetFramesPerSecond(context);
    }
}

public class TestOrderedLayerRenderer : BaseLayerRenderer
{
    private readonly List<string> _callOrder;

    public TestOrderedLayerRenderer(List<string> callOrder) => _callOrder = callOrder;

    public override int Priority => 0;
    public override string Name => "TestOrderedLayerRenderer";

    protected override ValueTask OnPreRenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        _callOrder.Add("PreRender");
        return ValueTask.CompletedTask;
    }

    protected override ValueTask RenderLayerAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        _callOrder.Add("RenderLayer");
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnPostRenderAsync(RenderLayerContext context, CancellationToken ct = default)
    {
        _callOrder.Add("PostRender");
        return ValueTask.CompletedTask;
    }
}
