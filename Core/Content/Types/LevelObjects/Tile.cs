using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Cont.Types.LevelObjects;

public class Tile : LevelObject
{
    int frameIndex = 0;
    Point framePos = Point.Zero;
    int _tileSize = 16;

    public override void OnPlaced()
    {
        // calculate frame index for all neighbors
        foreach (var neighbor in GetNeighbors())
        {
            neighbor.CalculateFrameIndex();
        }

        CalculateFrameIndex();
    }

    public override void SetPosition(Point position)
    {
        var neighbors = GetNeighbors();

        base.SetPosition(position);

        // calculate frame index for all neighbors
        foreach (var neighbor in neighbors)
        {
            neighbor.CalculateFrameIndex();
        }

        CalculateFrameIndex();
    }

    public override void OnRemoved()
    {
        // calculate frame index for all neighbors
        foreach (var neighbor in GetNeighbors())
        {
            neighbor.CalculateFrameIndex();
        }
    }

    public void CalculateFrameIndex()
    {
        bool hasTopLeft = HasNeighbor(new Point(-_tileSize, -_tileSize));
        bool hasTop = HasNeighbor(new Point(0, -_tileSize));
        bool hasTopRight = HasNeighbor(new Point(_tileSize, -_tileSize));
        bool hasRight = HasNeighbor(new Point(_tileSize, 0));
        bool hasBottomRight = HasNeighbor(new Point(_tileSize, _tileSize));
        bool hasBottom = HasNeighbor(new Point(0, _tileSize));
        bool hasBottomLeft = HasNeighbor(new Point(-_tileSize, _tileSize));
        bool hasLeft = HasNeighbor(new Point(-_tileSize, 0));

        if (!hasLeft && hasRight && !hasTop && hasBottom)
        {
            if (hasBottomRight)
            {
                SetFrame(0);
            }
            else
            {
                SetFrame(32);
            }
        }
        else if (hasLeft && hasRight && !hasTop && hasBottom)
        {
            if (hasBottomLeft && hasBottomRight)
            {
                SetFrame(1);
            }
            else if (hasBottomLeft && !hasBottomRight)
            {
                SetFrame(30);
            }
            else if (!hasBottomLeft && hasBottomRight)
            {
                SetFrame(31);
            }
            else if (!hasBottomLeft && !hasBottomRight)
            {
                SetFrame(33);
            }
        }
        else if (hasLeft && !hasRight && !hasTop && hasBottom)
        {
            if (hasBottomLeft)
            {
                SetFrame(2);
            }
            else
            {
                SetFrame(34);
            }
        }
        else if (!hasLeft && !hasRight && !hasTop && hasBottom)
        {
            SetFrame(3);
        }
        else if (!hasLeft && hasRight && hasTop && hasBottom)
        {
            if (hasTopRight && hasBottomRight)
            {
                SetFrame(7);
            }
            else if (!hasTopRight && !hasBottomRight)
            {
                SetFrame(39);
            }
            else if (!hasTopRight && hasBottomRight)
            {
                SetFrame(35);
            }
            else if (hasTopRight && !hasBottomRight)
            {
                SetFrame(28);
            }
        }
        else if (hasLeft && hasRight && hasTop && hasBottom)
        {
            if (hasTopLeft && hasTopRight && hasBottomLeft && hasBottomRight)
            {
                SetFrame(8);
            }
            else if (!hasTopLeft && hasTopRight && hasBottomLeft && !hasBottomRight)
            {
                SetFrame(45);
            }
            else if (hasTopLeft && !hasTopRight && !hasBottomLeft && hasBottomRight)
            {
                SetFrame(44);
            }
            else if (hasTopLeft && hasTopRight && hasBottomLeft && !hasBottomRight)
            {
                SetFrame(4);
            }
            else if (hasTopLeft && hasTopRight && !hasBottomLeft && hasBottomRight)
            {
                SetFrame(5);
            }
            else if (hasTopLeft && !hasTopRight && hasBottomLeft && hasBottomRight)
            {
                SetFrame(11);
            }
            else if (!hasTopLeft && hasTopRight && hasBottomLeft && hasBottomRight)
            {
                SetFrame(12);
            }
            else if (!hasTopLeft && !hasTopRight && hasBottomLeft && hasBottomRight)
            {
                SetFrame(6);
            }
            else if (!hasTopLeft && hasTopRight && !hasBottomLeft && hasBottomRight)
            {
                SetFrame(13);
            }
            else if (hasTopLeft && !hasTopRight && hasBottomLeft && !hasBottomRight)
            {
                SetFrame(20);
            }
            else if (hasTopLeft && hasTopRight && !hasBottomLeft && !hasBottomRight)
            {
                SetFrame(27);
            }

            else if (!hasTopLeft && !hasTopRight && hasBottomLeft && !hasBottomRight)
            {
                SetFrame(18);
            }
            else if (!hasTopLeft && hasTopRight && !hasBottomLeft && !hasBottomRight)
            {
                SetFrame(19);
            }
            else if (hasTopLeft && !hasTopRight && !hasBottomLeft && !hasBottomRight)
            {
                SetFrame(25);
            }
            else if (!hasTopLeft && !hasTopRight && !hasBottomLeft && hasBottomRight)
            {
                SetFrame(26);
            }
            else if (!hasTopLeft && !hasTopRight && !hasBottomLeft && !hasBottomRight)
            {
                SetFrame(40);
            }
        }
        else if (hasLeft && !hasRight && hasTop && hasBottom)
        {
            if (hasTopLeft && hasBottomLeft)
            {
                SetFrame(9);
            }
            else if (!hasTopLeft && !hasBottomLeft)
            {
                SetFrame(41);
            }
            else if (!hasTopLeft && hasBottomLeft)
            {
                SetFrame(36);
            }
            else if (hasTopLeft && !hasBottomLeft)
            {
                SetFrame(29);
            }
        }
        else if (!hasLeft && !hasRight && hasTop && hasBottom)
        {
            SetFrame(10);
        }
        else if (!hasLeft && hasRight && hasTop && !hasBottom)
        {
            if (hasTopRight)
            {
                SetFrame(14);
            }
            else
            {
                SetFrame(46);
            }
        }
        else if (hasLeft && hasRight && hasTop && !hasBottom)
        {
            if (hasTopLeft && hasTopRight)
            {
                SetFrame(15);
            }
            else if (hasTopLeft && !hasTopRight)
            {
                SetFrame(37);
            }
            else if (!hasTopLeft && hasTopRight)
            {
                SetFrame(38);
            }
            else if (!hasTopLeft && !hasTopRight)
            {
                SetFrame(47);
            }
        }
        else if (hasLeft && !hasRight && hasTop && !hasBottom)
        {
            if (hasTopLeft)
            {
                SetFrame(16);
            }
            else
            {
                SetFrame(48);
            }
        }
        else if (!hasLeft && !hasRight && hasTop && !hasBottom)
        {
            SetFrame(17);
        }
        else if (!hasLeft && hasRight && !hasTop && !hasBottom)
        {
            SetFrame(21);
        }
        else if (hasLeft && hasRight && !hasTop && !hasBottom)
        {
            SetFrame(22);
        }
        else if (hasLeft && !hasRight && !hasTop && !hasBottom)
        {
            SetFrame(23);
        }
        else if (!hasLeft && !hasRight && !hasTop && !hasBottom)
        {
            SetFrame(24);
        }
    }

