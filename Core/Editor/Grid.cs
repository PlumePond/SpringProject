using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.SaveSystem;
using SpringProject.Core.UI;

namespace SpringProject.Core.Editor;

public class Grid
{
    public GridLayer[] layers { get; private set; }
    public int activeLayer { get; private set; }
    public bool showHitboxes { get; private set; } = false;
    public bool showAllLayers { get; private set; } = false;
    public bool colorObjects { get; private set; } = false;
    public bool showGridLines { get; private set; } = false;
    public bool showParallax { get; private set; } = false;
    public Point size { get; private set; }
    public int GridSize { get; private set; } = 16;

    public int FogColorIndex { get; private set; } = 0;
    public int BackgroundColorIndex { get; private set; } = 0;
    public Font DebugFont { get; private set; }

    public bool editor { get; private set; }

    RasterizerState _rasterizerState = new RasterizerState { ScissorTestEnable = true };
    
    List<LevelObject> _placementQueue = new();
    List<LevelObject> _removalQueue = new();

    public Grid(bool editor)
    {
        this.editor = editor;

        layers =
        [
            new GridLayer("FG1", -1.0f, false, false),
            new GridLayer("FG2", -0.6f, false, false),
            new GridLayer("FG3", -0.2f, false, false),
            new GridLayer("Mid1", 0.0f, false, false),
            new GridLayer("Mid2", 0.0f, false, true),
            new GridLayer("Mid3", 0.0f, true, false),
            new GridLayer("BG1", 0.1f, true, false),
            new GridLayer("BG2", 0.3f, true, false),
            new GridLayer("BG3", 0.5f, true, false),
            new GridLayer("BG4", 0.6f, true, false),
            new GridLayer("BG5", 0.8f, true, false),
            new GridLayer("BG6", 1.0f, true, false),
        ];

        activeLayer = 4;

        DebugFont = FontManager.Get("body");
    }

    public void QueueRemove(LevelObject obj)
    {
        _removalQueue.Add(obj);
    }
    
    public void QueuePlace(LevelObject obj)
    {
        _placementQueue.Add(obj);
    }

    public void Update(GameTime gameTime)
    {
        for (int layer = 0; layer < layers.Length; layer++)
        {
            foreach (LevelObject levelObject in layers[layer].LevelObjects)
            {
                if (editor)
                {
                    levelObject.EditorUpdate(gameTime);
                }
                else
                {
                    levelObject.Update(gameTime);
                }
            }
        }

        foreach (var obj in _removalQueue)
        {
            layers[obj.layer].LevelObjects.Remove(obj);
            obj.OnRemoved();
        }
        _removalQueue.Clear();

        foreach (var obj in _placementQueue)
        {
            layers[obj.layer].LevelObjects.Add(obj);
            obj.OnPlaced();
        }
        _placementQueue.Clear();
    }

