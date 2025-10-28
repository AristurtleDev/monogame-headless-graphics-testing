using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameHeadlessGraphicsTesting.Fixtures;

namespace MonoGameHeadlessGraphicsTesting;

/// <summary>
/// Tests that validate the GraphicsDevice functionality in a headless environment.
/// </summary>
[Collection("GraphicsTest")]
public class GraphicsDeviceTests
{
    private readonly GraphicsTestFixture _graphicsFixture;

    public GraphicsDeviceTests(GraphicsTestFixture graphicsFixture)
    {
        _graphicsFixture = graphicsFixture;
    }

    [Fact]
    public void GraphicsDevice_Properties_ShouldBeValid()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        Assert.NotNull(gd);
        Assert.NotNull(gd.Adapter);
        Assert.NotNull(gd.PresentationParameters);
        Assert.True(gd.Viewport.Width > 0);
        Assert.True(gd.Viewport.Height > 0);
    }

    [Fact]
    public void GraphicsDevice_CreateRenderTarget_ShouldWork()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        using RenderTarget2D renderTarget = new RenderTarget2D(gd, 256, 256);

        Assert.Equal(256, renderTarget.Width);
        Assert.Equal(256, renderTarget.Height);
        Assert.Equal(SurfaceFormat.Color, renderTarget.Format);
    }

    [Fact]
    public void GraphicsDevice_SetRenderTarget_ShouldNotThrow()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        using RenderTarget2D renderTarget = new RenderTarget2D(gd, 128, 128);

        // These operations should work in a headless environment
        gd.SetRenderTarget(renderTarget);
        gd.Clear(Color.Red);
        gd.SetRenderTarget(null);
    }

    [Fact]
    public void GraphicsDevice_ViewportOperations_ShouldWork()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;
        Viewport originalViewport = gd.Viewport;

        try
        {
            Viewport newViewport = new Viewport(0, 0, 400, 300);
            gd.Viewport = newViewport;

            Assert.Equal(400, gd.Viewport.Width);
            Assert.Equal(300, gd.Viewport.Height);
        }
        finally
        {
            // Restore original viewport
            gd.Viewport = originalViewport;
        }
    }

    [Fact]
    public void GraphicsDevice_BlendStates_ShouldBeSettable()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;
        BlendState[] blendStates = [BlendState.Opaque, BlendState.AlphaBlend, BlendState.Additive, BlendState.NonPremultiplied];

        foreach (BlendState blendState in blendStates)
        {
            // Should not throw
            gd.BlendState = blendState;
            Assert.Equal(blendState, gd.BlendState);
        }
    }

    [Fact]
    public void GraphicsDevice_DepthStencilStates_ShouldBeSettable()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;
        DepthStencilState[] depthStencilStates = [DepthStencilState.Default, DepthStencilState.DepthRead, DepthStencilState.None];

        foreach (DepthStencilState depthStencilState in depthStencilStates)
        {
            // Should not throw
            gd.DepthStencilState = depthStencilState;
            Assert.Equal(depthStencilState, gd.DepthStencilState);
        }
    }

    [Fact]
    public void GraphicsDevice_RasterizerStates_ShouldBeSettable()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;
        RasterizerState[] rasterizerStates = [RasterizerState.CullClockwise, RasterizerState.CullCounterClockwise, RasterizerState.CullNone];

        foreach (RasterizerState rasterizerState in rasterizerStates)
        {
            // Should not throw
            gd.RasterizerState = rasterizerState;
            Assert.Equal(rasterizerState, gd.RasterizerState);
        }
    }

    [Fact]
    public void GraphicsDevice_IndexBuffer_ShouldBeCreatable()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        using IndexBuffer indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);

        ushort[] indices = [0, 1, 2, 0, 2, 3];

        // Should not throw
        indexBuffer.SetData(indices);
    }

    [Fact]
    public void GraphicsDevice_VertexBuffer_ShouldBeCreatable()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        using VertexBuffer vertexBuffer = new VertexBuffer(gd, VertexPositionColor.VertexDeclaration, 4, BufferUsage.WriteOnly);

        VertexPositionColor[] vertices =
        [
            new VertexPositionColor(new Vector3(-1, -1, 0), Color.Red),
            new VertexPositionColor(new Vector3(1, -1, 0), Color.Green),
            new VertexPositionColor(new Vector3(1, 1, 0), Color.Blue),
            new VertexPositionColor(new Vector3(-1, 1, 0), Color.Yellow)
        ];

        // Should not throw
        vertexBuffer.SetData(vertices);
    }
}
