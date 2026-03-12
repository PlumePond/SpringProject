using System;
using System.Diagnostics;
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

namespace SpringProject;

public class Main : Game
{
    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;

    bool _windowSizeUpdatePending = false;

    Canvas _activeCanvas;
    Texture2D _panelTexture;

    public static Matrix UIMatrtix;

    EditorObjectLoader editorObjectLoader;

    public static Main Instance { get; private set; }

    public static object Random { get; internal set; }

    public static SettingsData Settings { get; private set; }

    public static Grid Grid { get; private set; }

    public static Camera Camera { get; private set; }

    public static bool MouseHoverConsumed = false;
    public static bool MousePressConsumed = false;


    public Main()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        Instance = this;
    }

    protected override void Initialize()
    {
        // window setup
        _graphics.HardwareModeSwitch = false; // important for borderless windowed mode to work properly
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;

        // initialize canvas
        Point windowSize = Window.ClientBounds.Size;

        Settings = SettingsLoader.Load("settings.json");

        Point scaledSize = new Point(
            Window.ClientBounds.Width / Settings.UISize,
            Window.ClientBounds.Height / Settings.UISize
        );
        
        _activeCanvas = new Canvas(Point.Zero, scaledSize, Vector2.One, Origin.TopLeft, Anchor.TopLeft);
        Grid = new Grid();
        Camera = new Camera(GraphicsDevice);

        // initialize input states
        Input.AddState("Fullscreen", Keys.F11);
        Input.AddState("SnapHalf", Keys.LeftShift);
        Input.AddState("SnapPixel", Keys.LeftControl);
        Input.AddState("Swipe", Keys.Space);
        Input.AddState("RotateCCW", Keys.Q);
        Input.AddState("RotateCW", Keys.E);
        Input.AddState("FlipX", Keys.Z);
        Input.AddState("FlipY", Keys.X);
        Input.AddDirectionState("Move", Keys.W, Keys.S, Keys.A, Keys.D);

        CalculateUIMatrix();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _panelTexture = Content.Load<Texture2D>("Sprites/ui_panel-1");

        // create a panel element and add it to the canvas
        Panel panel1 = new Panel(new Point(5, -5), new Point(320, 50), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, _panelTexture);
        HorizontalArray horizontalArray = new HorizontalArray(new Point(5, 0), new Point(40, 40), Vector2.One, 5);
        _activeCanvas.AddChild(panel1);
        panel1.AddChild(horizontalArray);

        editorObjectLoader = new EditorObjectLoader(GraphicsDevice, "Data/LevelObjects");
        editorObjectLoader.LoadLevelObjects();

        AudioManager.SetSounds(AudioCompositeLoader.LoadAll("Data/Audio"));

        foreach (LevelObjectData levelObjectData in editorObjectLoader.LevelObjectsDatas.Values)
        {
            LevelObjectElement levelObjectElement = new LevelObjectElement(Point.Zero, Vector2.One, new Point(40, 40), levelObjectData);
            horizontalArray.AddChild(levelObjectElement);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // at the start of each frame, reset the mouse hovering and mouse press consumed flags
        MousePressConsumed = false;
        MouseHoverConsumed = false;

        // check if the window size has changed and apply the new size if necessary
        if (_windowSizeUpdatePending)
        {
            _windowSizeUpdatePending = false;
            _graphics.ApplyChanges();
        }

        if (_activeCanvas != null)
        {
            _activeCanvas.Update(gameTime);
        }

        Grid.Update(gameTime);
        Camera.Update();

        // my girlfriend's name is guinn and she is my favorite person in the world
        // :)

        // update input states
        Input.Update();

        // allow the user to toggle borderless windowed mode by pressing F11
        if (Input.Get("Fullscreen").Pressed)
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Transform);
        // draw grid
        Grid.Draw(_spriteBatch);
        _spriteBatch.End();

        if (_activeCanvas != null)
        {
            _activeCanvas.Draw(_spriteBatch);
        }

        base.Draw(gameTime);
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

        _activeCanvas.SetSize(scaledSize);

        CalculateUIMatrix();
    }

    public void CalculateUIMatrix()
    {
        UIMatrtix = Matrix.CreateScale(Settings.UISize, Settings.UISize, 1);
    }
}