    public void FixedUpdate(GameTime gameTime)
    {
        for (int layer = 0; layer < layers.Length; layer++)
        {
            foreach (LevelObject levelObject in layers[layer].LevelObjects)
            {
                if (!editor)
                {
                    levelObject.FixedUpdate(gameTime);
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Debug.Log($"Size: {size}");
        Rectangle originalScissor = Main.Graphics.ScissorRectangle;

        // convert the world-space grid bounds to screen space
        Vector2 topLeft = Camera.Instance.WorldToScreen(Vector2.Zero);
        Vector2 bottomRight = Camera.Instance.WorldToScreen(new Vector2(size.X * 16, size.Y * 16));

        Rectangle scissorRect = new Rectangle(topLeft.ToPoint(), (bottomRight - topLeft).ToPoint());
        Main.Graphics.ScissorRectangle = scissorRect;

        BackgroundPass(spriteBatch);
        LayerPass(spriteBatch);
        OutlinePass(spriteBatch);
        GridLinePass(spriteBatch);

        Main.Graphics.ScissorRectangle = originalScissor;
    }

    void BackgroundPass(SpriteBatch spriteBatch)
    {
        Color original = ColorManager.Get(BackgroundColorIndex);
        Color opaque = new Color(original.R, original.G, original.B, (byte)255);

        Main.Graphics.Clear(Color.Black);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, _rasterizerState, null, Camera.Instance.Transform);
        spriteBatch.Draw(TextureManager.Get("pixel"), new Rectangle(Point.Zero, new Point(size.X * 16, size.Y * 16)), opaque);
        spriteBatch.End();
    }

    void LayerPass(SpriteBatch spriteBatch)
    {
        // iterate through the layers backward
        for (int layer = layers.Length - 1; layer >= 0; layer--)
        {  
            if (layer + 1 < layers.Length)
            {
                if (layers[layer + 1].HasFog)
                {
                    DrawFog(spriteBatch, ColorManager.Get(FogColorIndex));
                }
            }

            float parallaxFactor = showParallax ? layers[layer].ParallaxFactor : 0.0f;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, _rasterizerState, null, Camera.Instance.GetParallaxTransform(parallaxFactor));

            foreach (LevelObject levelObject in layers[layer].LevelObjects)
            {
                // if the level object is selected or hovered, draw it in the outline pass instead
                if (levelObject.selected || levelObject.hovered) continue;

                Color tint = Color.White;

                if (!showAllLayers)
                {
                    tint = layer == activeLayer ? Color.White : Color.White * 0.1f;
                }

                // draw the object
                levelObject.SetTint(tint);
                
                if (editor)
                {
                    levelObject.DrawEditor(spriteBatch);
                }
                else
                {
                    levelObject.Draw(spriteBatch);
                }
                
                // draw the bounds of the object
                // draw object bounds
                if (showHitboxes && layer == activeLayer)
                {
                    levelObject.DrawDebug(spriteBatch, DebugFont);
                }
            }

            spriteBatch.End();
        }
    }

    void OutlinePass(SpriteBatch spriteBatch)
    {
        float gridParallaxFactor = showParallax ? layers[activeLayer].ParallaxFactor : 0.0f;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, _rasterizerState, null, Camera.Instance.GetParallaxTransform(gridParallaxFactor));

        foreach (var levelObject in layers[activeLayer].LevelObjects)
        {
            // don't draw the outline if it is not selected or hovered
            if (!levelObject.selected && !levelObject.hovered) continue;

            levelObject.DrawOutline(spriteBatch);
            levelObject.Draw(spriteBatch);
            
            if (editor)
            {
                levelObject.DrawEditor(spriteBatch);
            }
        }

        spriteBatch.End();
    }

    void GridLinePass(SpriteBatch spriteBatch)
    {
        float gridParallaxFactor = showParallax ? layers[activeLayer].ParallaxFactor : 0.0f;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Camera.Instance.GetParallaxTransform(gridParallaxFactor));

        if (showGridLines)
        {
            DrawGridLines(spriteBatch, Camera.Instance.Zoom);
        }
    
