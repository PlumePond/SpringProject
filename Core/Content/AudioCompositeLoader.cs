

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Audio;
using SpringProject.Core.Audio;

public static class AudioCompositeLoader
{
    public static Dictionary<string, AudioComposite> LoadAll(string folder)
    {
        var result = new Dictionary<string, AudioComposite>();

        foreach (string jsonFile in Directory.GetFiles(folder, "*.json"))
        {
            string name = Path.GetFileNameWithoutExtension(jsonFile);
            string json = File.ReadAllText(jsonFile);

            AudioCompositeData data = JsonSerializer.Deserialize<AudioCompositeData>(json);

            var composite = new AudioComposite(
                name,
                new List<SoundEffect>(),
                data.Volume,
                data.Pitch,
                data.Variance?.Volume ?? 0.0f,
                data.Variance?.Pitch ?? 0.0f,
                data.Loop
            );

            // find each .wav file that has the same name as the json file, (e.g. "footstep-1.wav", "footstep-2.wav", etc.)
            // or if there is only one .wav file with the same name as the json file (e.g. "footstep.wav"), load that one
            foreach (string wavFile in Directory.GetFiles(folder, $"{name}*.wav"))
            {
                var memStream = new MemoryStream(File.ReadAllBytes(wavFile));
                composite.Sounds.Add(SoundEffect.FromStream(memStream));
            }

            result[name] = composite;
        }

        return result;
    }

    // Private DTOs for deserialization
    private class AudioCompositeData
    {
        [JsonPropertyName("volume")]  public float Volume { get; set; } = 1.0f;
        [JsonPropertyName("pitch")]   public float Pitch  { get; set; } = 0.0f;
        [JsonPropertyName("loop")]    public bool  Loop   { get; set; } = false;
        [JsonPropertyName("variance")] public VarianceData? Variance { get; set; }
    }

    private class VarianceData
    {
        [JsonPropertyName("volume")] public float Volume { get; set; } = 0.0f;
        [JsonPropertyName("pitch")]  public float Pitch  { get; set; } = 0.0f;
    }
}