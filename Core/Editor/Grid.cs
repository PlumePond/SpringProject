using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;
using SpringProject.Settings;

namespace SpringProject.Core.Editor;

public class Grid
{
    public LevelObjectData SelectedObjectData => _selectedObjectData;
    
    const int LAYER_COUNT = 16;

    SnapSize _snapSize = SnapSize.Whole;
    GridLayer[] _layers;

    LevelObjectData _selectedObjectData;
    LevelObject _selectedObject = null;
    LevelObject _hoveredObject = null;

    MouseState _prevMouseState;
    bool _canPlaceObject = true;
    bool _swipe = false;
    int _rotation = 0;
    bool _flipX = false;
    bool _flipY = false;
    bool _showHitboxes = false;
    int _activeLayer = 0;
    bool _showAllLayers = false;
    bool _colorObjects = false;
    Color _fogColor = Color.White;
    SpriteFontBase _debugFont;

    Color _selectedColor = Color.White;

    AudioComposite _placeSound => AudioManager.Get("place");
    AudioComposite _removeSound => AudioManager.Get("remove");
    AudioComposite _invalidSound => AudioManager.Get("invalid");

    public int ActiveLayer => _activeLayer;

    public Grid()
    {
        // initialize the grid layers
        _layers = new GridLayer[LAYER_COUNT];
        for (int i = 0; i <_layers.Length; i++)
        {
            _layers[i] = new GridLayer();
        }

        _debugFont = FontManager.Get("body");
    }

    public void AddLevelObject(LevelObject levelObject, Point position, int layer)
    {
        _layers[layer].LevelObjects.Add(levelObject);
    }

    public void SelectColor(Color color)
    {
        _selectedColor = color;
    }

    public void SetFogColor(Color color)
    {
        _fogColor = color;
    }

    public void SetShowAllLayers(bool showAllLayers)
    {
        _showAllLayers = showAllLayers;
    }

    public void SetColorObjects(bool colorObjects)
    {
        _colorObjects = colorObjects;
    }

