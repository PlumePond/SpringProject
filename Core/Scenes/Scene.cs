using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.UI;

namespace SpringProject.Core.Scenes;

public abstract class Scene
{
    public Canvas ActiveCanvas { get; set; }

    public Scene()
    {
        // initialize canvas
        Point windowSize = Main.GameWindow.ClientBounds.Size;

        Point scaledSize = new Point(
            windowSize.X / Main.Settings.UISize,
            windowSize.Y / Main.Settings.UISize
        );
        
        ActiveCanvas = new Canvas(Point.Zero, scaledSize, Vector2.One, Origin.TopLeft, Anchor.TopLeft);

        Initialize();
    }

    public virtual void Initialize()
    {
        
    }

    public virtual void Start()
    {
        UpdateCanvasSize();
    }

    public virtual void Close()
    {
        
    }

    void UpdateCanvasSize()
    {
        // initialize canvas
        Point windowSize = Main.GameWindow.ClientBounds.Size;

        Point scaledSize = new Point(
            windowSize.X / Main.Settings.UISize,
            windowSize.Y / Main.Settings.UISize
        );

        ActiveCanvas.SetSize(scaledSize);
    }

    public virtual void Update(GameTime gameTime)
    {
        if (ActiveCanvas.Active)
        {
            ActiveCanvas.Update(gameTime);
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (ActiveCanvas.Active)
        {
            ActiveCanvas.Draw(spriteBatch);
        }
    }
}