using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace SpringProject.Core.UI;

public class DropdownList(Func<List<DropdownOption>> optionsProvider)
{
    public readonly Func<List<DropdownOption>> OptionsProvider = optionsProvider;
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

    public DropdownElement(Point localPosition, Point size, Anchor anchor = Anchor.MiddleCenter) : base(localPosition, size, anchor)
    {
        _textElement = new TextElement(Point.Zero, FontManager.Get("body"), "None", Color.White, Anchor.MiddleCenter);
        AddChild(_textElement);
        
        _arrayElement = new ArrayElement(new Point(0, size.Y), size, 0, ArrayDirection.Down, Anchor.TopLeft);
        AddChild(_arrayElement);
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
    }

    public void Close()
    {
        _open = false;
        
        _arrayElement.Children.Clear();

        foreach (var child in _arrayElement.Children)
        {
            if (child is ButtonElement button)
            {
                button.Reset();
            }
        }
    }

    void SelectOption(DropdownOption option)
    {
        SelectedOption = option;
        _textElement.SetText(option.Text);
        Close();
    }
}