using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.AI;
using SpringProject.Core.Audio;
using SpringProject.Core.Commands;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
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
    TextElement _debugText;

    Action<float> _setHueEvent;
    Action<float> _setSaturationEvent;
    Action<float> _setValueEvent;
    Action<float> _setAlphaEvent;

    GridArray _levelObjectGrid;
    GridArray _slotGrid;
    
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

        ActiveCanvas.AddChild(new ImageElement(Point.Zero, Anchor.TopLeft, "gradient-1", Color.White));

        Scrollbar scrollbar = new Scrollbar(new Point(90, 5), new Point(4, 179), Anchor.TopLeft, "scroll_bar", "scroll_handle", "scroll_handle_outline", null, 0, 1, 0, 2);
        ActiveCanvas.AddChild(scrollbar);

        Panel objectPanel = new Panel(new Point(3, 3), new Point(88, 183), Anchor.TopLeft, "panel_dark_gold", 3);
        ActiveCanvas.AddChild(objectPanel);

        Panel searchPanel = new Panel(new Point(0, 4), new Point(80, 13), Anchor.TopCenter, "panel_mid", 3);
        searchPanel.AddChild(new ImageElement(new Point(3, 0), Anchor.MiddleLeft, "search_icon", Color.White));
        Panel inputTextPanel = new Panel(new Point(-4, 0), new Point(65, 7), Anchor.MiddleRight, "text_box", 3);
        TextInputBox textInput = new TextInputBox(new Point(1, 0), new Point(65, 7), FontManager.Get("body"), "", Color.Gray, Color.White, Anchor.MiddleLeft);
        textInput.ChangeTextEvent += SearchLevelObjects;
        inputTextPanel.AddChild(textInput);
        searchPanel.AddChild(inputTextPanel);
        objectPanel.AddChild(searchPanel);

        ScrollRect scrollRect = new ScrollRect(new Point(0, -4), new Point(80, 160), Anchor.BottomCenter);
        objectPanel.AddChild(scrollRect);

        scrollRect.ScrollEvent += scrollbar.SetValue;
        scrollbar.ChangeValueEvent += scrollRect.SetScroll;
        scrollRect.UpdateCanScrollEvent += scrollbar.SetCanScroll;
        
        _levelObjectGrid = new GridArray(new Point(0, 0), new Point(80, 160), new Point(16, 16), 0, Anchor.TopLeft);
        _slotGrid = new GridArray(new Point(0, 0), new Point(80, 160), new Point(16, 16), 0, Anchor.TopLeft);
        scrollRect.AddChild(_slotGrid);
        scrollRect.AddChild(_levelObjectGrid);

        string toggleInactiveTexture = "toggle_light_inactive";
        string toggleActiveTexture = "toggle_light_active";
        string selectedTexture = "panel_selected";

        InfoPanel infoPanel = new InfoPanel(new Point(3, -3), new Point(88, 79), Anchor.BottomLeft, "panel_light");
        ActiveCanvas.AddChild(infoPanel);

        // show parallax
        ToggleElement showParralax = new ToggleElement(new Point(97, 3), new Point(16, 16), Anchor.TopLeft, toggleInactiveTexture, toggleActiveTexture, selectedTexture, "show_parallax", false);
        showParralax.ValueChanged += (bool value) => { ActiveGrid.SetShowParallax(value); };
        Input.Get("show_parallax").PressedEvent += showParralax.Toggle;
        ActiveCanvas.AddChild(showParralax);

        // show layers
        ToggleElement showLayers = new ToggleElement(new Point(116, 3), new Point(16, 16), Anchor.TopLeft, toggleInactiveTexture, toggleActiveTexture, selectedTexture, "show_layers", false);
        showLayers.ValueChanged += (bool value) => { ActiveGrid.SetShowAllLayers(value); };
        Input.Get("show_all_layers").PressedEvent += showLayers.Toggle;
        ActiveCanvas.AddChild(showLayers);

        // show hitboxes
        ToggleElement showHitboxes = new ToggleElement(new Point(135, 3), new Point(16, 16), Anchor.TopLeft, toggleInactiveTexture, toggleActiveTexture, selectedTexture, "show_hitboxes", false);
        showHitboxes.ValueChanged += (bool value) => { ActiveGrid.SetShowHitboxes(value); };
        Input.Get("show_hitboxes").PressedEvent += showHitboxes.Toggle;
        ActiveCanvas.AddChild(showHitboxes);

        // show grid
        ToggleElement showGrid = new ToggleElement(new Point(154, 3), new Point(16, 16), Anchor.TopLeft, toggleInactiveTexture, toggleActiveTexture, selectedTexture, "show_grid_lines", false);
        showGrid.ValueChanged += (bool value) => { ActiveGrid.SetShowGridLines(value); };
        Input.Get("show_grid_lines").PressedEvent += showGrid.Toggle;
        ActiveCanvas.AddChild(showGrid);

        RepopulateLevelObjects(_levelObjectGrid, _slotGrid, LevelObjectLoader.LevelObjectDataDictionary.Values.ToArray());

        LevelSaveManager.Load(levelName, ActiveGrid);

        Input.Get("save").PressedEvent += SaveLevel;

        SetupColorPanel();
        SetupDebugPanel();

        // initialize camera
        Camera = new EditorCamera(Main.Graphics, 4, ActiveGrid);
    }

    public override void Start()
    {
        base.Start();

        Cursor.SetEnabled(true);
        SelectColor(0);
    }

    void SetupDebugPanel()
    {
        ActiveCanvas.AddChild(new FPSMeter(new Point(0, 0), FontManager.Get("body"), "fps", Color.White, Anchor.BottomCenter));
    }

    void SetupColorPanel()
    {
        string sliderTexture = "panel_dark";
        string panelTexture = "panel_light_gold";
        string panelSelectedTexture = "panel_selected";
        string sliderFillTexture = "slider_fill";

        Panel colorPanel = new Panel(new Point(-3, -3), new Point(62, 100), Anchor.BottomRight, "panel_light_gold");
        ActiveCanvas.AddChild(colorPanel);

        GridArray colorGrid = new GridArray(new Point(0, 4), new Point(colorPanel.size.X - 8, 50), new Point(10), 1, Anchor.TopCenter);
        colorPanel.AddChild(colorGrid);

        RepopulateColorGrid(colorGrid, ColorManager.Colors);
        ColorManager.ColorListModifiedEvent += (colors) => RepopulateColorGrid(colorGrid, colors);
        
        // _colorViewPanel = new Panel(new Point(3, 3), new Point(10, 10), Anchor.TopLeft, "color_display", 3);
        // colorPanel.AddChild(_colorViewPanel);

        Point sliderSize = new Point(48, 7);
        Point handleSize = new Point(6, 10);
        Slider hueSlider = new Slider(new Point(-4, -35), sliderSize, handleSize, Anchor.BottomRight, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 360f, 0.0f);
        colorPanel.AddChild(hueSlider);
        hueSlider.ChangeValue += OnHueChanged;
        _setHueEvent += hueSlider.SetValue;
        Slider saturationSlider = new Slider(new Point(-4, -25), sliderSize, handleSize, Anchor.BottomRight, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 0.0f);
        colorPanel.AddChild(saturationSlider);
        saturationSlider.ChangeValue += OnSaturationChanged;
        _setSaturationEvent += saturationSlider.SetValue;
        Slider valueSlider = new Slider(new Point(-4, -15), sliderSize, handleSize, Anchor.BottomRight, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 1f, 1.0f);
        colorPanel.AddChild(valueSlider);
        valueSlider.ChangeValue += OnValueChanged;
        _setValueEvent += valueSlider.SetValue;

        Slider alphaSlider = new Slider(new Point(-4, -5), sliderSize, handleSize, Anchor.BottomRight, sliderTexture, panelTexture, panelSelectedTexture, sliderFillTexture, 0f, 255f, 255f);
        colorPanel.AddChild(alphaSlider);
        alphaSlider.ChangeValue += OnAlphaChanged;
        _setAlphaEvent += alphaSlider.SetValue;

        Color textColor = Color.Black * 0.5f;
        
        TextElement hueText = new TextElement(new Point(-5, 0), FontManager.Get("body"), "h", textColor, Anchor.MiddleLeft);
        hueSlider.AddChild(hueText);
        TextElement saturationText = new TextElement(new Point(-5, 0), FontManager.Get("body"), "s", textColor, Anchor.MiddleLeft);
        saturationSlider.AddChild(saturationText);
        TextElement valueText = new TextElement(new Point(-5, 0), FontManager.Get("body"), "v", textColor, Anchor.MiddleLeft);
        valueSlider.AddChild(valueText);
        TextElement alphaText = new TextElement(new Point(-5, 0), FontManager.Get("body"), "a", textColor, Anchor.MiddleLeft);
        alphaSlider.AddChild(alphaText);

        ButtonElement setBGColorButton = new ButtonElement(new Point(4, 38), new Point(16, 16), Anchor.TopLeft, "panel_light", panelSelectedTexture);
        setBGColorButton.AddChild(new ImageElement(new Point(0, 0), Anchor.MiddleCenter, "background_icon", Color.White));
        setBGColorButton.Pressed += SetBackground;
        colorPanel.AddChild(setBGColorButton);

        ButtonElement setObjectColor = new ButtonElement(new Point(23, 38), new Point(16, 16), Anchor.TopLeft, "panel_light", panelSelectedTexture);
        setObjectColor.AddChild(new ImageElement(new Point(0, 0), Anchor.MiddleCenter, "set_object_color", Color.White));
        setObjectColor.Pressed += SetObjectColor;
        colorPanel.AddChild(setObjectColor);
    }

    void RepopulateColorGrid(GridArray colorGrid, List<Color> colors)
    {
        for (int i = 0; i < colors.Count; i++)
        {
            colorGrid.AddChild(new ColorPanel(Point.Zero, i, this, new Point(10), Anchor.TopLeft));
        }
    }

    void RepopulateLevelObjects(GridArray objectGrid, GridArray slotGrid, LevelObjectData[] levelObjectDatas)
    {
        // reset children if there are already children
        if (objectGrid.Children.Count > 0)
        {
            objectGrid.Clear();
        }

        for (int i = 0; i < levelObjectDatas.Length; i++)
        {
            var data = levelObjectDatas[i];
            
            Point frame = data.frame != Point.Zero ? data.frame : data.size;
            var levelObject = new LevelObjectElement(Point.Zero, Anchor.TopLeft, frame, data, GridPlacement);
            objectGrid.AddChild(levelObject);
        }

        objectGrid.PackChildren();

        RepopulateSlots(slotGrid, objectGrid.GridRows);
    }

    void RepopulateSlots(GridArray slotGrid, int rowCount)
    {
        // reset children if there are already children
        if (slotGrid.Children.Count > 0)
        {
            slotGrid.Clear();
        }

        rowCount = Math.Max(10, rowCount);

        int count = rowCount * 5;

        for (int i = 0; i < count; i++)
        {
            ImageElement slot = new ImageElement(Point.Zero, Anchor.TopLeft, "object_slot", Color.White);
            slotGrid.AddChild(slot);
        }

        slotGrid.PackChildren();
    }

    void SearchLevelObjects(string text)
    {
        var datas = new List<LevelObjectData>();
        
        // check to see if any of the names of the level objects contain the value of the search 
        foreach (var kvp in LevelObjectLoader.LevelObjectDataDictionary)
        {
            if (kvp.Key.ToLower().Contains(text.ToLower()))
            {
                datas.Add(kvp.Value);
            }
        }

        RepopulateLevelObjects(_levelObjectGrid, _slotGrid, datas.ToArray());
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
        //_debugText.SetText($"Layer: {ActiveGrid.layers[ActiveGrid.activeLayer].Name}");

        if (Input.Get("undo").Pressed)
        {
            CommandInvoker.Undo();
        }

        if (Input.Get("redo").Pressed)
        {
            CommandInvoker.Redo();
        }

        if (Input.Get("back").Pressed)
        {
            Main.SetScene<MainMenu>();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.Transform);
        // draw grid
        ActiveGrid.Draw(spriteBatch);
        GridPlacement.Draw(spriteBatch);

        base.Draw(spriteBatch);
    }

    public override void Close()
    {
        base.Close();
    }

    void SaveLevel()
    {
        LevelSaveManager.Save(ActiveGrid);
        AudioManager.Get("save").Play();
    }

    void SetBackground()
    {
        ActiveGrid.SetFogColorIndex(ColorManager.SelectedColorIndex);
        ActiveGrid.SetBackgroundColorIndex(ColorManager.SelectedColorIndex);

        Debug.Log("BACKGROUND SET");
    }

    void GetBackground()
    {
        SelectColor(ActiveGrid.BackgroundColorIndex);
    }

    void SetObjectColor()
    {
        GridPlacement.selectedObject?.SetColorIndex(ColorManager.SelectedColorIndex);
    }

    void GetObjectColor()
    {
        if (GridPlacement.selectedObject != null)
        {
            int objectColor = GridPlacement.selectedObject.colorIndex;
            SelectColor(objectColor);
        }
    }

    void OpenMainMenu()
    {
        Main.SetScene<MainMenu>(false);
    }

    public void SelectColor(int index)
    {
        ColorManager.SetColorIndex(index);
        var color = ColorManager.Get(index);

        ColorUtils.HSV hsv = ColorUtils.ToHSV(color);

        _hue = (float)hsv.H;
        _saturation = (float)hsv.S;
        _value = (float)hsv.V;
        _alpha = color.A;

        _setHueEvent?.Invoke(_hue);
        _setSaturationEvent?.Invoke(_saturation);
        _setValueEvent?.Invoke(_value);
        _setAlphaEvent?.Invoke(_alpha);
    }

    void OnHueChanged(float hue)
    {
        _hue = hue;
        RecalculateColor();
    }

    void OnSaturationChanged(float saturation)
    {
        _saturation = saturation;
        RecalculateColor();
    }

    void OnValueChanged(float value)
    {
        _value = value;
        RecalculateColor();
    }

    void OnAlphaChanged(float alpha)
    {
        _alpha = alpha;
        RecalculateColor();
    }

    void RecalculateColor()
    {
        var fromHsv = ColorUtils.FromHSV(_hue, _saturation, _value);
        var alphaColor = new Color(fromHsv.R, fromHsv.G, fromHsv.B, (byte)_alpha);
        ColorManager.Set(ColorManager.SelectedColorIndex, alphaColor);
    }
}