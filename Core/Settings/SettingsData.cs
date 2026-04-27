using System.Text.Json.Serialization;

namespace SpringProject.Settings;

public class SettingsData
{
        [JsonPropertyName("ui_size")]  public int UISize { get; set; } = 2;
        [JsonPropertyName("vsync")] public bool VSync { get; set; } = true;
}