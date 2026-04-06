using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Audio;
using SpringProject.Core.Commands;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Editor;

public class GridPlacement
{
    public LevelObjectData SelectedObjectData => _selectedObjectData;

    SnapSize _snapSize = SnapSize.Whole;

    ResizeHandle _activeHandle = ResizeHandle.None;
    Point _mouseDragStart;
    Rectangle _dragStartBounds;

    LevelObjectData _selectedObjectData = null;
    public LevelObject selectedObject { get; private set; } = null;
    public LevelObject hoveredObject { get; private set; } = null;
    
    bool _canPlaceObject = true;
    bool _swipe = false;
    int _rotation = 0;
    bool _flipX = false;
    bool _flipY = false;

    int resizeDistance = 4;

    bool _objectHoverConsumed = false;

    Color _selectedColor = Color.White;

    AudioComposite _placeSound => AudioManager.Get("place");
    AudioComposite _removeSound => AudioManager.Get("remove");
    AudioComposite _invalidSound => AudioManager.Get("invalid");

    Texture2D _resizeHandleTexture = null;
    Texture2D _resizeHandleSelectedTexture = null;

    Grid _grid;

    public GridPlacement(Grid grid)
    {
        _grid = grid;

        _resizeHandleTexture = TextureManager.Get("resize_handle");
        _resizeHandleSelectedTexture = TextureManager.Get("resize_handle_selected");
    }

    public void SelectColor(Color color)
    {
        _selectedColor = color;
    }

