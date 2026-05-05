using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
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

    public SnapSize SnapSize = SnapSize.Whole;

    ResizeHandle _activeHandle = ResizeHandle.None;
    Point _mouseDragStart;
    Rectangle _dragStartBounds;

    LevelObjectData _selectedObjectData = null;
    public List<LevelObject> SelectedObjects { get; private set; } = new();
    public LevelObject hoveredObject { get; private set; } = null;
    public Point LastPreviewPosition { get; private set; }
    public bool LastPreviewInvalid { get; private set; }
    
    public bool CanPlaceObject = true;
    bool _swipe = false;
    public int Rotation = 0;
    bool _flipX = false;
    bool _flipY = false;

    bool _objectHoverConsumed = false;
    public bool JustPlaced = false;
    public Point WorldMousePos;
    public Point UIMousePos;
    Point _lastSwipePlacementCell = new Point(int.MinValue, int.MinValue);

    AudioComposite _placeSound => AudioManager.Get("place");
    AudioComposite _removeSound => AudioManager.Get("remove");

    Texture2D _resizeHandleTexture = null;
    Texture2D _resizeHandleSelectedTexture = null;

    public Grid Grid;
    public static Tool CurrentTool;

    public static PointerTool Pointer;
    public static BoxSelectTool BoxSelect;
    public static PaintTool Paint;
    public static DropperTool Dropper;

    public GridPlacement(Grid grid)
    {
        Grid = grid;

        _resizeHandleTexture = TextureManager.Get("resize_handle");
        _resizeHandleSelectedTexture = TextureManager.Get("resize_handle_selected");

        InitializeTools();
    }

    public static void SetTool(Tool tool)
    {
        CurrentTool = tool;
        Cursor.SetCursor(tool.CursorType);
    }

    void InitializeTools()
    {
        Pointer = new PointerTool(this);
        BoxSelect = new BoxSelectTool(this);
        Paint = new PaintTool(this);
        Dropper = new DropperTool(this);

        SetTool(Pointer);
    }

    public void Update(GameTime gameTime)
    {
        // used to prevent placing an object and selecting it in the same frame
        JustPlaced = false;

        _swipe = Input.Get("swipe").Holding;

        float parallaxFactor = Grid.showParallax ? Grid.layers[Grid.activeLayer].ParallaxFactor : 0.0f;
        WorldMousePos = Camera.Instance.ScreenToWorld(Input.Get("cursor").Vector, parallaxFactor).ToPoint();
        UIMousePos = (Input.Get("cursor").Vector / Main.Settings.UISize).ToPoint();

        HandleTools(gameTime);
        HandleRotations();
        HandleFlipping();
        HandleSnapMode();
        HandleLayerSelection();
        HandleHovering();
        HandleResizing();
        HandleDeletion();
    }

    void HandleDeletion()
    {
        if (Input.Get("delete").Pressed)
        {
            BatchRemoveObjects(SelectedObjects);
        }
    }

    void HandleLayerSelection()
    {
        if (Input.Get("layer_up").Pressed)
        {
            Grid.SetActiveLayer(Grid.activeLayer + 1);
            if (Grid.activeLayer > Grid.layers.Length - 1)
            {
                Grid.SetActiveLayer(Grid.layers.Length - 1);
            }

            Deselect();
            Dehover();

            CanPlaceObject = true;
        }   
        if (Input.Get("layer_down").Pressed)
        {
            Grid.SetActiveLayer(Grid.activeLayer - 1);
            if (Grid.activeLayer < 0)
            {
                Grid.SetActiveLayer(0);
            }
            
            Deselect();
            Dehover();

            CanPlaceObject = true;
        }
    }

    void HandleSnapMode()
    {
        // check for snap mode input
        if (Input.Get("snap_half").Holding)
        {
            SnapSize = SnapSize.Half;
        }
        else if (Input.Get("snap_pixel").Holding)
        {
            SnapSize = SnapSize.Pixel;
        }
        else
        {
            SnapSize = SnapSize.Whole;
        }
    }

    void HandleFlipping()
    {
        if (Input.Get("flip_x").Pressed)
        {
            _flipX = !_flipX;

            foreach (var obj in SelectedObjects)
            {
                obj.SetFlipX(_flipX);
            }
        }

        if (Input.Get("flip_y").Pressed)
        {
            _flipY = !_flipY;

            foreach (var obj in SelectedObjects)
            {
                obj.SetFlipX(_flipY);
            }
        }
    }

    void HandleRotations()
    {
        if (Input.Get("rotate_ccw").Pressed)
        {
            Rotation = (Rotation + 270) % 360;

            foreach (var obj in SelectedObjects)
            {
                if (obj.data.scalable) continue;
                obj.RotateCounterClockwise();
            }
        }
        else if (Input.Get("rotate_cw").Pressed)
        {
            Rotation = (Rotation + 90) % 360;
            foreach (var obj in SelectedObjects)
            {
                if (obj.data.scalable) continue;
                obj.RotateClockwise();
            }
        }
    }

    Tool previousTool;

    void HandleTools(GameTime gameTime)
    {
        if (Input.MousePressConsumed) return;
        if (Input.MouseHoverConsumed) return;

        if (Input.Get("dropper").Pressed)
        {
            previousTool = CurrentTool;
            SetTool(Dropper);
        }

        if (Input.Get("dropper").Released)
        {
            SetTool(previousTool);
        }

        CurrentTool.Update(gameTime);

        if (Input.Get("place").Pressed)
        {
            if (hoveredObject != null)
            {
                CurrentTool.PressPrimary(hoveredObject, WorldMousePos, UIMousePos, false);
            }
            else if (!IsNearAnyHandle(WorldMousePos))
            {
                CurrentTool.PressEmpty(WorldMousePos, UIMousePos, _swipe);
            }
        }
        if (Input.Get("place").Holding)
        {
            CurrentTool.Hold(gameTime, hoveredObject);

            if (hoveredObject != null && _swipe)
            {
                CurrentTool.PressPrimary(hoveredObject, WorldMousePos, UIMousePos, true);
            }
            else if (!IsNearAnyHandle(WorldMousePos) && _swipe)
            {
                int snapSize = SelectedObjectData != null && SelectedObjectData.enforceGrid ? (int)SnapSize.Whole: (int)SnapSize; Point currentCell = SnapToGrid(WorldMousePos, snapSize);
                if (currentCell != _lastSwipePlacementCell)
                {
                    _lastSwipePlacementCell = currentCell;
                    CurrentTool.PressEmpty(WorldMousePos, UIMousePos, true); // ← pass true for swipe
                }
            }
        }
        if (Input.Get("place").Released)
        {
            _lastSwipePlacementCell = new Point(int.MinValue, int.MinValue);
            CurrentTool.Release();
        }
        if (Input.Get("remove").Pressed && hoveredObject != null)
        {
            CurrentTool.PressSecondary(hoveredObject, WorldMousePos, UIMousePos, false);
        }
        if (Input.Get("remove").Holding && _swipe && hoveredObject != null)
        {
            CurrentTool.PressSecondary(hoveredObject, WorldMousePos, UIMousePos, false);
        }
    }

    void HandleHovering()
    {
        // check for object selection
        // reverse order in order to hover the topmost selected object
        for (int i = Grid.layers[Grid.activeLayer].LevelObjects.Count - 1; i >= 0; i--)
        {
            LevelObject levelObject = Grid.layers[Grid.activeLayer].LevelObjects[i];

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
            if (levelObject.CanHover(WorldMousePos) && levelObject != hoveredObject)
            {
                // do not allow hovering if the cursor is by a resizing handle
                if (IsNearAnyHandle(WorldMousePos))
                {
                    break;
                }

                Hover(levelObject);
                CanPlaceObject = false;
                break;
            }
            else if (!levelObject.CanHover(WorldMousePos) && levelObject == hoveredObject)
            {
                Dehover();
                CanPlaceObject = true;
            }
        }
    }

    public void Select(LevelObject levelObject)
    {
        if (SelectedObjects.Count == 1 && SelectedObjects[0] == levelObject) return;

        Deselect();
        SelectedObjects.Add(levelObject);
        levelObject.SetSelected(true);

        Rotation = levelObject.transform.rotation;
        _flipX = levelObject.transform.flipX;
        _flipY = levelObject.transform.flipY;
    }

    public void SelectMultiple(List<LevelObject> objects)
    {
        Deselect();
        SelectedObjects = objects;
        foreach (var obj in objects)
            obj.SetSelected(true);
    }

    public bool IsNearAnyHandle(Point mousePos)
    {
        var levelObject = SelectedObjects.Count == 1 ? SelectedObjects[0] : null;

        if (levelObject == null) return false;

        Rectangle bounds = levelObject.hitbox;

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
            if (Math.Abs(mousePos.X - p.X) < levelObject.ResizeDistance &&
                Math.Abs(mousePos.Y - p.Y) < levelObject.ResizeDistance)
                return true;
        }

        return false;
    }

    void HandleResizing()
    {
        if (SelectedObjects.Count != 1) return;
        var selectedObject = SelectedObjects[0];
        if (!selectedObject.data.scalable) return;

        // check for object resizing
        int resizeDistance = 8;
        float parallaxFactor = Grid.showParallax ? Grid.layers[Grid.activeLayer].ParallaxFactor : 0.0f;
        Point mousePos = Camera.Instance.ScreenToWorld(Input.Get("cursor").Vector, parallaxFactor).ToPoint();

        bool atTopEdge    = Math.Abs(mousePos.Y - selectedObject.hitbox.Top)    < resizeDistance;
        bool atBottomEdge = Math.Abs(mousePos.Y - selectedObject.hitbox.Bottom) < resizeDistance;
        bool atLeftEdge   = Math.Abs(mousePos.X - selectedObject.hitbox.Left)   < resizeDistance;
        bool atRightEdge  = Math.Abs(mousePos.X - selectedObject.hitbox.Right)  < resizeDistance;

        // begin drag
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
                _dragStartBounds = selectedObject.hitbox;
                Input.ConsumePress(); // prevent deselect from firing
            }
        }

        // apply drag
        if (Input.Get("select").Holding && _activeHandle != ResizeHandle.None)
        {
            int snapValue = (int)SnapSize;

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

    public void Deselect()
    {
        foreach (var obj in SelectedObjects)
        {
            obj.SetSelected(false);
        }
        SelectedObjects.Clear();
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
        float parallaxFactor = Grid.showParallax ? Grid.layers[Grid.activeLayer].ParallaxFactor : 0.0f;
        Point mousePos = Camera.Instance.ScreenToWorld(Input.Get("cursor").Vector, parallaxFactor).ToPoint();

        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Main.UIMatrtix);
        CurrentTool?.DrawUI(spriteBatch);
        spriteBatch.End();
        
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Instance.GetParallaxTransform(parallaxFactor));
        
        CurrentTool?.DrawWorld(spriteBatch);

        // draw tile debug
        if (!Input.MouseHoverConsumed && Grid.showHitboxes && _selectedObjectData != null)
        {
            Point gridPos = SnapToGrid(mousePos, _selectedObjectData.enforceGrid ? (int)SnapSize.Whole : (int)SnapSize);
            Point gridSize = new Point((int)SnapSize, (int)SnapSize);
            Rectangle gridRect = new Rectangle(gridPos, gridSize);
            Debug.DrawRectangleOutline(spriteBatch, gridRect, Color.White, 1);
        }

        // draw preview
        if (_selectedObjectData != null && CanPlaceObject && SelectedObjects.Count == 0 && !Input.MouseHoverConsumed && !JustPlaced)
        {
            int snapSize = _selectedObjectData.enforceGrid ? (int)SnapSize.Whole : (int)SnapSize;
            LastPreviewPosition = CalculateSmartPlacement(_selectedObjectData, mousePos, snapSize, Rotation, out bool invalidPlacement);
            LastPreviewInvalid = invalidPlacement;

            Color objectColor = Grid.colorObjects ? ColorManager.SelectedColor : Color.White;
            Color color = invalidPlacement ? Color.Red * 0.5f : objectColor * 0.5f;
            DrawPlacementPreview(spriteBatch, _selectedObjectData, LastPreviewPosition, Rotation, _flipX, _flipY, color);
        }

        if (SelectedObjects.Count == 1 && SelectedObjects[0].data.scalable)
        {
            DrawHandles(spriteBatch, SelectedObjects[0], _activeHandle);
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
            new Point(levelObject.hitbox.Left,              levelObject.hitbox.Top),               // TopLeft
            new Point(levelObject.hitbox.Center.X,          levelObject.hitbox.Top),               // Top
            new Point(levelObject.hitbox.Right,             levelObject.hitbox.Top),               // TopRight
            new Point(levelObject.hitbox.Right,             levelObject.hitbox.Center.Y),          // Right
            new Point(levelObject.hitbox.Right,             levelObject.hitbox.Bottom),            // BottomRight
            new Point(levelObject.hitbox.Center.X,          levelObject.hitbox.Bottom),            // Bottom
            new Point(levelObject.hitbox.Left,              levelObject.hitbox.Bottom),            // BottomLeft
            new Point(levelObject.hitbox.Left,              levelObject.hitbox.Center.Y),          // Left
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
        JustPlaced = true;

        CommandInvoker.Execute(new PlaceObjectCommand(this, levelObjectData, point, Grid, _flipX, _flipY, Rotation, layer, Grid.colorObjects ? ColorManager.SelectedColorIndex : 0));

        _placeSound.Play();

        if (levelObjectData.placeSound != null)
        {
            AudioManager.Get(levelObjectData.placeSound).Play();
        }
    }

    public void RemoveObject(LevelObject levelObject)
    {
        CommandInvoker.Execute(new RemoveObjectCommand(this, Grid, levelObject));
            
        _removeSound.Play();
    }

    public void BatchRemoveObjects(List<LevelObject> levelObjects)
    {
        CommandInvoker.Execute(new BatchRemoveObjectsCommand(this, Grid, levelObjects.ToArray()));
            
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
        foreach (LevelObject obj in Grid.layers[Grid.activeLayer].LevelObjects)
        {
            if (obj == ignoreObject) continue;
            if (rect.Intersects(obj.hitbox))
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
    public Point CalculateSmartPlacement(LevelObjectData data, Point position, int snapSize, int rotation, out bool invalidPlacement, LevelObject ignoreObject = null)
    {
        Point snappedMouse = SnapToGrid(position, snapSize);

        Point framedSize = (data.frame != Point.Zero) ? data.frame : data.size;

        // calculate the hitbox offset from the object's origin
        Point hitboxOffset = Point.Zero;
        if (!data.hitbox.Equals(Rectangle.Empty))
        {
            framedSize = data.hitbox.Size;
            hitboxOffset = data.hitbox.Location; // offset from object origin to hitbox
        }

        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(framedSize.Y, framedSize.X) : new Point(framedSize.X, framedSize.Y);

        int centerOffsetX = (rotatedSize.X / 2 / snapSize) * snapSize;
        int centerOffsetY = (rotatedSize.Y / 2 / snapSize) * snapSize;

        Point centeredPosition = new Point(snappedMouse.X - centerOffsetX, snappedMouse.Y - centerOffsetY);

        int maxOffsetX = Math.Max(rotatedSize.X - snapSize, 0);
        int maxOffsetY = Math.Max(rotatedSize.Y - snapSize, 0);

        List<(Point point, int distSq)> candidates = new();

        for (int dx = -maxOffsetX; dx <= maxOffsetX; dx += snapSize)
        {
            for (int dy = -maxOffsetY; dy <= maxOffsetY; dy += snapSize)
            {
                Point candidate = new Point(snappedMouse.X + dx, snappedMouse.Y + dy);

                int distSq = (int)Math.Pow(candidate.X - centeredPosition.X, 2) +  (int)Math.Pow(candidate.Y - centeredPosition.Y, 2);
                candidates.Add((candidate, distSq));
            }
        }

        candidates.Sort((a, b) => a.distSq.CompareTo(b.distSq));

        foreach (var (candidate, _) in candidates)
        {
            if (!OverlapsExistingObject(candidate, framedSize, rotation, ignoreObject))
            {
                invalidPlacement = false;
                // subtract hitbox offset so the object's transform position is correct
                return new Point(candidate.X - hitboxOffset.X, candidate.Y - hitboxOffset.Y);
            }
        }

        invalidPlacement = true;
        return new Point(centeredPosition.X - hitboxOffset.X, centeredPosition.Y - hitboxOffset.Y);
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