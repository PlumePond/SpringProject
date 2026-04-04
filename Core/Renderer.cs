using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpringProject.Core
{
    public class Renderer
    {
        private readonly Game _game;
        private RenderTarget2D _renderTarget;
        private Viewport _viewport;
        private float _ratioX;
        private float _ratioY;
        private Vector2 _virtualMousePosition = new Vector2();
        public Color BackgroundColor = Color.DarkOliveGreen;

        public Renderer(Game game)
        {
            _game = game;
            // Set to your desired low resolution
            VirtualWidth = 640;
            VirtualHeight = 360;
            ScreenWidth = 1920;
            ScreenHeight = 1080;
        }

        public int VirtualHeight;
        public int VirtualWidth;
        public int ScreenWidth;
        public int ScreenHeight;

        public void Initialize()
        {
            // Create the render target at the virtual resolution
            _renderTarget = new RenderTarget2D(_game.GraphicsDevice, VirtualWidth, VirtualHeight);

            SetupVirtualScreenViewport();
            _ratioX = (float)_viewport.Width / VirtualWidth;
            _ratioY = (float)_viewport.Height / VirtualHeight;
            _dirtyMatrix = true;
        }

        public void SetupFullViewport()
        {
            var vp = new Viewport();
            vp.X = vp.Y = 0;
            vp.Width = ScreenWidth;
            vp.Height = ScreenHeight;
            _game.GraphicsDevice.Viewport = vp;
            _dirtyMatrix = true;
        }

        public void BeginDraw()
        {
            // First, render to the low-res render target
            _game.GraphicsDevice.SetRenderTarget(_renderTarget);
            _game.GraphicsDevice.Clear(BackgroundColor);

            // Set viewport to the full render target size
            _game.GraphicsDevice.Viewport = new Viewport(0, 0, VirtualWidth, VirtualHeight);
        }

        public void EndDraw()
        {
            // Now render the low-res texture to the screen, scaled up
            _game.GraphicsDevice.SetRenderTarget(null);
            SetupFullViewport();
            _game.GraphicsDevice.Clear(Color.Black); // Black bars for letterboxing

            // Calculate proper viewport for scaling
            SetupVirtualScreenViewport();

            // Draw the render target scaled up to the screen
            var spriteBatch = new SpriteBatch(_game.GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            spriteBatch.Draw(_renderTarget, _viewport.Bounds, Color.White);
            spriteBatch.End();
        }

        public bool RenderingToScreenIsFinished;
        private static Matrix _scaleMatrix;
        private bool _dirtyMatrix = true;

        public Matrix GetTransformationMatrix()
        {
            if (_dirtyMatrix)
                RecreateScaleMatrix();
            return _scaleMatrix;
        }

        private void RecreateScaleMatrix()
        {
            // For rendering to the render target, we don't need any scaling
            // The camera will handle the world-to-screen transformation
            Matrix.CreateScale(1f, 1f, 1f, out _scaleMatrix);
            _dirtyMatrix = false;
        }

        public Vector2 ScaleMouseToScreenCoordinates(Vector2 screenPosition)
        {
            var realX = screenPosition.X - _viewport.X;
            var realY = screenPosition.Y - _viewport.Y;
            _virtualMousePosition.X = realX / _ratioX;
            _virtualMousePosition.Y = realY / _ratioY;
            return _virtualMousePosition;
        }

        public void SetupVirtualScreenViewport()
        {
            var targetAspectRatio = VirtualWidth / (float)VirtualHeight;

            // Figure out the largest area that fits in this resolution at the desired aspect ratio
            var width = ScreenWidth;
            var height = (int)(width / targetAspectRatio + 0.5f);

            if (height > ScreenHeight)
            {
                height = ScreenHeight;
                // Pillarbox
                width = (int)(height * targetAspectRatio + 0.5f);
            }

            // For pixel perfect rendering, ensure width and height are multiples of virtual resolution
            // This gives you integer scaling when possible
            int scaleX = width / VirtualWidth;
            int scaleY = height / VirtualHeight;
            int scale = System.Math.Min(scaleX, scaleY);

            if (scale > 0)
            {
                width = VirtualWidth * scale;
                height = VirtualHeight * scale;
            }

            // Set up the new viewport centered in the backbuffer
            _viewport = new Viewport
            {
                X = (ScreenWidth / 2) - (width / 2),
                Y = (ScreenHeight / 2) - (height / 2),
                Width = width,
                Height = height
            };
        }

        public void Dispose()
        {
            _renderTarget?.Dispose();
        }
    }
}