    public void Update(GameTime gameTime)
    {
        // used to prevent placing an object and selecting it in the same frame
        bool justPlaced = false;

        _swipe = Input.Get("swipe").Holding;

        if (Input.Get("rotate_ccw").Pressed)
        {
            _rotation = (_rotation + 270) % 360;
            if (selectedObject != null && !selectedObject.data.scalable)
            {
                selectedObject.RotateCounterClockwise();
            }
        }
        else if (Input.Get("rotate_cw").Pressed)
        {
            _rotation = (_rotation + 90) % 360;
            if (selectedObject != null && !selectedObject.data.scalable)
            {
                selectedObject.RotateClockwise();
            }
        }

        if (Input.Get("flip_x").Pressed)
        {
            _flipX = !_flipX;
            if (selectedObject != null)
            {
                selectedObject.SetFlipX(_flipX);
            }
        }

        if (Input.Get("flip_y").Pressed)
        {
            _flipY = !_flipY;
            if (selectedObject != null)
            {
                selectedObject.SetFlipY(_flipY);
            }
        }

        // check for snap mode input
        if (Input.Get("snap_half").Holding)
        {
            _snapSize = SnapSize.Half;
        }
        else if (Input.Get("snap_pixel").Holding)
        {
            _snapSize = SnapSize.Pixel;
        }
        else
        {
            _snapSize = SnapSize.Whole;
        }

        if (Input.Get("layer_up").Pressed)
        {
            _grid.SetActiveLayer(_grid.activeLayer + 1);
            if (_grid.activeLayer > _grid.layers.Length - 1)
            {
                _grid.SetActiveLayer(_grid.layers.Length - 1);
            }

            Deselect();
            Dehover();

            _canPlaceObject = true;
        }   
        if (Input.Get("layer_down").Pressed)
        {
            _grid.SetActiveLayer(_grid.activeLayer - 1);
            if (_grid.activeLayer < 0)
            {
                _grid.SetActiveLayer(0);
            }
            
            Deselect();
            Dehover();

            _canPlaceObject = true;
        }

        float parallaxFactor = _grid.showParallax ? _grid.layers[_grid.activeLayer].ParallaxFactor : 0.0f;
        Point mousePos = Camera.Instance.ScreenToWorld(Input.Get("cursor").Vector, parallaxFactor).ToPoint();

        // check for object placement
        // do not place if there is a currently selected world object
        // do not place if the mouse press is already consumed by a UI element
        if (_selectedObjectData != null && _canPlaceObject && selectedObject == null && !Input.MousePressConsumed && !Input.MouseHoverConsumed)
        {
            //Debug.Log($"Enforce grid: {_selectedObjectData.enforceGrid}");
            int snapSize = _selectedObjectData.enforceGrid ? (int)SnapSize.Whole : (int)_snapSize;
            Point snappedPosition = CalculateSmartPlacement(mousePos, _selectedObjectData.size, _selectedObjectData.frame, snapSize, _rotation, out bool invalidPlacement);
            if (Input.Get("place").Pressed || (Input.Get("place").Holding && _swipe))
            {
                if (invalidPlacement)
                {
                    // play error sound
                    _invalidSound.Play();
                }
                else
                {
                    justPlaced = true;
                    PlaceObject(_selectedObjectData, snappedPosition, _grid.activeLayer);
                }
            }
        }

        // check for object selection
        // reverse order in order to hover the topmost selected object
        for (int i = _grid.layers[_grid.activeLayer].LevelObjects.Count - 1; i >= 0; i--)
        {
            LevelObject levelObject = _grid.layers[_grid.activeLayer].LevelObjects[i];

            // if the mouse hover has already been consumed by a UI element, do not allow hovering over world objects
            if (Input.MouseHoverConsumed)
            {
                Dehover();
                break;
            }

            // update object hover consumed
            if (levelObject == hoveredObject)
            {
                _objectHoverConsumed = true;
            }
            
            // check for object hovering
            if (levelObject.bounds.Contains(mousePos) && levelObject != hoveredObject)
            {
                 // do not allow hovering if the cursor is by a resizing handle
                if (selectedObject != null)
                {
                    if (IsNearAnyHandle(mousePos, selectedObject))
                    {
                        break;
                    }
                }

                Hover(levelObject);
                _canPlaceObject = false;
                break;
            }
            else if (!levelObject.bounds.Contains(mousePos) && levelObject == hoveredObject)
            {
                Dehover();
                _canPlaceObject = true;
            }
        }

        // check for object movement
        if (selectedObject != null)
        {
            var direction = Input.Get("move").Point;

            if (Input.Get("move").Pressed)
            {
                var newPos = new Point(selectedObject.transform.position.X + direction.X * (int)_snapSize, selectedObject.transform.position.Y + direction.Y * (int)_snapSize);
                if (!OverlapsExistingObject(newPos, selectedObject.data.size, selectedObject.transform.rotation, selectedObject))
                {
                    selectedObject.SetPosition(newPos);
                }
            }
        }

        // check for object deletion
        if (hoveredObject != null && !justPlaced && Input.Get("remove").Pressed)
        {
            RemoveObject(hoveredObject);
            _canPlaceObject = true;
        }
        if (hoveredObject != null && !justPlaced && _swipe && Input.Get("remove").Holding)
        {
            RemoveObject(hoveredObject);
            _canPlaceObject = true;
        }

        // check for object selection
        if (Input.Get("select").Pressed && !justPlaced)
        {
            if (hoveredObject != null)
            {
                if (selectedObject == hoveredObject && !Input.MouseHoverConsumed)
                {
                    Deselect();
                }
                else
                {
                    Select(hoveredObject);
                }
            }
            else if (selectedObject != null && !Input.MouseHoverConsumed)
            {
                // only deselect if the mouse isn't near a resize handle
                if (!IsNearAnyHandle(mousePos, selectedObject))
                {
                    Deselect();
                }
            }
        }

        // check for object resizing
        Resizing();
    }

    public void Select(LevelObject levelObject)
    {
        selectedObject?.SetSelected(false);
        selectedObject = levelObject;
        selectedObject.SetSelected(true);
        
        // inherit the selected object's properties
        _rotation = selectedObject.transform.rotation;
        _flipX = selectedObject.transform.flipX;
        _flipY = selectedObject.transform.flipY;

        //Debug.Log("Selected Object Material?: " + selectedObject.data.material);
    }

