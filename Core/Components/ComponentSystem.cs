using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Components;

public class ComponentSystem
{
    public static readonly List<Component> components = new List<Component>();

    public static void Add(Component component)
    {
        components.Add(component);
    }

    public static void Remove(Component component)
    {
        components.Remove(component);
    }

    public static void Reset()
    {
        foreach (Component component in components.ToList())
        {
            component.OnDestroy();
        }
        components.Clear();
    }

    public static void Update(GameTime gameTime)
    {
        foreach (var component in components.ToList())
        {
            if (!component.Enabled) continue;
            component.Update(gameTime);
        }
    }

    public static void FixedUpdate(GameTime gameTime)
    {
        foreach (var component in components.ToList())
        {
            if (!component.Enabled) continue;
            component.FixedUpdate(gameTime);
        }
    }

    public static void EditorUpdate(GameTime gameTime)
    {
        foreach (var component in components.ToList())
        {
            if (!component.Enabled) continue;
            component.EditorUpdate(gameTime);
        }
    }
}