using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.UI;

public class DropdownList
{
    public string SelectedKey { get; set; } = null; // serialized by Newtonsoft

    public void Select(string key)
    {
        SelectedKey = key;
    }

    [JsonIgnore] public Func<List<DropdownOption>> OptionsProvider { get; private set; }
    [JsonIgnore] public Action<DropdownOption> SelectedEvent { get; set; }

    public void SetOptionsProvider(Func<List<DropdownOption>> provider)
    {
        Debug.Log("Options provider set!");
        OptionsProvider = provider;
    }

    public DropdownList()
    {
        OptionsProvider = () => new List<DropdownOption>();
    }

    public DropdownList(Func<List<DropdownOption>> optionsProvider)
    {
        OptionsProvider = optionsProvider;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        // After deserialization, restore the OptionsProvider to a default or Audio-based provider
        // We'll set it to look up from AudioManager if available
        Debug.Log("DROPDOWN ELEMENT DESERIALIZED!");
        OptionsProvider ??= () => new List<DropdownOption>();
    }
}

public class DropdownOption(string text, object value)
{
    public string Text { get; set; } = text;
    public object Value { get; set; } = value;
}

public class DropdownElement : Element
{
    List<DropdownOption> _options = new();
    bool _open = false;

    public DropdownOption SelectedOption { get; private set; } = null;
    TextElement _textElement;
    ArrayElement _arrayElement;

    DropdownList _list;

    public DropdownElement(Point localPosition, Point size, DropdownList list, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, anchor)
    {
        _list = list;
        _textElement = new TextElement(Point.Zero, FontManager.Get("body"), "None", Color.White, Anchor.MiddleCenter);
        AddChild(_textElement);
        
        _arrayElement = new ArrayElement(new Point(0, size.Y), size, 0, ArrayDirection.Down, Anchor.TopLeft);
        AddChild(_arrayElement);

        // if (_list.SelectedKey != null)
        // {
        //     _textElement.SetText(_list.SelectedKey);
        // }
    }

    public void SelectFromKey(string key)
    {
        foreach (var option in _options)
        {
            if (option.Text.Equals(key))
            {
                SelectedOption = option;
                _list.Select(option.Text); // persist to the DropdownList
                _textElement.SetText(option.Text);    
                _list.SelectedEvent?.Invoke(option);
                return;
            }
        }
    }

    void SelectOption(DropdownOption option)
    {
        SelectedOption = option;
        _list.Select(option.Text); // persist to the DropdownList
        _textElement.SetText(option.Text);
        _list.SelectedEvent?.Invoke(option);
        Close();
    }

    public void AddOption(DropdownOption option)
    {
        _options.Add(option);
    }

    public void ClearOptions()
    {
        _options.Clear();
    }

    public override void OnPressed()
    {
        if (_open)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        _open = true;

        foreach (var option in _options)
        {
            var optionElement = new ButtonElement(Point.Zero, size, Anchor.TopLeft, "panel_mid", "panel_selected_small");
            optionElement.AddChild(new TextElement(Point.Zero, FontManager.Get("body"), option.Text, Color.White, Anchor.MiddleCenter));

            optionElement.Pressed += () => SelectOption(option);

            _arrayElement.AddChild(optionElement);
        }

        // restore display from saved state
        if (_list.SelectedKey != null)
        {
            SelectFromKey(_list.SelectedKey);
        }
    }

    public void Close()
    {
        _open = false;
        
        foreach (var child in _arrayElement.Children)
        {
            if (child is ButtonElement button)
            {
                button.Reset();
            }
        }
        
        _arrayElement.Children.Clear();
    }
}