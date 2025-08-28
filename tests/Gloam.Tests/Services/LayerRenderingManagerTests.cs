using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Core.Primitives;
using Gloam.Runtime.Services;
using Moq;

namespace Gloam.Tests.Services;

public class LayerRenderingManagerTests
{
    private RenderLayerContext _context = null!;
    private LayerRenderingManager _manager = null!;
    private Mock<ILayerRenderer> _mockRenderer1 = null!;
    private Mock<ILayerRenderer> _mockRenderer2 = null!;
    private Mock<ILayerRenderer> _mockRenderer3 = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRenderer1 = new Mock<ILayerRenderer>();
        _mockRenderer2 = new Mock<ILayerRenderer>();
        _mockRenderer3 = new Mock<ILayerRenderer>();

        // Set up different priorities
        _mockRenderer1.Setup(r => r.Priority).Returns(1);
        _mockRenderer1.Setup(r => r.Name).Returns("Renderer1");
        _mockRenderer1.Setup(r => r.IsVisible).Returns(true);

        _mockRenderer2.Setup(r => r.Priority).Returns(3);
        _mockRenderer2.Setup(r => r.Name).Returns("Renderer2");
        _mockRenderer2.Setup(r => r.IsVisible).Returns(true);

        _mockRenderer3.Setup(r => r.Priority).Returns(2);
        _mockRenderer3.Setup(r => r.Name).Returns("Renderer3");
        _mockRenderer3.Setup(r => r.IsVisible).Returns(true);

        // Set up real context (records can't be mocked)
        var mockRenderer = new Mock<IRenderer>();
        var mockSurface = new Mock<IRenderSurface>();
        var mockInputDevice = new Mock<IInputDevice>();
        mockRenderer.Setup(r => r.Surface).Returns(mockSurface.Object);
        mockSurface.Setup(s => s.Width).Returns(800);
        mockSurface.Setup(s => s.Height).Returns(600);

