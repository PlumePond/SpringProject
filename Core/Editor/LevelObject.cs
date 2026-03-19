using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringProject.Core.Editor;

public class LevelObject
{
    public LevelObjectData data { get; private set; }
    public Point position { get; private set; }
    public int rotation { get; set; } = 0;
    public bool flipX { get; set; } = false;
    public bool flipY { get; set; } = false;
    public Color color { get; set; } = Color.White;
    public Rectangle bounds { get; private set; }

    public LevelObject(LevelObjectData data, Point position)
    {
        this.data = data;
        this.position = position;
    }

    public void SetPosition(Point position)
    {
        this.position = position;
        CalculateBounds();
    }

    public void SetFlipX(bool flipX)
    {
        this.flipX = flipX;
    }

    public void SetFlipY(bool flipY)
    {
        this.flipY = flipY;
    }

    public void RotateClockwise()
    {
        rotation = (rotation + 90) % 360;
        CalculateBounds();
    }

    public void RotateCounterClockwise()
    {
        rotation = (rotation + 270) % 360;
        CalculateBounds();
    }
    
    public void SetRotation(int rotation)
    {
        this.rotation = rotation % 360;
        CalculateBounds();
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public void CalculateBounds()
    {
        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(data.size.Y, data.size.X) : new Point(data.size.X, data.size.Y);
        bounds = new Rectangle(position.X, position.Y, rotatedSize.X, rotatedSize.Y);
    }
}