    bool IsNearAnyHandle(Point mousePos, LevelObject levelObject)
    {
        Rectangle bounds = levelObject.bounds;

        Point[] handlePositions = new Point[]
        {
            new Point(bounds.Left,     bounds.Top),      // TopLeft
            new Point(bounds.Center.X, bounds.Top),      // Top
            new Point(bounds.Right,    bounds.Top),      // TopRight
            new Point(bounds.Right,    bounds.Center.Y), // Right
            new Point(bounds.Right,    bounds.Bottom),   // BottomRight
            new Point(bounds.Center.X, bounds.Bottom),   // Bottom
            new Point(bounds.Left,     bounds.Bottom),   // BottomLeft
            new Point(bounds.Left,     bounds.Center.Y), // Left
        };

        foreach (Point p in handlePositions)
        {
            if (Math.Abs(mousePos.X - p.X) < resizeDistance &&
                Math.Abs(mousePos.Y - p.Y) < resizeDistance)
                return true;
        }

        return false;
    }

    void Resizing()
    {
        // check for object resizing
        if (selectedObject != null && selectedObject.data.scalable)
        {
            int resizeDistance = 8;
            float parallaxFactor = _grid.showParallax ? _grid.layers[_grid.activeLayer].ParallaxFactor : 0.0f;
            Point mousePos = Camera.Instance.ScreenToWorld(Input.Get("cursor").Vector, parallaxFactor).ToPoint();

            bool atTopEdge    = Math.Abs(mousePos.Y - selectedObject.bounds.Top)    < resizeDistance;
            bool atBottomEdge = Math.Abs(mousePos.Y - selectedObject.bounds.Bottom) < resizeDistance;
            bool atLeftEdge   = Math.Abs(mousePos.X - selectedObject.bounds.Left)   < resizeDistance;
            bool atRightEdge  = Math.Abs(mousePos.X - selectedObject.bounds.Right)  < resizeDistance;

            // Begin drag
            if (Input.Get("select").Pressed && _activeHandle == ResizeHandle.None)
            {
                ResizeHandle handle = ResizeHandle.None;

                if      (atTopEdge    && atLeftEdge)  handle = ResizeHandle.TopLeft;
                else if (atTopEdge    && atRightEdge) handle = ResizeHandle.TopRight;
                else if (atBottomEdge && atLeftEdge)  handle = ResizeHandle.BottomLeft;
                else if (atBottomEdge && atRightEdge) handle = ResizeHandle.BottomRight;
                else if (atTopEdge)                   handle = ResizeHandle.Top;
                else if (atBottomEdge)                handle = ResizeHandle.Bottom;
                else if (atLeftEdge)                  handle = ResizeHandle.Left;
                else if (atRightEdge)                 handle = ResizeHandle.Right;

                if (handle != ResizeHandle.None)
                {
                    _activeHandle    = handle;
                    _mouseDragStart  = mousePos;
                    _dragStartBounds = selectedObject.bounds;
                    Input.ConsumePress(); // prevent deselect from firing
                }
            }

            // apply drag
            if (Input.Get("select").Holding && _activeHandle != ResizeHandle.None)
            {
                int snapValue = (int)_snapSize;

                // raw delta snapped to grid increments
                int rawDX = mousePos.X - _mouseDragStart.X;
                int rawDY = mousePos.Y - _mouseDragStart.Y;
                int dx = (rawDX / snapValue) * snapValue;
                int dy = (rawDY / snapValue) * snapValue;

                // remap drag delta to match object's local axes
                (dx, dy) = selectedObject.transform.rotation switch
                {
                    90  => ( dy, -dx),
                    180 => (-dx, -dy),
                    270 => (-dy,  dx),
                    _   => ( dx,  dy),
                };

                int newX = _dragStartBounds.X;
                int newY = _dragStartBounds.Y;
                int newW = _dragStartBounds.Width;
                int newH = _dragStartBounds.Height;

                int minSize = snapValue; // never collapse below one grid cell

                switch (_activeHandle)
                {
                    case ResizeHandle.TopLeft:
                        newX = _dragStartBounds.X + dx;
                        newY = _dragStartBounds.Y + dy;
                        newW = Math.Max(_dragStartBounds.Width  - dx, minSize);
                        newH = Math.Max(_dragStartBounds.Height - dy, minSize);
                        break;
                    case ResizeHandle.Top:
                        newY = _dragStartBounds.Y + dy;
                        newH = Math.Max(_dragStartBounds.Height - dy, minSize);
                        break;
                    case ResizeHandle.TopRight:
                        newY = _dragStartBounds.Y + dy;
                        newW = Math.Max(_dragStartBounds.Width  + dx, minSize);
                        newH = Math.Max(_dragStartBounds.Height - dy, minSize);
                        break;
                    case ResizeHandle.Right:
                        newW = Math.Max(_dragStartBounds.Width + dx, minSize);
                        break;
                    case ResizeHandle.BottomRight:
                        newW = Math.Max(_dragStartBounds.Width  + dx, minSize);
                        newH = Math.Max(_dragStartBounds.Height + dy, minSize);
                        break;
                    case ResizeHandle.Bottom:
                        newH = Math.Max(_dragStartBounds.Height + dy, minSize);
                        break;
                    case ResizeHandle.BottomLeft:
                        newX = _dragStartBounds.X + dx;
                        newW = Math.Max(_dragStartBounds.Width  - dx, minSize);
                        newH = Math.Max(_dragStartBounds.Height + dy, minSize);
                        break;
                    case ResizeHandle.Left:
                        newX = _dragStartBounds.X + dx;
                        newW = Math.Max(_dragStartBounds.Width - dx, minSize);
                        break;
                }

                // clamp position when size hits minimum (left/top edges)
                if (newW == minSize && (_activeHandle == ResizeHandle.Left
                                    || _activeHandle == ResizeHandle.TopLeft
                                    || _activeHandle == ResizeHandle.BottomLeft))
                {
                    newX = _dragStartBounds.Right - minSize;
                }
                if (newH == minSize && (_activeHandle == ResizeHandle.Top
                                    || _activeHandle == ResizeHandle.TopLeft
                                    || _activeHandle == ResizeHandle.TopRight))
                {
                    newY = _dragStartBounds.Bottom - minSize;
                }

                selectedObject.SetPosition(new Point(newX, newY));
                selectedObject.SetSize(new Point(newW, newH));
            }

            // end drag
            if (Input.Get("select").Released)
            {
                _activeHandle = ResizeHandle.None;
            }
        }
    }

