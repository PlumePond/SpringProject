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
            SetFrame(0);
        }
        else if (hasLeft && hasRight && !hasTop && hasBottom)
        {
            SetFrame(1);
        }
        else if (hasLeft && !hasRight && !hasTop && hasBottom)
        {
            SetFrame(2);
        }
        else if (!hasLeft && !hasRight && !hasTop && hasBottom)
        {
            SetFrame(3);
        }
        else if (!hasLeft && hasRight && hasTop && hasBottom)
        {
            SetFrame(7);
        }
        else if (hasLeft && hasRight && hasTop && hasBottom)
        {
            SetFrame(8);
        }
        else if (hasLeft && !hasRight && hasTop && hasBottom)
        {
            SetFrame(9);
        }
        else if (!hasLeft && !hasRight && hasTop && hasBottom)
        {
            SetFrame(10);
        }
        else if (!hasLeft && hasRight && hasTop && !hasBottom)
        {
            SetFrame(14);
        }
        else if (hasLeft && hasRight && hasTop && !hasBottom)
        {
            SetFrame(15);
        }
        else if (hasLeft && !hasRight && hasTop && !hasBottom)
        {
            SetFrame(16);
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

        if (hovered)
        {
            spriteBatch.Draw(data.outline, drawPos, sourceRect, Color.White, radians, origin, drawScale, effects, 0f);
        }
        else if (selected)
        {
            spriteBatch.Draw(data.outline, drawPos, sourceRect, Color.Yellow, radians, origin, drawScale, effects, 0f);
        }
    }
}