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
using System.Net;
using SpringProject.Core.SaveSystem;
using SpringProject.Core.Scenes;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

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
    //public static Color UIEnabledColor = new Color(255 / 255f, 255 / 255f, 255 / 255f);
    public static Color UIDefaultColor = new Color(36, 28, 24);
    public static Color UIEnabledColor = new Color(255, 187, 15);

    public static Color HoverOutlineColor = Color.White;
    public static Color SelectedOutlineColor = new Color(255, 187, 15);
    public static Color SelectedTintColor = new Color(255, 187, 15);

    double _accumulator = 0.0f;
    public const double FIXED_TIMESTEP = 1.0 / 60.0;
    const int MAX_STEPS = 5;
    GameTime _fixedGameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(FIXED_TIMESTEP));

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
        AudioManager.CreateChannel("ambience");
        AudioManager.CreateChannel("sfx");

        // window setup
        _graphics.HardwareModeSwitch = false; // important for borderless windowed mode to work properly
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
        
        Debug.Initialize(GraphicsDevice);
        base.Initialize();

        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();
    }

    bool _audioInitialized = false;

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        if (!_audioInitialized)
        {
            var dummy = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
            dummy.Dispose();
            // loads all content needed for the game
            Loader.Load("Data", GraphicsDevice);
            _audioInitialized = true;
        }

        Input.Get("screenshot").PressedEvent = TakeScreenshot;

        CalculateUIMatrix();

        SetScene<MainMenu>(false);

        // LevelEditor.levelName = "test_world-1";
        // SetScene<LevelEditor>(false);

        IsMouseVisible = false;
        Cursor.SetEnabled(true);
        Cursor.SetCursor(CursorType.Pointer);

        ApplySettings();
    }

    void Accumulate(GameTime gameTime)
    {
        _accumulator += gameTime.ElapsedGameTime.TotalSeconds;

        var _remainingSteps = 0;
        while (_accumulator >= FIXED_TIMESTEP && _remainingSteps < MAX_STEPS)
        {
            FixedUpdate(_fixedGameTime);
            _accumulator -= FIXED_TIMESTEP;
            _remainingSteps++;
        }
    }

    public void ApplySettings()
    {
        IsFixedTimeStep = Settings.VSync;
        _graphics.SynchronizeWithVerticalRetrace = Settings.VSync;
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
        Accumulate(gameTime);

        // my girlfriend's name is guinn and she is my favorite person in the world
        // i love her very much and want to cover her with kisses
        // :)

        // allow the user to toggle borderless windowed mode
        if (Input.Get("fullscreen").Pressed)
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }

        AudioManager.Update(gameTime);

        Cursor.Update(gameTime);
        
        base.Update(gameTime);
    }

    void FixedUpdate(GameTime gameTime)
    {
        ActiveScene.FixedUpdate(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        ActiveScene.Draw(_spriteBatch);
        Cursor.Draw(_spriteBatch);
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

        NotificationManager.Notify($"Screenshot taken!");
    }
}