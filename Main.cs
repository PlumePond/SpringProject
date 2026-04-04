using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core;
using SpringProject.Core.Audio;
using SpringProject.Core.Content;
using SpringProject.Core.Editor;
using SpringProject.Core.UI;
using SpringProject.Settings;
using SpringProject.Core.UserInput;
using FontStashSharp;
using System.IO;
using SpringProject.Core.Debugging;
using static Extensions;
using System.Net;
using SpringProject.Core.SaveSystem;
using SpringProject.Core.Scenes;
using System.Collections.Generic;

namespace SpringProject;

public class Main : Game
{
    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;

    bool _windowSizeUpdatePending = false;

    public static Matrix UIMatrtix;

    public static Main Instance { get; private set; }

    public static Random Random { get; internal set; }

    public static SettingsData Settings { get; set; }

    public static GraphicsDevice Graphics => Instance.GraphicsDevice;
    public static GameWindow GameWindow => Instance.Window;

    public static Scene ActiveScene;
    public static Scene MainMenuScene;
    public static Scene LevelEditorScene;

    //public static Color UIEnabledColor = new Color(246 / 255f, 244 / 255f, 118 / 255f);
    public static Color UIEnabledColor = new Color(255 / 255f, 255 / 255f, 255 / 255f);
    public static Color UIDefaultColor = new Color(94 / 255f, 91 / 255f, 106 / 255f);

    private static Dictionary<Type, Scene> _sceneCache = new();

    public Main()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Random = new Random();

        Instance = this;
    }

    protected override void Initialize()
    {
        // window setup
        _graphics.HardwareModeSwitch = false; // important for borderless windowed mode to work properly
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;

        Debug.Initialize(GraphicsDevice);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // loads all content needed for the game
        Loader.Load("Data", GraphicsDevice);

        Input.Get("screenshot").PressedEvent = TakeScreenshot;

        CalculateUIMatrix();

        SetScene<MainMenu>(false);
    }

    protected override void Update(GameTime gameTime)
    {
        // update input states
        Input.Update();

        // check if the window size has changed and apply the new size if necessary
        if (_windowSizeUpdatePending)
        {
            _windowSizeUpdatePending = false;
            _graphics.ApplyChanges();
        }

        ActiveScene.Update(gameTime);

        // my girlfriend's name is guinn and she is my favorite person in the world
        // i love her very much and want to cover her with kisses
        // :)

        // allow the user to toggle borderless windowed mode
        if (Input.Get("fullscreen").Pressed)
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        ActiveScene.Draw(_spriteBatch);
    }

    public static void SetScene<T>(bool forceNew = true) where T : Scene, new()
    {
        ActiveScene?.Close();
        ActiveScene = forceNew ? new T() : GetOrCreateScene<T>();
        ActiveScene.Start();
    }

    private static Scene GetOrCreateScene<T>() where T : Scene, new()
    {
        if (!_sceneCache.TryGetValue(typeof(T), out var scene))
        {
            scene = new T();
            _sceneCache[typeof(T)] = scene;
        }
        return scene;
    }

    void OnClientSizeChanged(object sender, EventArgs e)
    {
        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _windowSizeUpdatePending = true;

        Point scaledSize = new Point(
        Window.ClientBounds.Width / Settings.UISize,
        Window.ClientBounds.Height / Settings.UISize
        );

        ActiveScene.ActiveCanvas.SetSize(scaledSize);

        CalculateUIMatrix();
    }

    public void CalculateUIMatrix()
    {
        UIMatrtix = Matrix.CreateScale(Settings.UISize, Settings.UISize, 1);
    }

    public void TakeScreenshot()
    {
        AudioManager.Get("screenshot").Play();

        bool canvasActive = ActiveScene.ActiveCanvas.Active;

        ActiveScene.ActiveCanvas.SetActive(false);

        int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int height = GraphicsDevice.PresentationParameters.BackBufferHeight;
        
        RenderTarget2D screenshot = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);

        // draw the scene to the render target
        GraphicsDevice.SetRenderTarget(screenshot);
        Draw(new GameTime());
        
        // Reset to back buffer
        GraphicsDevice.SetRenderTarget(null);

        string fileName = "";

        if (ActiveScene is GameScene)
        {
            fileName += $"{GameScene.levelName}_in-game";
        }
        else if (ActiveScene is LevelEditor)
        {
            fileName += $"{LevelEditor.levelName}_in-editor";
        }
        else if (ActiveScene is MainMenu)
        {
            fileName += "main_menu";
        }

        fileName += $"_{DateTime.Now.ToString("MM-dd-yy_HH-mm-ss")}";
        fileName += ".png";

        string path = Path.Combine("Data", "Screenshots", fileName);
        
        using (Stream stream = File.OpenWrite(path))
        {
            screenshot.SaveAsPng(stream, screenshot.Width, screenshot.Height);
        }
        screenshot.Dispose();

        ActiveScene.ActiveCanvas.SetActive(canvasActive);
    }
}