    void SetFrame(int index)
    {
        int columns = data.sprite.Width / _tileSize;

        int x = index % columns;
        int y = index / columns;

        framePos = new Point(x * _tileSize, y * _tileSize);
    }

    bool HasNeighbor(Point offset)
    {
        Point pos = transform.position + offset;

        foreach (var levelObject in grid.layers[layer].LevelObjects)
        {
            // if the level object is a tile
            if (levelObject is Tile tile && tile.transform.position == pos && levelObject != this)
            {
                // if the tile shares a tag
                foreach (var tag in data.tags)
                {
                    if (tile.data.tags.Contains(tag))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    List<Tile> GetNeighbors()
    {
        var neighbors = new List<Tile>();

        Point[] offsets = {
            new Point(-_tileSize, -_tileSize),
            new Point(0, -_tileSize),
            new Point(_tileSize, -_tileSize),
            new Point(_tileSize, 0),
            new Point(_tileSize, _tileSize),
            new Point(0, _tileSize),
            new Point(-_tileSize, _tileSize),
            new Point(-_tileSize, 0)
        };

        foreach (var offset in offsets)
        {
            Point pos = transform.position + offset;

            foreach (var levelObject in grid.layers[layer].LevelObjects)
            {
                // if the level object is a tile
                if (levelObject is Tile tile && tile.transform.position == pos && levelObject != this)
                {
                    // if the tile shares a tag
                    foreach (var tag in data.tags)
                    {
                        if (tile.data.tags.Contains(tag))
                        {
                            neighbors.Add(tile);
                            break; // ensure you don't add the tile again if it shares multiple tags
                        }
                    }
                }
            }
        }

        return neighbors;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Point framedSize = data.frame != Point.Zero ? data.frame : data.size;
        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(framedSize.X / 2f, framedSize.Y / 2f);
        float radians = transform.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (transform.flipY) effects |= SpriteEffects.FlipVertically;

        Vector2 drawScale = new Vector2((float)size.X / data.sprite.Width, (float)size.Y / data.sprite.Height);

        Color objectColor = selected ? Color.LightGoldenrodYellow * color : color;
        
        Rectangle? sourceRect = frame != Point.Zero ? new Rectangle(framePos, frame) : null;

        spriteBatch.Draw(data.sprite, drawPos, sourceRect, objectColor * tint, radians, origin, drawScale, effects, 0f);
    }

    public override void DrawOutline(SpriteBatch spriteBatch)
    {
        // no need to draw this if it is not selected or hovered boii
        if (!hovered && !selected) return;

        Point framedSize = data.frame != Point.Zero ? data.frame : data.size;
        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(framedSize.X / 2f, framedSize.Y / 2f);
        float radians = transform.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (transform.flipY) effects |= SpriteEffects.FlipVertically;

        Vector2 drawScale = new Vector2((float)size.X / data.sprite.Width, (float)size.Y / data.sprite.Height);

        Color objectColor = selected ? Color.LightGoldenrodYellow * color : color;
        
        Rectangle? sourceRect = frame != Point.Zero ? new Rectangle(framePos, frame) : null;

        Color outlineColor = selected ? Color.Yellow : Color.White;
        TextureUtils.DrawOutlineExpanded(spriteBatch, data.alphaTexture, drawPos, sourceRect, outlineColor, radians, origin, drawScale, effects, 0);
    }
}