    public void Update(GameTime gameTime)
    {
        // used to prevent placing an object and selecting it in the same frame
        bool justPlaced = false;

        _swipe = Input.Get("swipe").Holding;

        if (Input.Get("rotate_ccw").Pressed)
        {
            _rotation = (_rotation + 270) % 360;
            if (_selectedObject != null)
            {
                _selectedObject.RotateCounterClockwise();
            }
        }
        else if (Input.Get("rotate_cw").Pressed)
        {
            _rotation = (_rotation + 90) % 360;
            if (_selectedObject != null)
            {
                _selectedObject.RotateClockwise();
            }
        }

        if (Input.Get("flip_x").Pressed)
        {
            _flipX = !_flipX;
            if (_selectedObject != null)
            {
                _selectedObject.SetFlipX(_flipX);
            }
        }

        if (Input.Get("flip_y").Pressed)
        {
            _flipY = !_flipY;
            if (_selectedObject != null)
            {
                _selectedObject.SetFlipY(_flipY);
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

        if (Input.Get("show_hitboxes").Pressed)
        {
            _showHitboxes = !_showHitboxes;
        }

        if (Input.Get("layer_up").Pressed)
        {
            _activeLayer++;
            if (_activeLayer > _layers.Length - 1)
            {
                _activeLayer = _layers.Length - 1;
            }

            _selectedObject = null;
            _hoveredObject = null;
            _canPlaceObject = true;
        }   
        if (Input.Get("layer_down").Pressed)
        {
            _activeLayer--;
            if (_activeLayer < 0)
            {
                _activeLayer = 0;
            }

            _selectedObject = null;
            _hoveredObject = null;
            _canPlaceObject = true;
        }

        Point mousePos = Main.Camera.ScreenToWorld(Input.Get("cursor").Vector).ToPoint();

        // check for object placement
        // do not place if there is a currently selected world object
        // do not place if the mouse press is already consumed by a UI element
        if (_selectedObjectData != null && _canPlaceObject && _selectedObject == null && !Input.MousePressConsumed && !Input.MouseHoverConsumed)
        {
            Point snappedPosition = CalculateSmartPlacement(mousePos, _selectedObjectData.size, _rotation, out bool invalidPlacement);
            if (Input.Get("place").Pressed || (Input.Get("place").Holding && _swipe))
            {
                if (invalidPlacement)
                {
                    // play error sound
                    _invalidSound.Play();
                }
                else
                {
                    // place object
                    var levelObject = new LevelObject(_selectedObjectData, snappedPosition);
                    levelObject.SetRotation(_rotation);
                    levelObject.SetFlipX(_flipX);
                    levelObject.SetFlipY(_flipY);

                    if (_colorObjects)
                    {
                        levelObject.color = _selectedColor;
                    }
                    
                    AddLevelObject(levelObject, snappedPosition, _activeLayer);
                    justPlaced = true;
                    _placeSound.Play();
                }
            }
        }

        // check for object selection
        foreach (LevelObject levelObject in _layers[_activeLayer].LevelObjects)
        {
            // if the mouse hover has already been consumed by a UI element, do not allow hovering over world objects
            if (Input.MouseHoverConsumed)
            {
                break;
            }
            
            // check for object hovering
            if (levelObject.bounds.Contains(mousePos) && levelObject != _hoveredObject)
            {
                _hoveredObject = levelObject;
                _canPlaceObject = false;
                break;
            }
            else if (!levelObject.bounds.Contains(mousePos) && levelObject == _hoveredObject)
            {
                _hoveredObject = null;
                _canPlaceObject = true;
            }
        }

        // check for object movement
        if (_selectedObject != null)
        {
            Point direction = Input.Get("move").Point;

            if (Input.Get("move").Pressed)
            {
                Point newPos = new Point(_selectedObject.position.X + direction.X * (int)_snapSize, _selectedObject.position.Y + direction.Y * (int)_snapSize);
                if (!OverlapsExistingObject(newPos, _selectedObject.data.size, _selectedObject.rotation, _selectedObject))
                {
                    _selectedObject.SetPosition(newPos);
                }
            }
        }

        // check for object deletion
        if (_hoveredObject != null && !justPlaced && Input.Get("remove").Pressed)
        {
            _layers[_activeLayer].LevelObjects.Remove(_hoveredObject);
            _removeSound.Play();
            _hoveredObject = null;
            _canPlaceObject = true;

            if (_hoveredObject == _selectedObject)
            {
                _selectedObject = null;
            }
        }
        if (_hoveredObject != null && !justPlaced && _swipe && Input.Get("remove").Holding)
        {
            _layers[_activeLayer].LevelObjects.Remove(_hoveredObject);
            _removeSound.Play();
            _hoveredObject = null;
            _canPlaceObject = true;
            
            if (_hoveredObject == _selectedObject)
            {
                _selectedObject = null;
            }
        }

        // check for object selection
        if (Input.Get("select").Pressed && !justPlaced)
        {
            if (_hoveredObject != null)
            {
                if (_selectedObject == _hoveredObject)
                {
                    _selectedObject = null;
                }
                else
                {
                    SelectObject(_hoveredObject);
                }
            }
            else
            {
                _selectedObject = null;
            }
        }
    }

    public void SelectObject(LevelObject selectedObject)
    {
        _selectedObject = selectedObject;
        _rotation = _selectedObject.rotation;
        _flipX = _selectedObject.flipX;
        _flipY = _selectedObject.flipY;

        //Debug.Log("Selected Object Material?: " + _selectedObject.data.material);
    }

    public Point SnapToGrid(Point pos)
    {
        int snapSize = (int)_snapSize;
        int x = (int)Math.Floor(pos.X / (double)snapSize) * snapSize;
        int y = (int)Math.Floor(pos.Y / (double)snapSize) * snapSize;
        return new Point(x, y);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Point mousePos = Main.Camera.ScreenToWorld(Input.Get("cursor").Vector).ToPoint();

        // draw tile debug
        if (!Input.MouseHoverConsumed && _showHitboxes)
        {
            Point gridPos = SnapToGrid(mousePos);
            Point gridSize = new Point((int)_snapSize, (int)_snapSize);
            Rectangle gridRect = new Rectangle(gridPos, gridSize);
            Debug.DrawRectangleOutline(spriteBatch, gridRect, Color.White, 1);
        }
        
        // iterate through the layers backward
        for (int layer = _layers.Length - 1; layer >= 0; layer--)
        {  
            if (_showAllLayers && layer > 1)
            {
                DrawFog(spriteBatch, _fogColor);
            }

            foreach (LevelObject levelObject in _layers[layer].LevelObjects)
            {
                Color tint = Color.White;

                if (!_showAllLayers)
                {
                    tint = layer == _activeLayer ? Color.White : Color.White * 0.25f;
                }

                // draw the object
                DrawLevelObject(spriteBatch, levelObject, tint);
                
                // draw the bounds of the object
                // draw object bounds
                if (_showHitboxes && layer == _activeLayer)
                {
                    Rectangle bounds = levelObject.bounds;
                    Color hitboxColor = levelObject.data.solid ? Color.Green : Color.Blue;
                    Debug.DrawRectangle(spriteBatch, bounds, hitboxColor * 0.25f);

                    bool hovered = levelObject == _hoveredObject;
                    bool selected = levelObject == _selectedObject;

                    string debugText = $"{levelObject.data.material}";
                    Vector2 textPos = bounds.Center.ToVector2();
                    Vector2 textOrigin = _debugFont.MeasureString(debugText) * 0.5f;
                    spriteBatch.DrawString(_debugFont, debugText, textPos, Color.White, 0, textOrigin, Vector2.One * 0.25f);

                    if (hovered)
                    {
                        Debug.DrawRectangleOutline(spriteBatch, bounds, Color.White, 1);   
                    }
                    else if (selected)
                    {
                        Debug.DrawRectangleOutline(spriteBatch, bounds, Color.Yellow, 1);   
                    }
                    else
                    {
                        Debug.DrawRectangleOutline(spriteBatch, bounds, hitboxColor, 1);   
                    }
                }
            }
        }

        // draw preview
        if (_selectedObjectData != null && _canPlaceObject && _selectedObject == null && !Input.MouseHoverConsumed)
        {
            Point snappedPos = CalculateSmartPlacement(mousePos, _selectedObjectData.size, _rotation, out bool invalidPlacement);
            Color objectColor = _colorObjects ? _selectedColor : Color.White;
            Color color = invalidPlacement ? Color.Red * 0.5f : objectColor * 0.5f;
            DrawPlacementPreview(spriteBatch, _selectedObjectData, snappedPos, _rotation, _flipX, _flipY, color);
        }
    }

    public void DrawFog(SpriteBatch spriteBatch, Color color)
    {
        Point fogPos = Main.Camera.ScreenToWorld(new Vector2(-50, -50)).ToPoint();
        Point fogSize = new Point((int)(Main.gameWindow.ClientBounds.Size.X / Main.Camera.Zoom) + 100, (int)(Main.gameWindow.ClientBounds.Size.Y / Main.Camera.Zoom) + 100);
        Rectangle fogRect = new Rectangle(fogPos, fogSize);
        Debug.DrawRectangle(spriteBatch, fogRect, color);
    }

    public void DrawLevelObject(SpriteBatch spriteBatch, LevelObject levelObject, Color tint)
    {
        Rectangle bounds = levelObject.bounds;

        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(levelObject.data.sprite.Width / 2f, levelObject.data.sprite.Height / 2f);
        float radians = levelObject.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (levelObject.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (levelObject.flipY) effects |= SpriteEffects.FlipVertically;

        bool hovered = levelObject == _hoveredObject;
        bool selected = levelObject == _selectedObject;
        Color color = selected ? Color.LightGoldenrodYellow : levelObject.color;

        spriteBatch.Draw(levelObject.data.sprite, drawPos, null, color * tint, radians, origin, Vector2.One, effects, 0f);

        if (hovered)
        {
            spriteBatch.Draw(levelObject.data.outline, drawPos, null, Color.White, radians, origin, Vector2.One, effects, 0f);
        }
        else if (selected)
        {
            spriteBatch.Draw(levelObject.data.outline, drawPos, null, Color.Yellow, radians, origin, Vector2.One, effects, 0f);
        }
    }

    public void DrawPlacementPreview(SpriteBatch spriteBatch, LevelObjectData data, Point pos, float rotation, bool flipX, bool flipY, Color color)
    {
        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(data.size.Y, data.size.X) : new Point(data.size.X, data.size.Y);
        Rectangle bounds = new Rectangle(pos.X, pos.Y, rotatedSize.X, rotatedSize.Y);

        Vector2 drawPos = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
        Vector2 origin = new Vector2(data.sprite.Width / 2f, data.sprite.Height / 2f);
        float radians = rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (flipX) effects |= SpriteEffects.FlipHorizontally;
        if (flipY) effects |= SpriteEffects.FlipVertically;

        spriteBatch.Draw(data.sprite, drawPos, null, color, radians, origin, Vector2.One, effects, 0f);
    }

    public void SetSelectedObjectData(LevelObjectData levelObjectData)
    {
        _selectedObjectData = levelObjectData;
    }

    public Point CalculatePlacement(Point pos, Point size, int rotation, out bool invalidPlacement)
    {
        pos = SnapToGrid(pos);
        invalidPlacement = OverlapsExistingObject(pos, size, rotation);

        return pos;
    }

    public bool OverlapsExistingObject(Rectangle rect, LevelObject ignoreObject = null)
    {
        foreach (LevelObject obj in _layers[_activeLayer].LevelObjects)
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
    public Point CalculateSmartPlacement(Point position, Point size, int rotation, out bool invalidPlacement, LevelObject ignoreObject = null)
    {
        int snapValue = (int)_snapSize;

        // Snap the mouse position to the grid
        Point snappedMouse = SnapToGrid(position);

        // account for rotation
        bool swapDimensions = rotation == 90 || rotation == 270;
        Point rotatedSize = swapDimensions ? new Point(size.Y, size.X) : new Point(size.X, size.Y);

        // Center offset, rounded to nearest snap increment
        int centerOffsetX = (rotatedSize.X / 2 / snapValue) * snapValue;
        int centerOffsetY = (rotatedSize.Y / 2 / snapValue) * snapValue;

        Point centeredPosition = new Point(snappedMouse.X - centerOffsetX, snappedMouse.Y - centerOffsetY);

        // Search nearby grid-aligned offsets, closest first
        // The constraint: at least one edge of the object must still touch snappedMouse's grid cell
        // So the offset range is limited to [-objectSize+snapValue, objectSize-snapValue] in each axis
        int maxOffsetX = rotatedSize.X - snapValue;
        int maxOffsetY = rotatedSize.Y - snapValue;

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
            if (!OverlapsExistingObject(candidate, size, rotation, ignoreObject))
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