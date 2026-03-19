

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Audio;
using SpringProject.Core.Audio;
using SpringProject.Settings;

public static class SettingsLoader
{
    public static SettingsData Load(string file)
    {
        if (!File.Exists(file))
        {
            Debug.WriteLine($"Settings file '{file}' not found. Creating default settings.");
            var defaultSettings = new SettingsData();
            Save(file, defaultSettings);
            return defaultSettings;
        }

        string json = File.ReadAllText(file);
        return JsonSerializer.Deserialize<SettingsData>(json);
    }

    public static void Save(string file, SettingsData settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(file, json);
    }
}