    public void Deselect()
    {
        selectedObject?.SetSelected(false);
        selectedObject = null;
    }

    public void Hover(LevelObject levelObject)
    {
        hoveredObject?.SetHovered(false);
        hoveredObject = levelObject;
        hoveredObject.SetHovered(true);
    }

    public void Dehover()
    {
        hoveredObject?.SetHovered(false);
        hoveredObject = null;
    }

    public Point SnapToGrid(Point pos, int snapSize)
    {
        int x = (int)Math.Floor(pos.X / (double)snapSize) * snapSize;
        int y = (int)Math.Floor(pos.Y / (double)snapSize) * snapSize;
        return new Point(x, y);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float parallaxFactor = _grid.showParallax ? _grid.layers[_grid.activeLayer].ParallaxFactor : 0.0f;
        Point mousePos = Camera.Instance.ScreenToWorld(Input.Get("cursor").Vector, parallaxFactor).ToPoint();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Instance.GetParallaxTransform(parallaxFactor));

        // draw tile debug
        if (!Input.MouseHoverConsumed && _grid.showHitboxes)
        {
            Point gridPos = SnapToGrid(mousePos, _selectedObjectData.enforceGrid ? (int)SnapSize.Whole : (int)_snapSize);
            Point gridSize = new Point((int)_snapSize, (int)_snapSize);
            Rectangle gridRect = new Rectangle(gridPos, gridSize);
            Debug.DrawRectangleOutline(spriteBatch, gridRect, Color.White, 1);
        }

