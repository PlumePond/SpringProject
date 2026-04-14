using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Editor;

public static class ColorManager
{
    public static readonly List<Color> Colors = new List<Color>();
    public static int SelectedColorIndex { get; private set; } = 0;
    public static Color SelectedColor => Get(SelectedColorIndex);

    public static Action<List<Color>> ColorListModifiedEvent;

    const int SLOTS = 15;

    public static void SetColorIndex(int index)
    {
        SelectedColorIndex = index;
    }

    /// <summary>
    /// Get the color based on its index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static Color Get(int index)
    {
        if (index < Colors.Count)
        {
            return Colors[index];
        }
        else
        {
            return Color.White;
        }
    }

    public static void Add(Color color)
    {
        Colors.Add(color);
        ColorListModifiedEvent?.Invoke(Colors);
    }

    public static void Set(int index, Color color)
    {
        if (index >= 0 && index < Colors.Count)
        {
            Colors[index] = color;
        }
        else
        {
            throw new Exception($"ColorManager: Color {index} does not exist!");
        }
    }

    public static void Load(Color[] colors)
    {
        Colors.Clear();

        if (colors == null)
        {
            for (int i = 0; i < SLOTS; i++)
            {
                Add(Color.White);
            }
        }
        else if (colors.Length < 1)
        {
            for (int i = 0; i < SLOTS; i++)
            {
                Add(Color.White);
            }
        }
        else
        {
            foreach (var color in colors)
            {
                Add(color);
            }
        }

        Debug.Log($"Colors loaded! {Colors.Count}");
    }
}