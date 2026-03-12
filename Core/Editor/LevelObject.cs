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
    public bool hovering { get; private set; } = false;
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
    }

    public void SetHovering(bool hovering)
    {
        this.hovering = hovering;
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
    }

    public void RotateCounterClockwise()
    {
        rotation = (rotation + 270) % 360;
    }
    
    public void SetRotation(int rotation)
    {
        this.rotation = rotation % 360;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }

    public void CalculateBounds(int snapSize = 16)
    {
        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions
            ? new Point(data.size.Y, data.size.X)
            : new Point(data.size.X, data.size.Y);

        Point center = new Point(position.X + data.size.X / 2, position.Y + data.size.Y / 2);
        
        int left = center.X - rotatedSize.X / 2;
        int top = center.Y - rotatedSize.Y / 2;
        
        // Snap top-left back to grid
        left = (left / snapSize) * snapSize;
        top = (top / snapSize) * snapSize;

        bounds = new Rectangle(left, top, rotatedSize.X, rotatedSize.Y);
    }
}