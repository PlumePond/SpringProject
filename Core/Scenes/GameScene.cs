using System;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Content;
using SpringProject.Core.Content.Types.LevelObjects;
using SpringProject.Core.Editor;
using SpringProject.Core.SaveSystem;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Scenes;

public class GameScene : Scene
{
    public Camera Camera { get; private set; }
    public Grid Grid { get; private set; }

    public static string levelName;

    public override void Initialize()
    {
        base.Initialize();

        Grid = new Grid(false);

        LevelSaveManager.Load(levelName, Grid);

        Texture2D panelTexture = TextureManager.Get("panel");
        Texture2D panelSelectedTexture = TextureManager.Get("panel_selected");

        Camera = new GameCamera(Main.Graphics, 4, Grid, Player.Instance?.transform);

        Grid.SetShowAllLayers(true);
        Grid.SetShowParallax(true);

        Panel savePanel = new Panel(new Point(-5, 5), new Point(42, 47), Vector2.One, Origin.TopRight, Anchor.TopRight, panelTexture, 3);

        ButtonElement editorButton = new ButtonElement(new Point(5, 26), new Point(32, 16), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, panelSelectedTexture, 3);
        editorButton.Pressed += () =>
        {
            LevelEditor.levelName = levelName;
            Main.SetScene<LevelEditor>(true);
        };
        ImageElement saveButtonIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("edit_icon"), Main.UIDefaultColor);
        editorButton.AddChild(saveButtonIcon);
        savePanel.AddChild(editorButton);

        ButtonElement mainMenuButton = new ButtonElement(new Point(5, 5), new Point(32, 16), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, panelSelectedTexture, 3);
        mainMenuButton.Pressed += () => 
        { 
            Main.SetScene<MainMenu>(false);
        };
        ImageElement mainMenuIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("menu_icon"), Main.UIDefaultColor);
        mainMenuButton.AddChild(mainMenuIcon);
        savePanel.AddChild(mainMenuButton);

        ActiveCanvas.AddChild(savePanel);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Grid.Update(gameTime);
        Camera.Update(gameTime);

        if (Input.Get("show_hitboxes").Pressed)
        {
            Grid.SetShowHitboxes(!Grid.showHitboxes);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Grid.Draw(spriteBatch);

        base.Draw(spriteBatch);
    }
}