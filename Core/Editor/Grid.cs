using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Audio;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Editor;

public class Grid
{
    public LevelObjectData SelectedObjectData => _selectedObjectData;

    SnapSize _snapSize = SnapSize.Whole;
    List<LevelObject> _levelObjects;

    LevelObjectData _selectedObjectData;
    LevelObject _selectedObject = null;
    LevelObject _hoveredObject = null;

    MouseState _prevMouseState;
    bool _canPlaceObject = true;
    bool _smartPlacement = true;
    bool _swipe = false;
    int _rotation = 0;
    bool _flipX = false;
    bool _flipY = false;

    public Grid()
    {
        _levelObjects = new List<LevelObject>();
    }

    public void AddLevelObject(LevelObject levelObject, Point position)
    {
        _levelObjects.Add(levelObject);
    }

    public void Update(GameTime gameTime)
    {
        // used to prevent placing an object and selecting it in the same frame
        bool justPlaced = false;

        _swipe = Input.Get("Swipe").Holding;

        if (Input.Get("RotateCCW").Pressed)
        {
            _rotation = (_rotation + 270) % 360;
            if (_selectedObject != null)
            {
                _selectedObject.RotateCounterClockwise();
                _selectedObject.CalculateBounds((int)_snapSize);
            }
        }
        else if (Input.Get("RotateCW").Pressed)
        {
            _rotation = (_rotation + 90) % 360;
            if (_selectedObject != null)
            {
                _selectedObject.RotateClockwise();
                _selectedObject.CalculateBounds((int)_snapSize);
            }
        }

        if (Input.Get("FlipX").Pressed)
        {
            _flipX = !_flipX;
            if (_selectedObject != null)
            {
                _selectedObject.SetFlipX(_flipX);
            }
        }

        if (Input.Get("FlipY").Pressed)
        {
            _flipY = !_flipY;
            if (_selectedObject != null)
            {
                _selectedObject.SetFlipY(_flipY);
            }
        }

        // check for snap mode input
        if (Input.Get("SnapHalf").Holding)
        {
            _snapSize = SnapSize.Half;
        }
        else if (Input.Get("SnapPixel").Holding)
        {
            _snapSize = SnapSize.Pixel;
        }
        else
        {
            _snapSize = SnapSize.Whole;
        }

        MouseState mouseState = Mouse.GetState();

        // check for object placement
        // do not place if there is a currently selected world object
        // do not place if the mouse press is already consumed by a UI element
        if (_selectedObjectData != null && _canPlaceObject && _selectedObject == null && !Main.MousePressConsumed)
        {
            Point mousePosition = Main.Camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y)).ToPoint();
            Point snappedPosition = CalculateSmartPlacement(mousePosition, _selectedObjectData.size, out bool invalidPlacement);
            if ((mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released) || (_swipe && mouseState.LeftButton == ButtonState.Pressed))
            {
                if (invalidPlacement)
                {
                    // play error sound
                    AudioManager.Get("back").Play();
                }
                else
                {
                    // place object
                    var levelObject = new LevelObject(_selectedObjectData, snappedPosition);
                    levelObject.SetRotation(_rotation);
                    levelObject.SetFlipX(_flipX);
                    levelObject.SetFlipY(_flipY);
                    levelObject.CalculateBounds((int)_snapSize);
                    AddLevelObject(levelObject, snappedPosition);
                    justPlaced = true;
                }
            }
        }

        // check for object selection
        foreach (LevelObject levelObject in _levelObjects)
        {
            // if the mouse hover has already been consumed by a UI element, do not allow hovering over world objects
            if (Main.MouseHoverConsumed)
            {
                break;
            }
            
            // check for object hovering
            Point mousePosition = Main.Camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y)).ToPoint();
            if (levelObject.bounds.Contains(mousePosition) && !levelObject.hovering)
            {
                levelObject.SetHovering(true);

                _hoveredObject = levelObject;
                _canPlaceObject = false;
                break;
            }
            else if (!levelObject.bounds.Contains(mousePosition) && levelObject.hovering)
            { 
                levelObject.SetHovering(false);

                if (_hoveredObject == levelObject)
                {
                    _hoveredObject = null;
                    _canPlaceObject = true;
                }
            }
        }

        // check for object movement
        if (_selectedObject != null)
        {
            Point direction = Input.Get("Move").Point;

            if (Input.Get("Move").Pressed)
            {
                Point newPos = new Point(_selectedObject.position.X + direction.X * (int)_snapSize, _selectedObject.position.Y + direction.Y * (int)_snapSize);
                if (!OverlapsExistingObject(newPos, _selectedObject.data.size, _selectedObject.rotation,  _selectedObject))
                {
                    _selectedObject.SetPosition(newPos);
                }
            }
        }

        // check for object deletion
        if (_hoveredObject != null && mouseState.RightButton == ButtonState.Pressed && _prevMouseState.RightButton == ButtonState.Released && !justPlaced)
        {
            _levelObjects.Remove(_hoveredObject);
            _hoveredObject = null;
            _canPlaceObject = true;
        }
        if (_hoveredObject != null && mouseState.RightButton == ButtonState.Pressed && !justPlaced && _swipe)
        {
            _levelObjects.Remove(_hoveredObject);
            _hoveredObject = null;
            _canPlaceObject = true;
        }

        // check for object selection
        if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released && !justPlaced)
        {
            if (_hoveredObject != null)
            {
                if (_selectedObject == _hoveredObject)
                {
                    _selectedObject = null;
                }
                else
                {
                    _selectedObject = _hoveredObject;
                    _rotation = _selectedObject.rotation;
                    _flipX = _selectedObject.flipX;
                    _flipY = _selectedObject.flipY;
                }
            }
            else
            {
                _selectedObject = null;
            }
        }
        _prevMouseState = mouseState;
    }

    public Point SnapToGrid(Point position)
    {
        int snapValue = (int)_snapSize;
        int snappedX = (position.X / snapValue) * snapValue;
        int snappedY = (position.Y / snapValue) * snapValue;
        return new Point(snappedX, snappedY);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (LevelObject levelObject in _levelObjects)
        {
            Rectangle bounds = levelObject.bounds;
            Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
            Vector2 origin = new Vector2(levelObject.data.sprite.Width / 2f, levelObject.data.sprite.Height / 2f);
            float radians = levelObject.rotation * (float)Math.PI / 180f;

            SpriteEffects effects = SpriteEffects.None;
            if (levelObject.flipX) effects |= SpriteEffects.FlipHorizontally;
            if (levelObject.flipY) effects |= SpriteEffects.FlipVertically;

            bool selected = levelObject == _selectedObject;
            Color color = selected ? Color.LightGoldenrodYellow : levelObject.color;

            spriteBatch.Draw(levelObject.data.sprite, drawPos, null, color, radians, origin, Vector2.One, effects, 0f);

            if (levelObject.hovering)
                spriteBatch.Draw(levelObject.data.outline, drawPos, null, Color.White, radians, origin, Vector2.One, effects, 0f);
            else if (selected)
                spriteBatch.Draw(levelObject.data.outline, drawPos, null, Color.Yellow, radians, origin, Vector2.One, effects, 0f);
        }

        // draw preview
        if (_selectedObjectData != null && _canPlaceObject && _selectedObject == null && !Main.MouseHoverConsumed)
        {
            MouseState mouseState = Mouse.GetState();
            Point mousePosition = Main.Camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y)).ToPoint();
            Point snappedPosition = CalculateSmartPlacement(mousePosition, _selectedObjectData.size, out bool invalidPlacement);

            SpriteEffects effects = SpriteEffects.None;
            if (_flipX) effects |= SpriteEffects.FlipHorizontally;
            if (_flipY) effects |= SpriteEffects.FlipVertically;

            bool swapDimensions = _rotation == 90 || _rotation == 270;
            Point rotatedSize = swapDimensions
                ? new Point(_selectedObjectData.size.Y, _selectedObjectData.size.X)
                : _selectedObjectData.size;

            Point center = new Point(snappedPosition.X + _selectedObjectData.size.X / 2, snappedPosition.Y + _selectedObjectData.size.Y / 2);
            int left = center.X - rotatedSize.X / 2;
            int top  = center.Y - rotatedSize.Y / 2;
            left = (left / (int)_snapSize) * (int)_snapSize;
            top  = (top  / (int)_snapSize) * (int)_snapSize;
            Vector2 drawPosition = new Vector2(left + rotatedSize.X / 2f, top + rotatedSize.Y / 2f);

            Vector2 origin = new Vector2(_selectedObjectData.sprite.Width / 2f, _selectedObjectData.sprite.Height / 2f);
            spriteBatch.Draw(_selectedObjectData.sprite, drawPosition, null, invalidPlacement ? Color.Red * 0.5f : Color.White * 0.5f, _rotation * (float)Math.PI / 180f, origin, Vector2.One, effects, 0f);
        }
    }

    public void SetSelectedObjectData(LevelObjectData levelObjectData)
    {
        _selectedObjectData = levelObjectData;
    }

    public Point CalculatePlacement(Point point, Point size, out bool invalidPlacement)
    {
        point = SnapToGrid(point);
        invalidPlacement = OverlapsExistingObject(point, size);

        return point;
    }

    public bool OverlapsExistingObject(Point point, Point objectSize, int rotation = 0, LevelObject ignoreObject = null)
    {
        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions
            ? new Point(objectSize.Y, objectSize.X)
            : objectSize;

        Point center = new Point(point.X + objectSize.X / 2, point.Y + objectSize.Y / 2);
        Rectangle rect = new Rectangle(center.X - rotatedSize.X / 2, center.Y - rotatedSize.Y / 2, rotatedSize.X, rotatedSize.Y);

        foreach (LevelObject obj in _levelObjects)
        {
            if (obj == ignoreObject) continue;
            if (rect.Intersects(obj.bounds))
                return true;
        }
        return false;
    }

    // calculate the best position to place an object based on the current mouse position and the positions of existing objects. 
    // it will try not to place the object on top of another object if possible.
    // it should snap to the grid based on the snap size.
    // it will try to center the object on the mouse position.
    // however, it can only center the object as much as the grid size allows, 
    // so if the snap size is whole, it will only be able to center the object in increments of 16 pixels, 
    // if the snap size is half, it will be able to center the object in increments of 8 pixels, 
    // and if the snap size is pixel, it will be able to center the object perfectly on the mouse position.
    // at least one part of the object must be placed on the snapped position in the grid where the mouse is,
    // it must never be placed completely off the mouse position
    public Point CalculateSmartPlacement(Point position, Point objectSize, out bool invalidPlacement, LevelObject ignoreObject = null)
    {
        int snapValue = (int)_snapSize;

        // Snap the mouse position to the grid
        Point snappedMouse = SnapToGrid(position);

        // Center offset, rounded to nearest snap increment
        int centerOffsetX = (objectSize.X / 2 / snapValue) * snapValue;
        int centerOffsetY = (objectSize.Y / 2 / snapValue) * snapValue;

        Point centeredPosition = new Point(snappedMouse.X - centerOffsetX, snappedMouse.Y - centerOffsetY);

        // Search nearby grid-aligned offsets, closest first
        // The constraint: at least one edge of the object must still touch snappedMouse's grid cell
        // So the offset range is limited to [-objectSize+snapValue, objectSize-snapValue] in each axis
        int maxOffsetX = objectSize.X - snapValue;
        int maxOffsetY = objectSize.Y - snapValue;

        // Build candidates sorted by distance from centeredPosition
        List<(Point point, int distSq)> candidates = new();

        for (int dx = -maxOffsetX; dx <= maxOffsetX; dx += snapValue)
        {
            for (int dy = -maxOffsetY; dy <= maxOffsetY; dy += snapValue)
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
            if (!OverlapsExistingObject(candidate, objectSize, _rotation, ignoreObject))
            {
                invalidPlacement = false;
                return candidate;
            }
        }

        // No valid placement found
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