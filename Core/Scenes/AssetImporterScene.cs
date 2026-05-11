using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NativeFileDialogCore;
using Newtonsoft.Json;
using SpringProject.Core.Components;
using SpringProject.Core.Content.Types;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Scenes;

public class AssetImporterScene : Scene
{
    const string ContentRoot = "Assets";

    string _levelObjectPath = "";
    string _name = "unnamed_object";
    Texture2D _loadedTexture = null;
    string _type = "level_object";
    string _folder = "";
    string _material;
    bool _solid = false;

    ImageElement _textureDisplay = null;
    ArrayElement _arrayElement = null;

    Dictionary<Notification, Element> _notificationElements = new();

    public override void Initialize()
    {
        base.Initialize();

        SetupNotifcations();

        string panelTexture = "panel";
        string panelSelectedTexture = "panel_selected";

        _arrayElement = new ArrayElement(new Point(3, 3), Point.Zero, 3, ArrayDirection.Down, Anchor.TopLeft);
        ActiveCanvas.AddChild(_arrayElement);

        // import button
        var importButton = new ButtonElement(Point.Zero, new Point(74, 16), Anchor.TopLeft, panelTexture, panelSelectedTexture);
        importButton.AddChild(new TextElement(new Point(0, 0), FontManager.Get("body"), "Import Texture", Color.White));
        importButton.Pressed += ImportFile;
        _arrayElement.AddChild(importButton);

        // folder dropdown
        var folderList = new DropdownList(); 
        folderList.SetOptionsProvider(FolderOptionsProvider);
        folderList.SelectedEvent += SelectFolder;
        var folderDropdown = new DropdownElement(Point.Zero, new Point(48, 22), folderList, "Folder", Anchor.TopLeft);
        _arrayElement.AddChild(folderDropdown);

        // object name
        var namePanel = new Panel(Point.Zero, new Point(48, 16), Anchor.TopLeft, panelTexture);
        var nameInput = new TextInputBox(new Point(3, 0), new Point(48, 7), FontManager.Get("body"), "Name", Color.Gray, Color.White, TextFormat.SnakeCase, Anchor.MiddleLeft);
        nameInput.ChangeTextEvent += (string value) => { _name = value; };
        namePanel.AddChild(nameInput); 
        _arrayElement.AddChild(namePanel);

        // type dropdown
        var typeList = new DropdownList();
        typeList.SetOptionsProvider(TypeOptionsProvider);
        typeList.SelectedEvent += SelectType;
        var typeDropdown = new DropdownElement(Point.Zero, new Point(48, 22), typeList, "Type", Anchor.TopLeft);
        _arrayElement.AddChild(typeDropdown);

        // material dropdown
        var materialList = new DropdownList();
        materialList.SetOptionsProvider(MaterialOptionsProvider);
        materialList.SelectedEvent += SelectMaterial;
        var materialDropdown = new DropdownElement(Point.Zero, new Point(48, 22), materialList, "Material", Anchor.TopLeft);
        _arrayElement.AddChild(materialDropdown);

        // create button
        var createButton = new ButtonElement(Point.Zero, new Point(74, 16), Anchor.TopLeft, panelTexture, panelSelectedTexture);
        createButton.AddChild(new TextElement(new Point(0, 0), FontManager.Get("body"), "Create", Color.White));
        createButton.Pressed += Create;
        _arrayElement.AddChild(createButton);

        // solid toggle
        var solidToggle = new ToggleElement(Point.Zero, new Point(48, 22), Anchor.TopLeft, "toggle_light_inactive", "toggle_light_active", "panel_selected", "arrow_selector", false);
        solidToggle.AddChild(new TextElement(new Point(3, 3), FontManager.Get("body"), "Solid", Color.Gray, Anchor.TopLeft));
        var valueDisplay = new TextElement(new Point(3, -3), FontManager.Get("body"), "False", Color.White, Anchor.BottomLeft);
        solidToggle.AddChild(valueDisplay);
        solidToggle.ValueChanged += (bool value) =>
        {
            valueDisplay.SetText(value.ToString());
            _solid = value;
        };
        _arrayElement.AddChild(solidToggle);

        _textureDisplay = new ImageElement(new Point(-30, 0), new Point(64, 64), Anchor.MiddleRight, _loadedTexture);
        ActiveCanvas.AddChild(_textureDisplay);
    }

