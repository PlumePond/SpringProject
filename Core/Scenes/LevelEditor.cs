using System;
using System.Reflection.PortableExecutable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Commands;
using SpringProject.Core.Content;
using SpringProject.Core.Editor;
using SpringProject.Core.SaveSystem;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Scenes;

public class LevelEditor : Scene
{
    public Grid ActiveGrid { get; private set; }
    public GridPlacement GridPlacement { get; private set; }
    public Camera Camera { get; private set; }

    Panel _colorViewPanel;
    TextElement _debugText;

    Action<float> _setHueEvent;
    Action<float> _setSaturationEvent;
    Action<float> _setValueEvent;
    Action<float> _setAlphaEvent;

    Color _selectedColor = Color.White;
    
    float _hue = 0.0f;
    float _saturation = 0.0f;
    float _value = 1.0f;
    float _alpha = 1.0f;

    public static string levelName;

    public override void Initialize()
    {
        base.Initialize();

        ActiveGrid = new Grid(true);
        GridPlacement = new GridPlacement(ActiveGrid);

        // initialize camera
        Camera = new EditorCamera(Main.Graphics, 4, ActiveGrid);

        Texture2D panelTexture = TextureManager.Get("panel");
        Texture2D panelSelectedTexture = TextureManager.Get("panel_selected");
        Texture2D panelPressedTexture = TextureManager.Get("panel_dark");

        Texture2D sliderTexture = TextureManager.Get("panel_dark");
        Texture2D sliderFillTexture = TextureManager.Get("slider_fill");
        Texture2D colorDisplayTexture = TextureManager.Get("color_display");

        ActiveCanvas.AddChild(new ImageElement(Point.Zero, Vector2.One, Origin.TopLeft, Anchor.TopLeft, TextureManager.Get("gradient-1"), Color.White));

        // create a panel element and add it to the canvas
        Panel panel1 = new Panel(new Point(5, -5), new Point(LevelObjectLoader.LevelObjectDataDictionary.Count * 40 + 50, 50), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture);
        HorizontalArray horizontalArray = new HorizontalArray(new Point(5, 0), new Point(40, 40), Vector2.One, 5);
        ActiveCanvas.AddChild(panel1);
        panel1.AddChild(horizontalArray);

        Panel colorPanel = new Panel(new Point(-5, -5), new Point(110, 202), Vector2.One, Origin.BottomRight, Anchor.BottomRight, panelTexture);
        Slider hueSlider = new Slider(new Point(5, 110), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 360f, 0f, 3);
        Slider saturationSlider = new Slider(new Point(5, 121), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 0f, 3);
        Slider valueSlider = new Slider(new Point(5, 132), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 1f, 3);
        Slider alphaSlider = new Slider(new Point(5, 147), new Point(100, 10), Vector2.One, Origin.TopLeft, Anchor.TopLeft, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 1f, 3);
        _colorViewPanel = new Panel(new Point(5, 5), new Point(100, 100), Vector2.One, Origin.TopLeft, Anchor.TopLeft, colorDisplayTexture, 3);

        TextElement hueText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "H", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        hueSlider.AddChild(hueText);
        TextElement saturationText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "S", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        saturationSlider.AddChild(saturationText);
        TextElement valueText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "V", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        valueSlider.AddChild(valueText);
        TextElement alphaText = new TextElement(new Point(3, 3), Vector2.One * 0.25f, FontManager.Get("body"), "A", Color.Black * 0.5f, Origin.TopLeft, Anchor.TopLeft);
        alphaSlider.AddChild(alphaText);

        // set bg color button
        ButtonElement setBGColorButton = new ButtonElement(new Point(26, -5), new Point(32, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
        setBGColorButton.Pressed += SetBackground;
        ImageElement setBGColorIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("set_bg_color"), Main.UIDefaultColor);
        setBGColorButton.AddChild(setBGColorIcon);
        colorPanel.AddChild(setBGColorButton);

        // get bg color button
        ButtonElement getBGColorButton = new ButtonElement(new Point(63, -5), new Point(32, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
        getBGColorButton.Pressed += GetBackground;
        ImageElement getBGColorIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("get_bg_color"), Main.UIDefaultColor);
        getBGColorButton.AddChild(getBGColorIcon);
        colorPanel.AddChild(getBGColorButton);

        // paint objects toggle
        ToggleElement colorObjects = new ToggleElement(new Point(5, -25), new Point(16, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, TextureManager.Get("color_objects"), false, 3);
        Input.Get("paint_objects").PressedEvent += colorObjects.Toggle;
        colorObjects.ValueChanged += ActiveGrid.SetColorObjects;
        colorPanel.AddChild(colorObjects);

        // set object color button
        ButtonElement setObjectColorButton = new ButtonElement(new Point(26, -25), new Point(32, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
        setObjectColorButton.Pressed += SetObjectColor;
        ImageElement setObjectColorIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("set_object_color"), Main.UIDefaultColor);
        setObjectColorButton.AddChild(setObjectColorIcon);
        colorPanel.AddChild(setObjectColorButton);

        // get object color button
        ButtonElement getObjectColorButton = new ButtonElement(new Point(63, -25), new Point(32, 16), Vector2.One, Origin.BottomLeft, Anchor.BottomLeft, panelTexture, panelSelectedTexture, 3);
        getObjectColorButton.Pressed += GetObjectColor;
        ImageElement getObjectColorIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("get_object_color"), Main.UIDefaultColor);
        getObjectColorButton.AddChild(getObjectColorIcon);
        colorPanel.AddChild(getObjectColorButton);

        Panel gridPanel = new Panel(new Point(5, 5), new Point(68, 89), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, 16);

        ToggleElement showLayers = new ToggleElement(new Point(5, 5), new Point(16, 16), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, panelSelectedTexture, TextureManager.Get("show_layers"), false, 3);
        Input.Get("show_all_layers").PressedEvent += showLayers.Toggle;
        showLayers.ValueChanged += ActiveGrid.SetShowAllLayers;
        gridPanel.AddChild(showLayers);

        ToggleElement showHitboxes = new ToggleElement(new Point(0, 5), new Point(16, 16), Vector2.One, Origin.TopCenter, Anchor.TopCenter, panelTexture, panelSelectedTexture, TextureManager.Get("show_hitboxes"), false, 3);
        Input.Get("show_hitboxes").PressedEvent += showHitboxes.Toggle;
        showHitboxes.ValueChanged += ActiveGrid.SetShowHitboxes;
        gridPanel.AddChild(showHitboxes);

        ToggleElement showGridLines = new ToggleElement(new Point(-5, 5), new Point(16, 16), Vector2.One, Origin.TopRight, Anchor.TopRight, panelTexture, panelSelectedTexture, TextureManager.Get("show_grid_lines"), false, 3);
        Input.Get("show_grid_lines").PressedEvent += showGridLines.Toggle;
        showGridLines.ValueChanged += ActiveGrid.SetShowGridLines;
        gridPanel.AddChild(showGridLines);

        ToggleElement showParallax = new ToggleElement(new Point(5, 26), new Point(16, 16), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, panelSelectedTexture, TextureManager.Get("show_parallax"), false, 3);
        Input.Get("show_parallax").PressedEvent += showParallax.Toggle;
        showParallax.ValueChanged += ActiveGrid.SetShowParallax;
        gridPanel.AddChild(showParallax);
        
        ActiveCanvas.AddChild(gridPanel);

        Panel savePanel = new Panel(new Point(-5, 5), new Point(42, 68), Vector2.One, Origin.TopRight, Anchor.TopRight, panelTexture, 3);

        ButtonElement saveButton = new ButtonElement(new Point(5, 47), new Point(32, 16), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, panelSelectedTexture, 3);
        saveButton.Pressed += SaveLevel;
        ImageElement saveButtonIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("save_icon"), Main.UIDefaultColor);
        saveButton.AddChild(saveButtonIcon);
        savePanel.AddChild(saveButton);

        ButtonElement playButton = new ButtonElement(new Point(5, 26), new Point(32, 16), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, panelSelectedTexture, 3);
        playButton.Pressed += () =>
        {
            GameScene.levelName = levelName;
            Main.SetScene<GameScene>(true);
        };
        playButton.AddChild(new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("play_icon"), Main.UIDefaultColor));
        savePanel.AddChild(playButton);

        ButtonElement mainMenuButton = new ButtonElement(new Point(5, 5), new Point(32, 16), Vector2.One, Origin.TopLeft, Anchor.TopLeft, panelTexture, panelSelectedTexture, 3);
        mainMenuButton.Pressed += OpenMainMenu;
        ImageElement mainMenuIcon = new ImageElement(new Point(0, 0), Vector2.One, Origin.MiddleCenter, Anchor.MiddleCenter, TextureManager.Get("menu_icon"), Main.UIDefaultColor);
        mainMenuButton.AddChild(mainMenuIcon);
        savePanel.AddChild(mainMenuButton);

        ActiveCanvas.AddChild(savePanel);

        _debugText = new TextElement(new Point(5, 99), Vector2.One * 0.5f, FontManager.Get("body"), "", Color.White, Origin.TopLeft, Anchor.TopLeft);
        ActiveCanvas.AddChild(_debugText);

        colorPanel.AddChild(hueSlider);
        colorPanel.AddChild(saturationSlider);
        colorPanel.AddChild(valueSlider);
        colorPanel.AddChild(alphaSlider);
        colorPanel.AddChild(_colorViewPanel);
        ActiveCanvas.AddChild(colorPanel);

        hueSlider.ChangeValue += OnHueChanged;
        saturationSlider.ChangeValue += OnSaturationChanged;
        valueSlider.ChangeValue += OnValueChanged;
        alphaSlider.ChangeValue += OnAlphaChanged;

        _setHueEvent += hueSlider.SetValue;
        _setSaturationEvent += saturationSlider.SetValue;
        _setValueEvent += valueSlider.SetValue;
        _setAlphaEvent += alphaSlider.SetValue;

        foreach (LevelObjectData levelObjectData in LevelObjectLoader.LevelObjectDataDictionary.Values)
        {
            LevelObjectElement levelObjectElement = new LevelObjectElement(Point.Zero, Vector2.One, new Point(40, 40), levelObjectData, GridPlacement);
            horizontalArray.AddChild(levelObjectElement);
        }

        LevelSaveManager.Load(levelName, ActiveGrid);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (ActiveCanvas != null)
        {
            ActiveCanvas.Update(gameTime);
        }

        ActiveGrid.Update(gameTime);
        GridPlacement.Update(gameTime);
        Camera.Update(gameTime);

        _debugText.SetText($"Layer: {ActiveGrid.layers[ActiveGrid.activeLayer].Name}");

        if (Input.Get("undo").Pressed)
        {
            CommandInvoker.Undo();
        }

        if (Input.Get("redo").Pressed)
        {
            CommandInvoker.Redo();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Transform);
        // draw grid
        ActiveGrid.Draw(spriteBatch);
        GridPlacement.Draw(spriteBatch);

        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Transform);
        
        spriteBatch.End();

        base.Draw(spriteBatch);
    }

