using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpringProject.Core.Content;
using SpringProject.Core.Debugging;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.UI;

public class InfoPanel : Panel
{
    static InfoPanel _instance;
    static Dictionary<string, Element> _elements = new Dictionary<string, Element>();

    public InfoPanel(Point position, Point size, Anchor anchor, string textureName, int cornerSize = 3) : base(position, size, anchor, textureName, cornerSize)
    {
    }

    public override void OnEnable()
    {
        Debug.Log("ENABLED!");
        _instance = this;
    }
    
    /// <summary>
    /// Adds an Element as a child to the Info Panel.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="element"></param>
    /// <exception cref="Exception"></exception>
    public static void AddElement(string name, Element element)
    {
        if (_elements.ContainsKey(name))
        {
            throw new Exception($"Element '{name}' is already present in the dictionary!");
        }
        else
        {
            _instance.AddChild(element);
            _elements.Add(name, element);
        }
    }

    public static void RemoveElement(string name)
    {
        if (_elements.TryGetValue(name, out Element element))
        {
            _instance.Children.Remove(element);
        }
        else
        {
            throw new Exception($"Element '{name}' is not present in the dictionary!");
        }
    }

    public static bool HasElement(string name)
    {
        return _elements.ContainsKey(name);
    }

    public static bool TryGetElement<T>(string name, out T element) where T : Element
    {
        if (_elements.TryGetValue(name, out Element value))
        {
            if (value is not T)
            {
                throw new Exception($"Info Panel: Element '{name}' is not a {typeof(T).Name}. It is a {value.GetType().Name}!");
            }

            element = (T)value;
            return true;
        }
        else
        {
            element = null;
            return false;
        }
    }

    public static void ClearElements()
    {
        _elements.Clear();
        _instance.ClearChildren();
    }
}