    void SetupNotifcations()
    {
        ArrayElement notificationArray = new ArrayElement(Point.Zero, new Point(10, 0), 1, ArrayDirection.Up, Anchor.BottomCenter);
        ActiveCanvas.AddChild(notificationArray);

        NotificationManager.NotifyEvent += (Notification notification) => 
        {
            int width = (int)FontManager.Get("body").FontBase.MeasureString(notification.Title).X;
            var element = new Panel(Point.Zero, new Point(width + 6, 14), Anchor.BottomCenter, "panel_light");
            element.AddChild(new TextElement(Point.Zero, FontManager.Get("body"), notification.Title, Color.White, Anchor.MiddleCenter));
            _notificationElements.Add(notification, element);
            notificationArray.AddChild(element);
        };

        NotificationManager.RemoveEvent += (Notification notification) =>
        {
            if (_notificationElements.TryGetValue(notification, out var element))
            {
                notificationArray.RemoveChild(element);
                _notificationElements.Remove(notification);
            }
        };
    }

    List<DropdownOption> MaterialOptionsProvider()
    {
        List<DropdownOption> options = new();

        foreach (var material in Enum.GetValues<Material>())
        {
            string name = material.ToString();
            options.Add(new DropdownOption(name, name));
        }

        return options;
    }

    void SelectMaterial(DropdownOption option)
    {
        _material = (string)option.Value;
    }

    List<DropdownOption> FolderOptionsProvider()
    {
        List<DropdownOption> options = new();

        string levelObjectRoot = Path.Combine(ContentRoot, "LevelObjects");
        foreach (string folder in Directory.GetDirectories(levelObjectRoot))
        {
            string folderName = Path.GetFileName(folder);
            options.Add(new DropdownOption(folderName, folderName));
        }

        return options;
    }

    void SelectFolder(DropdownOption option)
    {
        Debug.Log($"Selected folder: {option.Text}");
        _levelObjectPath = (string)option.Value; 
        _folder = (string)option.Value;
    }

    List<DropdownOption> TypeOptionsProvider()
    {
        List<DropdownOption> types = new();

        foreach (var type in LevelObjectTypeLoader.Types)
        {
            types.Add(new DropdownOption(type.Key, type.Key));
        }

        return types;
    }

    void SelectType(DropdownOption option)
    {
        _type = (string)option.Value;
    }

    public void ImportFile()
    {
        var result = Dialog.FileOpen("png");

        if (!result.IsOk)
        {
            Debug.Log("Import File: Not OK.");
            return;
        }

        _loadedTexture = Texture2D.FromFile(Main.Graphics, result.Path);
        _textureDisplay.SetTexture(_loadedTexture);
    }

    public void Create()
    {
        if (_loadedTexture == null)
        {
            NotificationManager.Notify($"Failed to Create. Texture is missing.");
            return;
        }

        var data = new LevelObjectJsonData
        {
            type = _type,
            material = _material,
            solid = _solid
        };

        var jsonPath = Path.Combine(ContentRoot, "LevelObjects", _folder, _name + ".json");
        var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(jsonPath, jsonData);

        var texturePath = Path.Combine(ContentRoot, "LevelObjects", _folder, _name + ".png");
        using var stream = File.OpenWrite(texturePath);
        _loadedTexture.SaveAsPng(stream, _loadedTexture.Width, _loadedTexture.Height);

        Debug.Log("Created");
        NotificationManager.Notify($"Created: '{_name}'.");
    }

    public override void Start()
    {
        base.Start();

        Cursor.SetEnabled(false);
    }

    public override void Close()
    {
        base.Close();

        ComponentSystem.Reset();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Input.Get("back").Pressed)
        {
            Main.SetScene<MainMenu>();
        }

        NotificationManager.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Main.Graphics.Clear(Color.DarkGray);
        base.Draw(spriteBatch);
    }
}