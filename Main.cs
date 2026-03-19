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

namespace SpringProject;

public class Main : Game
{
    GraphicsDeviceManager _graphics;
    SpriteBatch _spriteBatch;

    bool _windowSizeUpdatePending = false;

    Canvas _activeCanvas;

    public static Matrix UIMatrtix;

    public static Main Instance { get; private set; }

    public static object Random { get; internal set; }

    public static SettingsData Settings { get; set; }

    public static Grid Grid { get; private set; }

    public static Camera Camera { get; private set; }

    public static GraphicsDevice graphicsDevice => Instance.GraphicsDevice;
    public static GameWindow gameWindow => Instance.Window;

    Color _selectedColor = Color.White;
    Color _backgroundColor = Color.White;
    float _hue = 0.0f;
    float _saturation = 0.0f;
    float _value = 1.0f;
    float _alpha = 1.0f;

    Panel _colorViewPanel;
    TextElement _debugText;

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

        Camera = new Camera(GraphicsDevice);

        Debug.Initialize(GraphicsDevice);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // loads all content needed for the game
        Loader.Load("Data", graphicsDevice);
        Grid = new Grid();

        // initialize canvas
        Point windowSize = Window.ClientBounds.Size;

        Point scaledSize = new Point(
            Window.ClientBounds.Width / Settings.UISize,
            Window.ClientBounds.Height / Settings.UISize
        );
        
        _activeCanvas = new Canvas(Point.Zero, scaledSize, Vector2.One, Origin.TopLeft, Anchor.TopLeft);

        CalculateUIMatrix();

        Texture2D panelTexture = TextureManager.Get("panel");
        Texture2D panelSelectedTexture = TextureManager.Get("panel_selected");
        Texture2D panelPressedTexture = TextureManager.Get("panel_dark");

        Texture2D sliderTexture = TextureManager.Get("panel_dark");
        Texture2D sliderFillTexture = TextureManager.Get("slider_fill");
        Texture2D colorDisplayTexture = TextureManager.Get("color_display");

        // create a panel element and add it to the canvas
        Panel panel1 = new Panel(new Point(5, -5), new Point(LevelObjectLoader.LevelObjectDataDictionary.Count * 40 + 50, 50), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture);
        HorizontalArray horizontalArray = new HorizontalArray(new Point(5, 0), new Point(40, 40), Vector2.One, 5);
        _activeCanvas.AddChild(panel1);
        panel1.AddChild(horizontalArray);

        Panel colorPanel = new Panel(new Point(-5, -5), new Point(110, 202), Vector2.One, Origin.BottomRight, Anchor.BottomRight, panelTexture);
        Slider hueSlider = new Slider(new Point(5, 110), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 360f, 3);
        Slider saturationSlider = new Slider(new Point(5, 121), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 3);
        Slider valueSlider = new Slider(new Point(5, 132), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 3);
        Slider alphaSlider = new Slider(new Point(5, 147), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 3);
        _colorViewPanel = new Panel(new Point(5, 5), new Point(100, 100), Vector2.One, Origin.TopLeft, Anchor.TopLeft, colorDisplayTexture, 3);
        
        TextElement hueText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "H", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        hueSlider.AddChild(hueText);
        TextElement saturationText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "S", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        saturationSlider.AddChild(saturationText);
        TextElement valueText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "V", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        valueSlider.AddChild(valueText);
        TextElement alphaText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "A", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        alphaSlider.AddChild(alphaText);

        ButtonElement setBGColorButton = new ButtonElement(new Point(5, -5), new Point(32, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
        setBGColorButton.Pressed += SetBackground;
        ImageElement backgroundIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("background_icon"));
        setBGColorButton.AddChild(backgroundIcon);
        colorPanel.AddChild(setBGColorButton);

        ToggleElement showLayers = new ToggleElement(new Point(5, -25), new Point(16, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, TextureManager.Get("show_layers_on"), TextureManager.Get("show_layers_off"), false, 3);
        Input.Get("show_all_layers").PressedEvent += showLayers.Toggle;
        showLayers.ValueChanged += Grid.SetShowAllLayers;
        colorPanel.AddChild(showLayers);

        ToggleElement colorObjects = new ToggleElement(new Point(26, -25), new Point(16, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, TextureManager.Get("color_objects_on"), TextureManager.Get("color_objects_off"), false, 3);
        Input.Get("paint_objects").PressedEvent += colorObjects.Toggle;
        colorObjects.ValueChanged += Grid.SetColorObjects;
        colorPanel.AddChild(colorObjects);

        _debugText = new TextElement(new Point(5, 5), Vector2.One * 0.5f, FontManager.Get("body"), "", Color.White, Origin.TopLeft, Anchor.TopLeft);
        _activeCanvas.AddChild(_debugText);

        colorPanel.AddChild(hueSlider);
        colorPanel.AddChild(saturationSlider);
        colorPanel.AddChild(valueSlider);
        colorPanel.AddChild(alphaSlider);
        colorPanel.AddChild(_colorViewPanel);
        _activeCanvas.AddChild(colorPanel);

        hueSlider.ChangeValue += OnHueChanged;
        saturationSlider.ChangeValue += OnSaturationChanged;
        valueSlider.ChangeValue += OnValueChanged;
        alphaSlider.ChangeValue += OnAlphaChanged;

        foreach (LevelObjectData levelObjectData in LevelObjectLoader.LevelObjectDataDictionary.Values)
        {
            LevelObjectElement levelObjectElement = new LevelObjectElement(Point.Zero, Vector2.One, new Point(40, 40), levelObjectData);
            horizontalArray.AddChild(levelObjectElement);
        }
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

        if (_activeCanvas != null)
        {
            _activeCanvas.Update(gameTime);
        }

        Grid.Update(gameTime);
        Camera.Update();

        // my girlfriend's name is guinn and she is my favorite person in the world
        // i love her very much and want to cover her with kisses
        // :)

        // allow the user to toggle borderless windowed mode
        if (Input.Get("fullscreen").Pressed)
        {
            _graphics.IsFullScreen = !_graphics.IsFullScreen;
            _graphics.ApplyChanges();
        }

        _debugText.SetText($"Layer: {Grid.ActiveLayer}");
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_backgroundColor);

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

    void SetBackground()
    {
        Grid.SetFogColor(_selectedColor);
        _backgroundColor = Extensions.FromHSV(_hue, _saturation, _value);
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

    void OnHueChanged(float hue)
    {
        _hue = hue;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        Grid.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }

    void OnSaturationChanged(float saturation)
    {
        _saturation = saturation;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        Grid.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }

    void OnValueChanged(float value)
    {
        _value = value;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        Grid.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }

    void OnAlphaChanged(float alpha)
    {
        _alpha = alpha;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        Grid.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }
}
