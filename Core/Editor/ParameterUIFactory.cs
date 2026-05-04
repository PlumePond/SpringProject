using System;
using Microsoft.Xna.Framework;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;

namespace SpringProject.Core.Editor;

public static class ParameterUIFactory
{
    static string _sliderTexture;
    static string _panelTexture;
    static string _selectedTexture;
    static string _fillTexture;
    static Point _sliderSize;
    static Point _handleSize;

    public static void Configure(string sliderTexture, string panelTexture, string selectedTexture, string fillTexture, Point sliderSize, Point handleSize)
    {
        _sliderTexture = sliderTexture;
        _panelTexture = panelTexture;
        _selectedTexture = selectedTexture;
        _fillTexture = fillTexture;
        _sliderSize = sliderSize;
        _handleSize = handleSize;
    }
    
    public static Element Create(ParameterDescriptor parameter)
    {
        var panel = new Panel(Point.Zero, new Point(InfoPanel.ScrollRect.size.X - 4, 16), Anchor.TopCenter, "object_slot");
        InfoPanel.ArrayElement.UpdateSizeEvent += (Point newSize) =>
        {
            panel.SetSize(new Point(InfoPanel.ScrollRect.size.X - 8, 16));
        }; 

        switch (parameter.ValueType)
        {
            case Type t when t == typeof(float): return CreateFloat(parameter, panel);
            case Type t when t == typeof(int): return CreateInt(parameter, panel);
            case Type t when t == typeof(string): return CreateString(parameter, panel);
            case Type t when t == typeof(DropdownList): return CreateDropdown(parameter, panel);
            default: return null;
        }
    }

    static Element CreateFloat(ParameterDescriptor parameter, Panel panel)
    {
        var slider = new Slider(Point.Zero, _sliderSize, _handleSize, Anchor.TopLeft, _sliderTexture, _panelTexture, _selectedTexture, _fillTexture, parameter.Min, parameter.Max, (float)parameter.GetValue());
        panel.AddChild(slider);
        slider.ChangeValue += (float value) => parameter.SetValue(value);
        var valueText = new TextElement(new Point(slider.size.X + 3, 0), FontManager.Get("body"), ((float)parameter.GetValue()).ToString("F2"), Color.White * 0.5f, Anchor.MiddleLeft);
        slider.ChangeValue += (float value) => valueText.SetText(value.ToString("F2"));
        slider.AddChild(valueText);

        slider.AddChild(new TextElement(new Point(0, 9), FontManager.Get("body"), parameter.Label, Color.DarkGray, Anchor.TopLeft));

        return panel;
    }

    static Element CreateInt(ParameterDescriptor parameter, Panel panel)
    {
        var slider = new Slider(Point.Zero, _sliderSize, _handleSize, Anchor.TopLeft, _sliderTexture, _panelTexture, _selectedTexture, _fillTexture, parameter.Min, parameter.Max, (int)parameter.GetValue());
        panel.AddChild(slider);
        slider.ChangeValue += (float value) => parameter.SetValue((int)value);
        var valueText = new TextElement(new Point(slider.size.X + 3, 0), FontManager.Get("body"), ((int)parameter.GetValue()).ToString(), Color.White * 0.5f, Anchor.MiddleLeft);
        slider.ChangeValue += (float value) => valueText.SetText(((int)value).ToString());
        slider.AddChild(valueText);

        slider.AddChild(new TextElement(new Point(0, 9), FontManager.Get("body"), parameter.Label, Color.DarkGray, Anchor.TopLeft));

        return panel;
    }

    static Element CreateString(ParameterDescriptor parameter, Panel panel)
    {
        var panelBox = new Panel(Point.Zero, _sliderSize, Anchor.MiddleCenter, "text_box");
        panel.AddChild(panelBox);
        var textBox = new TextInputBox(Point.Zero, _sliderSize, FontManager.Get("body"), parameter.Label, Color.DarkGray, Color.White, Anchor.TopLeft);
        textBox.ChangeTextEvent += (string value) => parameter.SetValue((string)value);
        textBox.SetText((string)parameter.GetValue());
        panelBox.AddChild(textBox);
        return panel;
    }

    static Element CreateDropdown(ParameterDescriptor parameter, Panel panel)
    {
        var dropdownList = (DropdownList)parameter.GetValue();
        var dropdown = new DropdownElement(Point.Zero, _sliderSize, dropdownList, Anchor.TopLeft);

        Debug.Log("Creating dropdown!");
        
        if (dropdownList?.OptionsProvider != null)
        {
            var options = dropdownList.OptionsProvider();

            Debug.Log($"Dropdown option count: {options.Count}");

            foreach (var option in options)
            {
                dropdown.AddOption(option);
                Debug.Log($"Added dropdown option '{option.Text}'");
            }
        }

        dropdown.SelectFromKey(dropdownList.SelectedKey);
        
        panel.AddChild(dropdown);
        
        return panel;
    }
}