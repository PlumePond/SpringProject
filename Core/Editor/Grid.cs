using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public Color FogColor { get; private set; } = Color.White;
    public Color BackgroundColor { get; private set; } = Color.White;
    public Font DebugFont { get; private set; }

    public bool editor { get; private set; }

    public Grid(bool editor)
    {
        this.editor = editor;

        layers =
        [
            new GridLayer("FG1", -1.0f, false),
            new GridLayer("FG2", -0.6f, false),
            new GridLayer("FG3", -0.2f, false),
            new GridLayer("Mid1", 0.0f, false),
            new GridLayer("Mid2", 0.0f, false),
            new GridLayer("Mid3", 0.0f, false),
            new GridLayer("BG1", 0.3f, true),
            new GridLayer("BG2", 0.4f, true),
            new GridLayer("BG3", 0.5f, true),
            new GridLayer("BG4", 0.6f, true),
            new GridLayer("BG5", 0.8f, true),
            new GridLayer("BG6", 1.0f, true),
        ];

        activeLayer = 4;

        DebugFont = FontManager.Get("body");
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
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Main.Graphics.Clear(BackgroundColor);

        // iterate through the layers backward
        for (int layer = layers.Length - 1; layer >= 0; layer--)
        {  
            if (layer + 1 < layers.Length)
            {
                if (layers[layer + 1].HasFog)
                {
                    DrawFog(spriteBatch, FogColor);
                }
            }

            float parallaxFactor = showParallax ? layers[layer].ParallaxFactor : 0.0f;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Instance.GetParallaxTransform(parallaxFactor));

            foreach (LevelObject levelObject in layers[layer].LevelObjects)
            {
                Color tint = Color.White;

                if (!showAllLayers)
                {
                    tint = layer == activeLayer ? Color.White : Color.White * 0.1f;
                }

                // draw the object
                levelObject.SetTint(tint);
                levelObject.Draw(spriteBatch);
                
                // draw the bounds of the object
                // draw object bounds
                if (showHitboxes && layer == activeLayer)
                {
                    levelObject.DrawDebug(spriteBatch, DebugFont);
                }
            }

            spriteBatch.End();
        }

        float gridParallaxFactor = showParallax ? layers[activeLayer].ParallaxFactor : 0.0f;
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Instance.GetGridLineTransform(gridParallaxFactor));

        if (showGridLines)
        {
            DrawGridLines(spriteBatch, Camera.Instance.Zoom);
        }
    
        spriteBatch.End();
    }

    void DrawGridLines(SpriteBatch spriteBatch, float zoom)
    {
        int rawCellSize = 16;
        int cellSize = (int)(rawCellSize * zoom);
        Color gridColor = Color.White * 0.1f;

        // get the visible world bounds from the camera
        Vector2 topLeft = Camera.Instance.ScreenToWorld(Vector2.Zero);
        Vector2 bottomRight = Camera.Instance.ScreenToWorld(new Vector2(Main.GameWindow.ClientBounds.Width, Main.GameWindow.ClientBounds.Height));

        // snap start positions to the nearest cell boundary
        int startX = (int)Math.Floor(topLeft.X / cellSize) * cellSize;
        int startY = (int)Math.Floor(topLeft.Y / cellSize) * cellSize;
        int endX   = (int)Math.Ceiling(bottomRight.X / cellSize) * cellSize;
        int endY   = (int)Math.Ceiling(bottomRight.Y / cellSize) * cellSize;

        // vertical lines
        for (int x = startX; x <= endX; x += cellSize)
        {
            Debug.DrawLine(spriteBatch, new Vector2(x, startY), new Vector2(x, endY), gridColor, 1);
        }

        // horizontal lines
        for (int y = startY; y <= endY; y += cellSize)
        {
            Debug.DrawLine(spriteBatch, new Vector2(startX, y), new Vector2(endX, y), gridColor, 1);
        }
    }

    public void DrawFog(SpriteBatch spriteBatch, Color color)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);

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

        foreach (var levelObjectSaveData in levelObjectSaveDataArray)
        {
            if (LevelObjectLoader.LevelObjectDataDictionary.ContainsKey(levelObjectSaveData.dataKey))
            {
                LevelObjectData levelObjectData = LevelObjectLoader.LevelObjectDataDictionary[levelObjectSaveData.dataKey];
                LevelObject levelObject = (LevelObject)Activator.CreateInstance(levelObjectData.type);
                levelObject.Initialize(levelObjectData, this, levelObjectSaveData.position);

                levelObject.SetRotation(levelObjectSaveData.rotation);
                levelObject.SetFlipX(levelObjectSaveData.flipX);
                levelObject.SetFlipY(levelObjectSaveData.flipY);
                levelObject.SetColor(levelObjectSaveData.color);
                levelObject.SetSize(levelObjectSaveData.size);

                AddLevelObject(levelObjectSaveData.layer, levelObject);
                levelObject.OnPlaced();
            }
            else
            {
                Debug.Log($"Warning: Level Object '{levelObjectSaveData.dataKey}' not found!");
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

    public void AddLevelObject(int layer, LevelObject levelObject)
    {
        layers[layer].LevelObjects.Add(levelObject);
        levelObject.SetLayer(layer);
    }

    public void SetActiveLayer(int layer)
    {
        activeLayer = layer;
    }

    public void SetFogColor(Color color)
    {
        FogColor = color;
    }

    public void SetBackgroundColor(Color color)
    {
        BackgroundColor = color;
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