    public override void Close()
    {
        base.Close();
    }

    void SaveLevel()
    {
        LevelSaveManager.Save(ActiveGrid);
    }

    void SetBackground()
    {
        ActiveGrid.SetFogColor(_selectedColor);
        ActiveGrid.SetBackgroundColor(Extensions.FromHSV(_hue, _saturation, _value));
    }

    void GetBackground()
    {
        _selectedColor = ActiveGrid.FogColor;
        SelectColor(ActiveGrid.FogColor);
    }

    void SetObjectColor()
    {
        GridPlacement.selectedObject?.SetColor(_selectedColor);
    }

    void GetObjectColor()
    {
        if (GridPlacement.selectedObject != null)
        {
            Color objectColor = GridPlacement.selectedObject.color;
            SelectColor(objectColor);
        }
    }

    void OpenMainMenu()
    {
        Main.SetScene<MainMenu>(false);
    }

    void SelectColor(Color color)
    {
        GridPlacement.SelectColor(color);
        _colorViewPanel.SetColor(color);
        Extensions.HSV hsv = Extensions.ToHSV(color);

        _hue = (float)hsv.H;
        _saturation = (float)hsv.S / 100.0f;
        _value = (float)hsv.V / 100.0f;
        _alpha = color.A;

        _setHueEvent?.Invoke(_hue);
        _setSaturationEvent?.Invoke(_saturation);
        _setValueEvent?.Invoke(_value);
        _setAlphaEvent?.Invoke(_alpha);
    }

    void OnHueChanged(float hue)
    {
        _hue = hue;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        GridPlacement.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }

    void OnSaturationChanged(float saturation)
    {
        _saturation = saturation;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        GridPlacement.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }

    void OnValueChanged(float value)
    {
        _value = value;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        GridPlacement.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }

    void OnAlphaChanged(float alpha)
    {
        _alpha = alpha;
        _selectedColor = Extensions.FromHSV(_hue, _saturation, _value) * _alpha;
        GridPlacement.SelectColor(_selectedColor);
        _colorViewPanel.SetColor(Extensions.FromHSV(_hue, _saturation, _value));
    }
}