        // draw preview
        if (_selectedObjectData != null && _canPlaceObject && selectedObject == null && !Input.MouseHoverConsumed)
        {
            int snapSize = _selectedObjectData.enforceGrid ? (int)SnapSize.Whole : (int)_snapSize;
            Point snappedPos = CalculateSmartPlacement(mousePos, _selectedObjectData.size, _selectedObjectData.frame, snapSize, _rotation, out bool invalidPlacement);
            Color objectColor = _grid.colorObjects ? _selectedColor : Color.White;
            Color color = invalidPlacement ? Color.Red * 0.5f : objectColor * 0.5f;
            DrawPlacementPreview(spriteBatch, _selectedObjectData, snappedPos, _rotation, _flipX, _flipY, color);
        }

        if (selectedObject != null && selectedObject.data.scalable)
        {
            DrawHandles(spriteBatch, selectedObject, _activeHandle);
        }

        spriteBatch.End();
    }

    public void DrawPlacementPreview(SpriteBatch spriteBatch, LevelObjectData data, Point pos, float rotation, bool flipX, bool flipY, Color color)
    {
        Point framedSize = data.frame != Point.Zero ? data.frame : data.size;

        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(framedSize.Y, framedSize.X) : new Point(framedSize.X, framedSize.Y);
        Rectangle bounds = new Rectangle(pos.X, pos.Y, rotatedSize.X, rotatedSize.Y);

        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);

        Vector2 origin = new Vector2(framedSize.X / 2f, framedSize.Y / 2f);
        float radians = rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (flipX) effects |= SpriteEffects.FlipHorizontally;
        if (flipY) effects |= SpriteEffects.FlipVertically;

        Rectangle? sourceRect = data.frame != Point.Zero ? new Rectangle(data.defaultFramePos, data.frame) : null;

        spriteBatch.Draw(data.sprite, drawPos, sourceRect, color, radians, origin, Vector2.One, effects, 0f);
    }

    void DrawHandles(SpriteBatch spriteBatch, LevelObject levelObject, ResizeHandle activeHandle)
    {
        int handleSize = 6;
        int half = handleSize / 2;

        Point[] handlePositions = new Point[]
        {
            new Point(levelObject.bounds.Left,              levelObject.bounds.Top),               // TopLeft
            new Point(levelObject.bounds.Center.X,          levelObject.bounds.Top),               // Top
            new Point(levelObject.bounds.Right,             levelObject.bounds.Top),               // TopRight
            new Point(levelObject.bounds.Right,             levelObject.bounds.Center.Y),          // Right
            new Point(levelObject.bounds.Right,             levelObject.bounds.Bottom),            // BottomRight
            new Point(levelObject.bounds.Center.X,          levelObject.bounds.Bottom),            // Bottom
            new Point(levelObject.bounds.Left,              levelObject.bounds.Bottom),            // BottomLeft
            new Point(levelObject.bounds.Left,              levelObject.bounds.Center.Y),          // Left
        };

        for (int i = 0; i < handlePositions.Length; i++)
        {
            // enum starts at None=0, TopLeft=1, so offset by 1
            bool isActive = (int)activeHandle == i + 1;

            Point p = handlePositions[i];
            Rectangle handle = new Rectangle(p.X - half, p.Y - half, handleSize, handleSize);

            spriteBatch.Draw(_resizeHandleTexture, handle, Color.White);

            if (isActive)
            {
                spriteBatch.Draw(_resizeHandleSelectedTexture, handle, Color.White);
            }
        }
    }

    public void PlaceObject(LevelObjectData levelObjectData, Point point, int layer)
    {
        CommandInvoker.Execute(new PlaceObjectCommand(this, levelObjectData, point, _grid, _flipX, _flipY, _rotation, layer, _grid.colorObjects ? _selectedColor : null));

        _placeSound.Play();

        if (levelObjectData.placeSound != null)
        {
            AudioManager.Get(levelObjectData.placeSound).Play();
        }
    }

    public void RemoveObject(LevelObject levelObject)
    {
        CommandInvoker.Execute(new RemoveObjectCommand(this, _grid, levelObject));
            
        _removeSound.Play();
    }

    public void SetSelectedObjectData(LevelObjectData levelObjectData)
    {
        _selectedObjectData = levelObjectData;
    }

    public Point CalculatePlacement(Point pos, Point size, int snapSize, int rotation, out bool invalidPlacement)
    {
        pos = SnapToGrid(pos, snapSize);
        invalidPlacement = OverlapsExistingObject(pos, size, rotation);

        return pos;
    }

    public bool OverlapsExistingObject(Rectangle rect, LevelObject ignoreObject = null)
    {
        foreach (LevelObject obj in _grid.layers[_grid.activeLayer].LevelObjects)
        {
            if (obj == ignoreObject) continue;
            if (rect.Intersects(obj.bounds))
                return true;
        }
        return false;
    }

    public bool OverlapsExistingObject(Point pos, Point size, int rotation, LevelObject ignoreObject = null)
    {
        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(size.Y, size.X) : new Point(size.X, size.Y);
        Rectangle bounds = new Rectangle(pos.X, pos.Y, rotatedSize.X, rotatedSize.Y);

        return OverlapsExistingObject(bounds, ignoreObject);
    }

    // calculate the best position to place an object based on the current mouse position and the positions of existing objects
    public Point CalculateSmartPlacement(Point position, Point size, Point frame, int snapSize, int rotation, out bool invalidPlacement, LevelObject ignoreObject = null)
    {
        // snap the mouse position to the grid
        Point snappedMouse = SnapToGrid(position, snapSize);

        // account for frame
        Point framedSize = (frame != Point.Zero) ? frame : size;

        // account for rotation
        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(framedSize.Y, framedSize.X) : new Point(framedSize.X, framedSize.Y);

        // center offset, rounded to nearest snap increment
        int centerOffsetX = (rotatedSize.X / 2 / snapSize) * snapSize;
        int centerOffsetY = (rotatedSize.Y / 2 / snapSize) * snapSize;

        Point centeredPosition = new Point(snappedMouse.X - centerOffsetX, snappedMouse.Y - centerOffsetY);

        // search nearby grid-aligned offsets, closest first
        // the constraint: at least one edge of the object must still touch snappedMouse's grid cell
        // so the offset range is limited to [-objectSize+snapValue, objectSize-snapValue] in each axis
        int maxOffsetX = rotatedSize.X - snapSize;
        int maxOffsetY = rotatedSize.Y - snapSize;

        // build candidates sorted by distance from centeredPosition
        List<(Point point, int distSq)> candidates = new();

        for (int dx = -maxOffsetX; dx <= maxOffsetX; dx += snapSize)
        {
            for (int dy = -maxOffsetY; dy <= maxOffsetY; dy += snapSize)
            {
                Point candidate = new Point(snappedMouse.X + dx, snappedMouse.Y + dy); // was: - dx, - dy
                int distSq = (candidate.X - centeredPosition.X) * (candidate.X - centeredPosition.X)
                        + (candidate.Y - centeredPosition.Y) * (candidate.Y - centeredPosition.Y);
                candidates.Add((candidate, distSq));
            }
        }

        candidates.Sort((a, b) => a.distSq.CompareTo(b.distSq));

        foreach (var (candidate, _) in candidates)
        {
            if (!OverlapsExistingObject(candidate, framedSize, rotation, ignoreObject))
            {
                invalidPlacement = false;
                return candidate;
            }
        }

        // no valid placement found
        invalidPlacement = true;
        return centeredPosition;
    }
}

public enum SnapSize
{
    Whole = 16,
    Half = 8,
    Pixel = 1
}

public enum ResizeHandle
{
    None,
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}