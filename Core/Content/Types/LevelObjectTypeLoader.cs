using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Content.Types;

public static class LevelObjectTypeLoader
{
    //static string path = "Data/Types/LevelObjects";

    public static Dictionary<string, Type> Types { get; private set; } = new Dictionary<string, Type>();

    // load level object types
    public static void Load()
    {
        var baseType = typeof(LevelObject);
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
            {
                var key = StringUtils.ToSnakeCase(type.Name);
                Types[key] = type;

                Debug.Log($"Level object type loaded: '{key}'");
            }
        }
    }
}