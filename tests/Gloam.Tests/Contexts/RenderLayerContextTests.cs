using System.Diagnostics;
using Gloam.Core.Contexts;
using Gloam.Core.Interfaces;
using Gloam.Core.Input;
using Gloam.Core.Primitives;
using Moq;

namespace Gloam.Tests.Contexts;

public class RenderLayerContextTests
{
    private Mock<IRenderer> _mockRenderer = null!;
    private Mock<IRenderSurface> _mockSurface = null!;
    private Mock<IInputDevice> _mockInputDevice = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRenderer = new Mock<IRenderer>();
        _mockSurface = new Mock<IRenderSurface>();
        _mockInputDevice = new Mock<IInputDevice>();
        
        _mockRenderer.Setup(r => r.Surface).Returns(_mockSurface.Object);
        _mockSurface.Setup(s => s.Width).Returns(800);
        _mockSurface.Setup(s => s.Height).Returns(600);
        _mockInputDevice.Setup(i => i.Mouse).Returns(new MouseState());
    }

    [Test]
    public void Create_WithValidParameters_ShouldReturnRenderLayerContext()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context, Is.Not.Null);
        Assert.That(context.Renderer, Is.SameAs(_mockRenderer.Object));
        Assert.That(context.InputDevice, Is.SameAs(_mockInputDevice.Object));
        Assert.That(context.Screen.Width, Is.EqualTo(800));
        Assert.That(context.Screen.Height, Is.EqualTo(600));
    }

    [Test]
    public void Create_ShouldSetScreenDimensionsFromRendererSurface()
    {
        _mockSurface.Setup(s => s.Width).Returns(1920);
        _mockSurface.Setup(s => s.Height).Returns(1080);
        
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context.Screen.Width, Is.EqualTo(1920));
        Assert.That(context.Screen.Height, Is.EqualTo(1080));
    }

    [Test]
    public void Create_ShouldCreateFrameInfoWithCorrectTiming()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context.FrameInfo, Is.Not.Null);
        Assert.That(context.FrameInfo.DeltaTime, Is.EqualTo(timeSinceLastRender));
        Assert.That(context.FrameInfo.FramesPerSecond, Is.GreaterThan(0));
    }

    [Test]
    public void Create_WithFirstFrame_ShouldCreateFrameInfoWithZeroFrameNumber()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = true;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context.FrameInfo.FrameNumber, Is.EqualTo(0));
        Assert.That(context.FrameNumber, Is.EqualTo(0)); // Convenience property
    }

    [Test]
    public void Create_WithFirstFrame_ShouldSetFramesPerSecondToZero()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = true;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context.FrameInfo.FramesPerSecond, Is.EqualTo(0f));
    }

    [Test]
    public void Create_ConvenienceProperties_ShouldMatchFrameInfo()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context.FrameNumber, Is.EqualTo(context.FrameInfo.FrameNumber));
        Assert.That(context.DeltaTime, Is.EqualTo(context.FrameInfo.DeltaTime));
        Assert.That(context.TotalTime, Is.EqualTo(context.FrameInfo.TotalTime));
    }

    [Test]
    public void Create_ShouldAccessRendererSurfaceToDetermineScreenSize()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        // Verify that the renderer's surface was accessed
        _mockRenderer.Verify(r => r.Surface, Times.AtLeastOnce);
        _mockSurface.Verify(s => s.Width, Times.Once);
        _mockSurface.Verify(s => s.Height, Times.Once);
    }

    [Test]
    public void Create_WithNullRenderer_ShouldThrowNullReferenceException()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        Assert.Throws<NullReferenceException>(() => 
            RenderLayerContext.Create(
                null!,
                _mockInputDevice.Object,
                startTimestamp,
                currentTimestamp,
                timeSinceLastRender,
                isFirstFrame,
                renderStep));
    }

    [Test]
    public void Create_WithNullInputDevice_ShouldCreateValidContext()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            null!,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context, Is.Not.Null);
        Assert.That(context.InputDevice, Is.Null);
    }

    [Test]
    public void Create_WithZeroTimestamp_ShouldCreateValidContext()
    {
        var currentTimestamp = 0L;
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context, Is.Not.Null);
        Assert.That(context.FrameInfo.TotalTime, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void Create_WithZeroRenderStep_ShouldCreateValidContext()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.Zero;

        var startTimestamp = 0L;
        Assert.DoesNotThrow(() => 
        {
            var context = RenderLayerContext.Create(
                _mockRenderer.Object,
                _mockInputDevice.Object,
                startTimestamp,
                currentTimestamp,
                timeSinceLastRender,
                isFirstFrame,
                renderStep);

            Assert.That(context, Is.Not.Null);
        });
    }

    [Test]
    public void Create_WithNegativeTimeSinceLastRender_ShouldCreateValidContext()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(-10);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context, Is.Not.Null);
        Assert.That(context.FrameInfo.DeltaTime, Is.EqualTo(timeSinceLastRender));
    }

    [Test]
    public void Create_WithDifferentSurfaceSizes_ShouldSetCorrectScreenDimensions()
    {
        // Test with very small surface
        _mockSurface.Setup(s => s.Width).Returns(1);
        _mockSurface.Setup(s => s.Height).Returns(1);
        
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context.Screen.Width, Is.EqualTo(1));
        Assert.That(context.Screen.Height, Is.EqualTo(1));
    }

    [Test]
    public void Create_WithLargeSurfaceSizes_ShouldSetCorrectScreenDimensions()
    {
        // Test with very large surface
        _mockSurface.Setup(s => s.Width).Returns(int.MaxValue);
        _mockSurface.Setup(s => s.Height).Returns(int.MaxValue);
        
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        Assert.That(context.Screen.Width, Is.EqualTo(int.MaxValue));
        Assert.That(context.Screen.Height, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void Create_MultipleCalls_ShouldCreateIndependentContexts()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var timeSinceLastRender = TimeSpan.FromMilliseconds(16.67);
        var isFirstFrame = false;
        var renderStep = TimeSpan.FromMilliseconds(16.67);

        var startTimestamp = 0L;
        var context1 = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        var context2 = RenderLayerContext.Create(
            _mockRenderer.Object,
            _mockInputDevice.Object,
            startTimestamp,
            currentTimestamp,
            timeSinceLastRender,
            isFirstFrame,
            renderStep);

        // Contexts should be separate instances
        Assert.That(context1, Is.Not.SameAs(context2));
        
        // But should have the same values
        Assert.That(context1.Screen.Width, Is.EqualTo(context2.Screen.Width));
        Assert.That(context1.Screen.Height, Is.EqualTo(context2.Screen.Height));
        Assert.That(context1.FrameInfo.DeltaTime, Is.EqualTo(context2.FrameInfo.DeltaTime));
    }
}