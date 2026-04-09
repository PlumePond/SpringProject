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

        _mainCanvas = new Canvas(Point.Zero, scaledSize, Anchor.TopLeft);
        _editorLevelSelectCanvas = new Canvas(Point.Zero, scaledSize, Anchor.TopLeft);
        _saveFileSelectCanvas = new Canvas(Point.Zero, scaledSize, Anchor.TopLeft);

        string panelTexture = "panel";
        string panelSelectedTexture = "panel_selected";
        string panelPressedTexture = "panel_dark";

        string sliderTexture = "panel_dark";
        string sliderFillTexture = "slider_fill";
        string colorDisplayTexture = "color_display";

        Font font = FontManager.Get("body");

        ButtonElement playButton = new ButtonElement(new Point(0, -16), new Point(96, 32), Anchor.MiddleCenter, panelTexture, panelSelectedTexture, 3);
        playButton.Pressed += () =>
        {
            SetActiveCanvas(_saveFileSelectCanvas);
        };
        playButton.AddChild(new TextElement(Point.Zero, font, "Play", Color.White));
        _mainCanvas.AddChild(playButton);

        ButtonElement editorButton = new ButtonElement(new Point(0, 16), new Point(96, 32), Anchor.MiddleCenter, panelTexture, panelSelectedTexture, 3);
        editorButton.Pressed += () =>
        {
            SetActiveCanvas(_editorLevelSelectCanvas);
        };
        editorButton.AddChild(new TextElement(Point.Zero, font, "Editor", Color.White));
        _mainCanvas.AddChild(editorButton);

        LevelSaveManager.LoadAll();

        HorizontalArray horizontalArray = new HorizontalArray(Point.Zero, scaledSize, 10);
        _editorLevelSelectCanvas.AddChild(horizontalArray);

        foreach (var levelData in LevelSaveManager.LoadedLevelsData)
        {
            var levelSelectPanel = new Panel(new Point(0, 16), new Point(64, 64), Anchor.MiddleLeft, panelTexture, 3);
            levelSelectPanel.AddChild(new TextElement(Point.Zero, font, levelData.Key, Color.White));
            horizontalArray.AddChild(levelSelectPanel);

            var levelPlayButton = new ButtonElement(Point.Zero, new Point(16, 16), Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
            levelPlayButton.AddChild(new ImageElement(Point.Zero, Anchor.MiddleCenter, "play_icon", Main.UIDefaultColor));
            levelPlayButton.Pressed += () => 
            { 
                GameScene.levelName = levelData.Key;  
                Main.SetScene<GameScene>();
            };
            levelSelectPanel.AddChild(levelPlayButton);

            var levelEditButton = new ButtonElement(new Point(16, 0), new Point(16, 16), Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
            levelEditButton.AddChild(new ImageElement(Point.Zero, Anchor.MiddleCenter, "edit_icon", Main.UIDefaultColor));
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

        _mainCanvas.AddChild(new TextInputBox(new Point(-5, -5), new Point(30, 7), font, "[insert text]", Color.Black * 0.25f, Color.Black, Anchor.BottomRight));

        SetActiveCanvas(_mainCanvas);
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

        spriteBatch.End();
    }
}