        var frameInfo = new FrameInfo(1, TimeSpan.FromMilliseconds(16.67), TimeSpan.FromSeconds(1), 60f);
        _context = new RenderLayerContext(
            mockRenderer.Object,
            mockInputDevice.Object,
            frameInfo,
            new Size(800, 600)
        );
    }

    [Test]
    public void Constructor_WithEmptyCollection_ShouldCreateEmptyManager()
    {
        _manager = new LayerRenderingManager();

        Assert.That(_manager.LayerRenderers, Is.Not.Null);
        Assert.That(_manager.LayerRenderers, Is.Empty);
    }

    [Test]
    public void Constructor_WithSingleRenderer_ShouldStoreRenderer()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);

        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(1));
        Assert.That(_manager.LayerRenderers[0], Is.SameAs(_mockRenderer1.Object));
    }

    [Test]
    public void Constructor_WithMultipleRenderers_ShouldSortByPriority()
    {
        // Add renderers in non-priority order: 1, 3, 2
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer2.Object);
        _manager.AddLayerRenderer(_mockRenderer3.Object);

        // Should be sorted by priority: 1, 2, 3
        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(3));
        Assert.That(_manager.LayerRenderers[0].Priority, Is.EqualTo(1)); // Renderer1
        Assert.That(_manager.LayerRenderers[1].Priority, Is.EqualTo(2)); // Renderer3
        Assert.That(_manager.LayerRenderers[2].Priority, Is.EqualTo(3)); // Renderer2
    }

    [Test]
    public void Constructor_WithDuplicatePriorities_ShouldMaintainStableSort()
    {
        var mockRenderer4 = new Mock<ILayerRenderer>();
        mockRenderer4.Setup(r => r.Priority).Returns(1); // Same priority as renderer1
        mockRenderer4.Setup(r => r.Name).Returns("Renderer4");
        mockRenderer4.Setup(r => r.IsVisible).Returns(true);

        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(mockRenderer4.Object);

        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(2));
        Assert.That(_manager.LayerRenderers[0].Priority, Is.EqualTo(1));
        Assert.That(_manager.LayerRenderers[1].Priority, Is.EqualTo(1));
        // Order should be maintained for same priority (stable sort)
        Assert.That(_manager.LayerRenderers[0].Name, Is.EqualTo("Renderer1"));
        Assert.That(_manager.LayerRenderers[1].Name, Is.EqualTo("Renderer4"));
    }

    [Test]
    public void Constructor_WithNegativePriorities_ShouldSort()
    {
        _mockRenderer1.Setup(r => r.Priority).Returns(-1);
        _mockRenderer2.Setup(r => r.Priority).Returns(1);
        _mockRenderer3.Setup(r => r.Priority).Returns(0);

        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer2.Object);
        _manager.AddLayerRenderer(_mockRenderer3.Object);

        // Should be sorted: -1, 0, 1
        Assert.That(_manager.LayerRenderers[0].Priority, Is.EqualTo(-1));
        Assert.That(_manager.LayerRenderers[1].Priority, Is.EqualTo(0));
        Assert.That(_manager.LayerRenderers[2].Priority, Is.EqualTo(1));
    }

    [Test]
    public async Task RenderAllLayersAsync_WithEmptyCollection_ShouldComplete()
    {
        _manager = new LayerRenderingManager();

        // Should not throw and complete quickly
        Assert.DoesNotThrowAsync(async () =>
            await _manager.RenderAllLayersAsync(_context)
        );
    }

    [Test]
    public async Task RenderAllLayersAsync_WithSingleRenderer_ShouldCallRender()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);

        await _manager.RenderAllLayersAsync(_context);

        _mockRenderer1.Verify(r => r.RenderAsync(_context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RenderAllLayersAsync_WithMultipleRenderers_ShouldCallRenderInPriorityOrder()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer2.Object);
        _manager.AddLayerRenderer(_mockRenderer3.Object);

        var callOrder = new List<string>();

        _mockRenderer1.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask)
            .Callback(() => callOrder.Add("Renderer1"));

        _mockRenderer2.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask)
            .Callback(() => callOrder.Add("Renderer2"));

        _mockRenderer3.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask)
            .Callback(() => callOrder.Add("Renderer3"));

        await _manager.RenderAllLayersAsync(_context);

        // Should be called in priority order: 1, 2, 3 (Renderer1, Renderer3, Renderer2)
        Assert.That(callOrder, Has.Count.EqualTo(3));
        Assert.That(callOrder[0], Is.EqualTo("Renderer1"));
        Assert.That(callOrder[1], Is.EqualTo("Renderer3"));
        Assert.That(callOrder[2], Is.EqualTo("Renderer2"));
    }

    [Test]
    public async Task RenderAllLayersAsync_WithCancellationToken_ShouldPassToken()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        using var cts = new CancellationTokenSource();

        await _manager.RenderAllLayersAsync(_context, cts.Token);

        _mockRenderer1.Verify(r => r.RenderAsync(_context, cts.Token), Times.Once);
    }

    [Test]
    public async Task RenderAllLayersAsync_WithCancelledToken_ShouldRespectCancellation()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRenderer1.Setup(r => r.IsVisible).Returns(true);
        _mockRenderer1.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .Returns((RenderLayerContext ctx, CancellationToken ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    return ValueTask.CompletedTask;
                }
            );

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _manager.RenderAllLayersAsync(_context, cts.Token)
        );
    }

    [Test]
    public async Task RenderAllLayersAsync_RendererThrowsException_ShouldPropagateException()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);

        _mockRenderer1.Setup(r => r.IsVisible).Returns(true);
        _mockRenderer1.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Renderer error"));

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _manager.RenderAllLayersAsync(_context)
        );
    }

    [Test]
    public async Task RenderAllLayersAsync_FirstRendererThrows_ShouldNotCallSubsequentRenderers()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer2.Object);

        _mockRenderer1.Setup(r => r.IsVisible).Returns(true);
        _mockRenderer2.Setup(r => r.IsVisible).Returns(true);
        _mockRenderer1.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("First renderer error"));

        try
        {
            await _manager.RenderAllLayersAsync(_context);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        _mockRenderer1.Verify(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRenderer2.Verify(
            r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public void AddLayerRenderer_ToEmptyCollection_ShouldAddRenderer()
    {
        _manager = new LayerRenderingManager();

        _manager.AddLayerRenderer(_mockRenderer1.Object);

        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(1));
        Assert.That(_manager.LayerRenderers[0], Is.SameAs(_mockRenderer1.Object));
    }

    [Test]
    public void AddLayerRenderer_ToExistingCollection_ShouldAddAndResort()
    {
        _manager = new LayerRenderingManager(); // Priority 3
        _manager.AddLayerRenderer(_mockRenderer2.Object);

        _manager.AddLayerRenderer(_mockRenderer1.Object); // Priority 1

        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(2));
        Assert.That(_manager.LayerRenderers[0].Priority, Is.EqualTo(1)); // Should be first
        Assert.That(_manager.LayerRenderers[1].Priority, Is.EqualTo(3)); // Should be second
    }

    [Test]
    public void AddLayerRenderer_WithSamePriority_ShouldMaintainOrder()
    {
        var mockRenderer4 = new Mock<ILayerRenderer>();
        mockRenderer4.Setup(r => r.Priority).Returns(1);
        mockRenderer4.Setup(r => r.Name).Returns("Renderer4");
        mockRenderer4.Setup(r => r.IsVisible).Returns(true);

        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object); // Priority 1

        _manager.AddLayerRenderer(mockRenderer4.Object); // Priority 1

        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(2));
        Assert.That(_manager.LayerRenderers[0].Name, Is.EqualTo("Renderer1"));
        Assert.That(_manager.LayerRenderers[1].Name, Is.EqualTo("Renderer4"));
    }

    [Test]
    public void AddLayerRenderer_SameRendererTwice_ShouldAddBothInstances()
    {
        _manager = new LayerRenderingManager();

        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer1.Object);

        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(2));
        Assert.That(_manager.LayerRenderers[0], Is.SameAs(_mockRenderer1.Object));
        Assert.That(_manager.LayerRenderers[1], Is.SameAs(_mockRenderer1.Object));
    }

    [Test]
    public void RemoveLayerRenderer_FromEmptyCollection_ShouldReturnFalse()
    {
        _manager = new LayerRenderingManager();

        var result = _manager.RemoveLayerRenderer(_mockRenderer1.Object);

        Assert.That(result, Is.False);
        Assert.That(_manager.LayerRenderers, Is.Empty);
    }

    [Test]
    public void RemoveLayerRenderer_ExistingRenderer_ShouldReturnTrueAndRemove()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer2.Object);

        var result = _manager.RemoveLayerRenderer(_mockRenderer1.Object);

        Assert.That(result, Is.True);
        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(1));
        Assert.That(_manager.LayerRenderers[0], Is.SameAs(_mockRenderer2.Object));
    }

    [Test]
    public void RemoveLayerRenderer_NonExistentRenderer_ShouldReturnFalse()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);

        var result = _manager.RemoveLayerRenderer(_mockRenderer2.Object);

        Assert.That(result, Is.False);
        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(1));
        Assert.That(_manager.LayerRenderers[0], Is.SameAs(_mockRenderer1.Object));
    }

    [Test]
    public void RemoveLayerRenderer_DuplicateRenderers_ShouldRemoveFirstOccurrence()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer1.Object);
        _manager.AddLayerRenderer(_mockRenderer2.Object);

        var result = _manager.RemoveLayerRenderer(_mockRenderer1.Object);

        Assert.That(result, Is.True);
        Assert.That(_manager.LayerRenderers, Has.Count.EqualTo(2));
        Assert.That(_manager.LayerRenderers.Count(r => r == _mockRenderer1.Object), Is.EqualTo(1));
        Assert.That(_manager.LayerRenderers.Count(r => r == _mockRenderer2.Object), Is.EqualTo(1));
    }

    [Test]
    public void RemoveLayerRenderer_RemoveAll_ShouldCreateEmptyCollection()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);

        var result = _manager.RemoveLayerRenderer(_mockRenderer1.Object);

        Assert.That(result, Is.True);
        Assert.That(_manager.LayerRenderers, Is.Empty);
    }

    [Test]
    public void LayerRenderers_ShouldBeReadOnly()
    {
        _manager = new LayerRenderingManager();
        _manager.AddLayerRenderer(_mockRenderer1.Object);

        // Verify that we can't modify the collection directly
        Assert.That(_manager.LayerRenderers, Is.InstanceOf<IReadOnlyList<ILayerRenderer>>());

        // This should not affect the internal collection
        var readOnlyList = _manager.LayerRenderers;

        // We can't cast to a mutable list and modify it
        Assert.That(readOnlyList, Is.Not.InstanceOf<List<ILayerRenderer>>());
    }

    [Test]
    public async Task Integration_AddRemoveRender_ShouldWorkCorrectly()
    {
        _manager = new LayerRenderingManager();

        // Add renderers
        _manager.AddLayerRenderer(_mockRenderer2.Object); // Priority 3
        _manager.AddLayerRenderer(_mockRenderer1.Object); // Priority 1

        var callOrder = new List<string>();

        _mockRenderer1.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask)
            .Callback(() => callOrder.Add("Renderer1"));

        _mockRenderer2.Setup(r => r.RenderAsync(It.IsAny<RenderLayerContext>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask)
            .Callback(() => callOrder.Add("Renderer2"));

        // Render all
        await _manager.RenderAllLayersAsync(_context);

        Assert.That(callOrder, Has.Count.EqualTo(2));
        Assert.That(callOrder[0], Is.EqualTo("Renderer1")); // Priority 1 first
        Assert.That(callOrder[1], Is.EqualTo("Renderer2")); // Priority 3 second

        // Remove one renderer
        _manager.RemoveLayerRenderer(_mockRenderer1.Object);
        callOrder.Clear();

        // Render again
        await _manager.RenderAllLayersAsync(_context);

        Assert.That(callOrder, Has.Count.EqualTo(1));
        Assert.That(callOrder[0], Is.EqualTo("Renderer2"));
    }
}
