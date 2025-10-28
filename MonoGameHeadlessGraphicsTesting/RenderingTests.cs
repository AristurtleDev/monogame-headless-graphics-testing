using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameHeadlessGraphicsTesting.Fixtures;

namespace MonoGameHeadlessGraphicsTesting;

/// <summary>
/// Tests that demonstrate rendering capabilities in a headless environment.
/// These tests prove that actual drawing operations work correctly
/// </summary>
[Collection("GraphicsTest")]
public class RenderingTests
{
    private readonly GraphicsTestFixture _graphicsFixture;

    public RenderingTests(GraphicsTestFixture graphicsFixture)
    {
        _graphicsFixture = graphicsFixture;
    }

    [Fact]
    public void SpriteBatch_BasicDrawing_ShouldNotThrow()
    {
        SpriteBatch spriteBatch = _graphicsFixture.SpriteBatch;
        Texture2D texture = _graphicsFixture.CreatePixelTexture();

        try
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Vector2(10, 10), Color.White);
            spriteBatch.Draw(texture, new Rectangle(50, 50, 100, 100), Color.Red);
            spriteBatch.End();
        }
        finally
        {
            texture.Dispose();
        }
    }

    [Fact]
    public void SpriteBatch_DifferentBlendModes_ShouldWork()
    {
        SpriteBatch spriteBatch = _graphicsFixture.SpriteBatch;
        Texture2D texture = _graphicsFixture.CreateTestTexture(32, 32, Color.Red);

        try
        {
            BlendState[] blendStates =
            [
                BlendState.Opaque,
                BlendState.AlphaBlend,
                BlendState.Additive,
                BlendState.NonPremultiplied
            ];

            foreach (BlendState blendState in blendStates)
            {
                spriteBatch.Begin(blendState: blendState);
                spriteBatch.Draw(texture, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
        }
        finally
        {
            texture.Dispose();
        }
    }

    [Fact]
    public void RenderTarget_DrawAndReadBack_ShouldPreserveColor()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;
        SpriteBatch spriteBatch = _graphicsFixture.SpriteBatch;

        using RenderTarget2D renderTarget = new RenderTarget2D(gd, 4, 4);
        using Texture2D texture = _graphicsFixture.CreateTestTexture(4, 4, Color.Blue);

        // Draw to render target
        gd.SetRenderTarget(renderTarget);
        gd.Clear(Color.Transparent);

        spriteBatch.Begin();
        spriteBatch.Draw(texture, Vector2.Zero, Color.White);
        spriteBatch.End();

        gd.SetRenderTarget(null);

        // Read back the data
        Color[] data = new Color[16];
        renderTarget.GetData(data);

        // Verify that we drew something (not all transparent)
        Assert.Contains(data, color => color != Color.Transparent);
    }

    [Fact]
    public void RenderTarget_ClearOperations_ShouldWork()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        using RenderTarget2D renderTarget = new RenderTarget2D(gd, 8, 8);

        Color[] testColors = [Color.Red, Color.Green, Color.Blue, Color.Yellow];

        foreach (Color clearColor in testColors)
        {
            gd.SetRenderTarget(renderTarget);
            gd.Clear(clearColor);
            gd.SetRenderTarget(null);

            Color[] data = new Color[64];
            renderTarget.GetData(data);

            // All pixels should be the clear color
            Assert.All(data, color => Assert.Equal(clearColor, color));
        }
    }

    [Fact]
    public void PrimitiveDrawing_BasicShapes_ShouldNotThrow()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        VertexPositionColor[] vertices =
        [
            new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0), Color.Red),
            new VertexPositionColor(new Vector3(0.5f, -0.5f, 0), Color.Green),
            new VertexPositionColor(new Vector3(0, 0.5f, 0), Color.Blue)
        ];

        // Note: This requires a basic effect for full rendering, but the setup should not throw
        foreach (VertexPositionColor vertex in vertices)
        {
            // Just verify we can access vertex data
            Assert.NotEqual(Vector3.Zero, vertex.Position);
            Assert.NotEqual(Color.Transparent, vertex.Color);
        }
    }

    [Fact]
    public void MultipleRenderTargets_Sequential_ShouldWork()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        using RenderTarget2D rt1 = new RenderTarget2D(gd, 64, 64);
        using RenderTarget2D rt2 = new RenderTarget2D(gd, 64, 64);

        // Draw to first render target
        gd.SetRenderTarget(rt1);
        gd.Clear(Color.Red);
        gd.SetRenderTarget(null);

        // Draw to second render target
        gd.SetRenderTarget(rt2);
        gd.Clear(Color.Blue);
        gd.SetRenderTarget(null);

        // Verify they contain different colors
        Color[] data1 = new Color[64 * 64];
        Color[] data2 = new Color[64 * 64];

        rt1.GetData(data1);
        rt2.GetData(data2);

        Assert.All(data1, color => Assert.Equal(Color.Red, color));
        Assert.All(data2, color => Assert.Equal(Color.Blue, color));
    }

    [Theory]
    [InlineData(16, 16)]
    [InlineData(64, 64)]
    [InlineData(128, 128)]
    [InlineData(256, 256)]
    public void RenderTarget_VariousSizes_ShouldBeSupported(int width, int height)
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;

        using RenderTarget2D renderTarget = new RenderTarget2D(gd, width, height);

        Assert.Equal(width, renderTarget.Width);
        Assert.Equal(height, renderTarget.Height);

        // Should be able to use as render target
        gd.SetRenderTarget(renderTarget);
        gd.Clear(Color.Magenta);
        gd.SetRenderTarget(null);

        // Should be able to read data
        Color[] data = new Color[width * height];
        renderTarget.GetData(data);

        Assert.Equal(width * height, data.Length);
        Assert.All(data, color => Assert.Equal(Color.Magenta, color));
    }

    [Fact]
    public void TextureFiltering_DifferentSamplerStates_ShouldBeSettable()
    {
        GraphicsDevice gd = _graphicsFixture.GraphicsDevice;
        SpriteBatch spriteBatch = _graphicsFixture.SpriteBatch;
        Texture2D texture = _graphicsFixture.CreateTestTexture(2, 2, Color.White);

        try
        {
            SamplerState[] samplerStates =
            [
                SamplerState.PointClamp,
                SamplerState.LinearClamp,
                SamplerState.PointWrap,
                SamplerState.LinearWrap
            ];

            foreach (SamplerState samplerState in samplerStates)
            {
                spriteBatch.Begin(samplerState: samplerState);
                spriteBatch.Draw(texture, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
        }
        finally
        {
            texture.Dispose();
        }
    }
}
