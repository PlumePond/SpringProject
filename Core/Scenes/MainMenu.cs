using System;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Editor;
using SpringProject.Core.SaveSystem;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Scenes;

public class MainMenu : Scene
{
    Canvas _mainCanvas;
    Canvas _editorLevelSelectCanvas;
    Canvas _saveFileSelectCanvas;

    public override void Initialize()
    {
        base.Initialize();

         // initialize canvas
        Point windowSize = Main.GameWindow.ClientBounds.Size;

        Point scaledSize = new Point(
            windowSize.X / Main.Settings.UISize,
            windowSize.Y / Main.Settings.UISize
        );

        _mainCanvas = new Canvas(Point.Zero, scaledSize, Vector2.One, Origin.TopLeft, Anchor.TopLeft);
        _editorLevelSelectCanvas = new Canvas(Point.Zero, scaledSize, Vector2.One, Origin.TopLeft, Anchor.TopLeft);
        _saveFileSelectCanvas = new Canvas(Point.Zero, scaledSize, Vector2.One, Origin.TopLeft, Anchor.TopLeft);

        Texture2D panelTexture = TextureManager.Get("panel");
        Texture2D panelSelectedTexture = TextureManager.Get("panel_selected");
        Texture2D panelPressedTexture = TextureManager.Get("panel_dark");

        Texture2D sliderTexture = TextureManager.Get("panel_dark");
        Texture2D sliderFillTexture = TextureManager.Get("slider_fill");
        Texture2D colorDisplayTexture = TextureManager.Get("color_display");

        SpriteFontBase font = FontManager.Get("body");

        ButtonElement playButton = new ButtonElement(new Point(0, -16), new Point(96, 32), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, panelTexture, panelSelectedTexture, 3);
        playButton.Pressed += () =>
        {
            SetActiveCanvas(_saveFileSelectCanvas);
        };
        playButton.AddChild(new TextElement(Point.Zero, Vector2.One * 0.5f, font, "Play", Color.White));
        _mainCanvas.AddChild(playButton);

        ButtonElement editorButton = new ButtonElement(new Point(0, 16), new Point(96, 32), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, panelTexture, panelSelectedTexture, 3);
        editorButton.Pressed += () =>
        {
            SetActiveCanvas(_editorLevelSelectCanvas);
        };
        editorButton.AddChild(new TextElement(Point.Zero, Vector2.One * 0.5f, font, "Editor", Color.White));
        _mainCanvas.AddChild(editorButton);

        LevelSaveManager.LoadAll();

        HorizontalArray horizontalArray = new HorizontalArray(Point.Zero, scaledSize, Vector2.One, 10);
        _editorLevelSelectCanvas.AddChild(horizontalArray);

        foreach (var levelData in LevelSaveManager.LoadedLevelsData)
        {
            var levelSelectPanel = new Panel(new Point(0, 16), new Point(64, 64), Vector2.One, Origin.MiddleLeft, Anchor.MiddleLeft, panelTexture, 3);
            levelSelectPanel.AddChild(new TextElement(Point.Zero, Vector2.One * 0.5f, font, levelData.Key, Color.White));
            horizontalArray.AddChild(levelSelectPanel);

            var levelPlayButton = new ButtonElement(Point.Zero, new Point(16, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
            levelPlayButton.AddChild(new ImageElement(Point.Zero, Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("play_icon"), Main.UIDefaultColor));
            levelPlayButton.Pressed += () => 
            { 
                GameScene.levelName = levelData.Key;  
                Main.SetScene<GameScene>();
            };
            levelSelectPanel.AddChild(levelPlayButton);

            var levelEditButton = new ButtonElement(new Point(16, 0), new Point(16, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
            levelEditButton.AddChild(new ImageElement(Point.Zero, Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("edit_icon"), Main.UIDefaultColor));
            levelEditButton.Pressed += () => 
            { 
                LevelEditor.levelName = levelData.Key;  
                Main.SetScene<LevelEditor>();
            };
            levelSelectPanel.AddChild(levelEditButton);
        }

        // var newLevelButton = new ButtonElement(new Point(0, 16), new Point(64, 64), Vector2.One, Origin.MiddleLeft, Anchor.MiddleLeft, panelTexture, panelSelectedTexture, 3);
        // newLevelButton.Pressed += () => 
        // { 
        //     LevelEditor.levelName = levelData.Key;  
        //     Main.SetScene<LevelEditor>();
        // };
        // newLevelButton.AddChild(new TextElement(Point.Zero, Vector2.One * 0.5f, font, levelData.Key, Color.White));
        // horizontalArray.AddChild(levelSelectEditButton);

        // grid array for testing purposes
        ScrollRect scrollRect = new ScrollRect(Point.Zero, new Point(80, 160), Vector2.One, Origin.TopLeft, Anchor.TopLeft);
        GridArray gridAray = new GridArray(Point.Zero, new Point(80, 160), Vector2.One, new Point(16, 16), 0, Origin.TopLeft, Anchor.TopLeft);
        scrollRect.AddChild(gridAray);
        _mainCanvas.AddChild(scrollRect);

        for (int i = 0; i < 128; i++)
        {
            ImageElement slot = new ImageElement(Point.Zero, Vector2.One, Origin.TopLeft, Anchor.TopLeft, TextureManager.Get("object_slot"), Color.White);
            slot.AddChild(new TextElement(Point.Zero, Vector2.One * 0.25f, FontManager.Get("body"), i.ToString(), Color.White, Origin.MiddleCenter, Anchor.MiddleCenter));
            gridAray.AddChild(slot);
        }

        SetActiveCanvas(_mainCanvas);
    }

    void SetActiveCanvas(Canvas canvas)
    {
        ActiveCanvas = canvas;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Main.Graphics.Clear(Color.White);

        base.Draw(spriteBatch);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Main.UIMatrtix);

        Texture2D texture = TextureManager.Get("scroll_bar");
        Rectangle frame = new Rectangle(40, 40, 16, 127);

        Rectangle topLeft = new Rectangle(0, 0, 2, 2);
        Rectangle top = new Rectangle(3, 0, 2, 2);
        Rectangle topRight = new Rectangle(6, 0, 2, 2);

        Rectangle left = new Rectangle(0, 3, 2, 2);
        Rectangle mid = new Rectangle(3, 3, 2, 2);
        Rectangle right = new Rectangle(6, 3, 2, 2);
        
        Rectangle bottomLeft = new Rectangle(0, 6, 2, 3);  // Y was 8, should be 6
        Rectangle bottom = new Rectangle(3, 6, 2, 3);  // Y was 8, should be 6
        Rectangle bottomRight = new Rectangle(6, 6, 2, 3);

        UIHelper.DrawSegmentedRepeating(spriteBatch, texture, frame, topLeft, top, topRight, left, mid, right, bottomLeft, bottom, bottomRight);

        spriteBatch.End();
    }
}