        spriteBatch.End();
    }

    void DrawGridLines(SpriteBatch spriteBatch, float zoom)
    {
        int cellSize = 16; // world-space units, no zoom scaling

        Color gridColor = Color.White * 0.1f;

        // Convert screen corners to world space using the parallax-adjusted transform
        // so the grid snaps correctly for the active layer's parallax factor
        float parallaxFactor = showParallax ? layers[activeLayer].ParallaxFactor : 0.0f;
        Vector2 topLeft     = Camera.Instance.ScreenToWorld(Vector2.Zero, parallaxFactor);
        Vector2 bottomRight = Camera.Instance.ScreenToWorld(
            new Vector2(Main.GameWindow.ClientBounds.Width, Main.GameWindow.ClientBounds.Height),
            parallaxFactor
        );

        // Snap to cell boundaries in world space
        int startX = (int)Math.Floor(topLeft.X / cellSize) * cellSize;
        int startY = (int)Math.Floor(topLeft.Y / cellSize) * cellSize;
        int endX   = (int)Math.Ceiling(bottomRight.X / cellSize) * cellSize;
        int endY   = (int)Math.Ceiling(bottomRight.Y / cellSize) * cellSize;

        for (int x = startX; x <= endX; x += cellSize)
        {
            Debug.DrawLine(spriteBatch, new Vector2(x, startY), new Vector2(x, endY), gridColor, 1);
        }

        for (int y = startY; y <= endY; y += cellSize)
        {
            Debug.DrawLine(spriteBatch, new Vector2(startX, y), new Vector2(endX, y), gridColor, 1);
        }
    }

    public void DrawFog(SpriteBatch spriteBatch, Color color)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, _rasterizerState);

        Point fogPos = new Point(0, 0);
        Point fogSize = new Point(Main.GameWindow.ClientBounds.Size.X, Main.GameWindow.ClientBounds.Size.Y);
        Rectangle fogRect = new Rectangle(fogPos, fogSize);
        Debug.DrawRectangle(spriteBatch, fogRect, color);

        spriteBatch.End();
    }

    public void LoadLevelObjects(LevelObjectSaveData[] levelObjectSaveDataArray)
    {
        if (levelObjectSaveDataArray == null)
        {
            Debug.Log("No Level Data found.");
            return;
        }

        foreach (var data in levelObjectSaveDataArray)
        {
            if (LevelObjectLoader.LevelObjectDataDictionary.ContainsKey(data.dataKey))
            {
                LevelObjectData levelObjectData = LevelObjectLoader.LevelObjectDataDictionary[data.dataKey];
                LevelObject levelObject = (LevelObject)Activator.CreateInstance(levelObjectData.type);
                levelObject.Initialize(levelObjectData, this, data.position);

                levelObject.SetRotation(data.rotation);
                levelObject.SetFlipX(data.flipX);
                levelObject.SetFlipY(data.flipY);
                levelObject.SetColorIndex(data.colorIndex);
                levelObject.SetSize(data.size);

                // restore exposed parameters
                var targets = new List<object> { levelObject };
                targets.AddRange(levelObject.Components);

                foreach (var target in targets)
                {
                    foreach (var param in ParameterScanner.Scan(target))
                    {
                        if (!data.parameters.TryGetValue(param.Label, out var raw)) continue;

                        // Newtonsoft deserializes numbers as long/double by default, so coerce
                        var coerced = CoerceValue(raw, param.ValueType);
                        param.SetValue(coerced);
                    }
                }

                AddLevelObject(data.layer, levelObject);
                levelObject.OnPlaced();
            }
            else
            {
                Debug.Log($"Warning: Level Object '{data.dataKey}' not found!");
            }
        }

        // sort each layer so non-entities come before entities. this is because collisions must be loaded first.
        
        foreach (var layer in layers)
        {
            layer.LevelObjects.Sort((a, b) =>
            {
                bool aIsEntity = a is Entity;
                bool bIsEntity = b is Entity;
                return aIsEntity.CompareTo(bIsEntity); // false (0) sorts before true (1)
            });
        }
    }

    static object CoerceValue(object raw, Type targetType)
    {
        if (raw is Newtonsoft.Json.Linq.JToken token)
            return token.ToObject(targetType);

        // fallback for primitives that came through as wrong numeric type
        return Convert.ChangeType(raw, targetType);
    }

    public bool InsideObject(Point point, int layer, out LevelObject levelObject)
    {
        foreach (LevelObject obj in layers[layer].LevelObjects)
        {
            if (obj.hitbox.Contains(point))
            {
                levelObject = obj;
                return true;
            }
        }
        
        levelObject = null;
        return false;
    }

    public bool RectInsideObject(Rectangle rect, int layer, out LevelObject levelObject, LevelObject ignore = null)
    {
        foreach (LevelObject obj in layers[layer].LevelObjects)
        {
            if (obj == ignore) continue;
            if (!obj.data.solid) continue;
            if (!obj.hitbox.Intersects(rect)) continue;

            levelObject = obj;
            return true;
        }

        levelObject = null;
        return false;
    }

    public void SetSize(Point size)
    {
        this.size = size;
    }

    public void AddLevelObject(int layer, LevelObject levelObject)
    {
        layers[layer].LevelObjects.Add(levelObject);
        levelObject.SetLayer(layer);
    }

    public void SetActiveLayer(int layer)
    {
        activeLayer = layer;
    }

    public void SetFogColorIndex(int index)
    {
        FogColorIndex = index;
    }

    public void SetBackgroundColorIndex(int index)
    {
        BackgroundColorIndex = index;
    }

    public void SetShowAllLayers(bool value)
    {
        showAllLayers = value;
    }

    public void SetColorObjects(bool value)
    {
        colorObjects = value;
    }

    public void SetShowHitboxes(bool value)
    {
        showHitboxes = value;
    }

    public void SetShowGridLines(bool value)
    {
        showGridLines = value;
    }
    
    public void SetShowParallax(bool value)
    {
        showParallax = value;
    }
}