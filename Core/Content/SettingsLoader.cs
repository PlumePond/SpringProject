

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Audio;
using SpringProject.Core;
using SpringProject.Core.Audio;
using SpringProject.Settings;
using SpringProject.Core.Debugging;

public static class SettingsLoader
{
    static SettingsLoader()
    {
        RuntimeReloader.FileChangedEvent += OnFileChanged;
    }

    public static SettingsData Load(string file)
    {
        if (!File.Exists(file))
        {
            Debug.Log($"Settings file '{file}' not found. Creating default settings.");
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

    static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Debug.Log($"Settings Loader: file changed: {e.Name}");
    }
}