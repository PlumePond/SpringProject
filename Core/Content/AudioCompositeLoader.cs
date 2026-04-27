

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Audio;
using SpringProject.Core;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;
using Silk.NET.OpenAL;
using System;

public static class AudioCompositeLoader
{
    public static Dictionary<string, AudioComposite> Load(string folder)
    {
        var dictionary = new Dictionary<string, AudioComposite>();
        var al = AudioManager.GetAL();

        foreach (string jsonFile in Directory.GetFiles(folder, "*.json"))
        {
            string name = Path.GetFileNameWithoutExtension(jsonFile);
            string json = File.ReadAllText(jsonFile);
            AudioCompositeData data = JsonSerializer.Deserialize<AudioCompositeData>(json);

            var composite = new AudioComposite(
                name,
                data.Volume,
                data.Pitch,
                data.Variance?.Volume ?? 0.0f,
                data.Variance?.Pitch ?? 0.0f,
                data.Loop,
                al
            );

            // find each .wav file that has the same name as the json file, (e.g. "footstep-1.wav", "footstep-2.wav", etc.)
            // or if there is only one .wav file with the same name as the json file (e.g. "footstep.wav"), load that one
            foreach (string wavFile in Directory.GetFiles(folder, $"{name}*.wav"))
            {
                LoadWavToBuffer(al, wavFile, out var buffer, out var format, out var sampleRate, out var pcmData);
                AudioClip audioClip = new AudioClip(Path.GetFileNameWithoutExtension(wavFile), buffer, pcmData, sampleRate, format);
                composite.AudioClips.Add(audioClip);
            }

            dictionary[name] = composite;
        }

        Debug.Log($"Audio Composites loaded! ({dictionary.Count}).");

        return dictionary;
    }

    /// <summary>
    /// Parses a WAV file and uploads its PCM data to an OpenAL buffer
    /// Supports 8-bit, 16-bi, mono, and stereo
    /// </summary>
    static void LoadWavToBuffer(AL al, string path, out uint buffer, out BufferFormat format, out int sampleRate, out byte[] pcmData)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        // riff header
        reader.ReadBytes(4); // "riff"
        reader.ReadInt32(); // chunk size
        reader.ReadBytes(4); // "wave"

        // fmt sub-chunk
        reader.ReadBytes(4); // "fmt"
        int fmtSize = reader.ReadInt32();
        short audioFmt = reader.ReadInt16(); // 1 = PCM
        short channels = reader.ReadInt16();
        sampleRate = reader.ReadInt32();
        reader.ReadInt32(); // byte rate
        reader.ReadInt16(); // block align
        short bitsPerSample = reader.ReadInt16();

        if (fmtSize > 16)
        {
            reader.ReadBytes(fmtSize - 16); // extra bytes
        }

        // skip non-data chunks (e.g. LIST, INFO)
        string chunkId = new string(reader.ReadChars(4));
        int chunkSize = reader.ReadInt32();
        while (chunkId != "data")
        {
            reader.ReadBytes(chunkSize);
            chunkId = new string(reader.ReadChars(4));
            chunkSize = reader.ReadInt32();
        }

        pcmData = reader.ReadBytes(chunkSize);

        format = (channels, bitsPerSample) switch
        {
            (1, 8) => BufferFormat.Mono8,
            (1, 16) => BufferFormat.Mono16,
            (2, 8) => BufferFormat.Stereo8,
            (2, 16) => BufferFormat.Stereo16,
            _ => throw new NotSupportedException($"Unsupported WAV format: {channels}ch {bitsPerSample}bit")
        };

        buffer = al.GenBuffer();
        al.BufferData(buffer, format, pcmData, sampleRate);
    }

    // private DTOs for deserialization
    class AudioCompositeData
    {
        [JsonPropertyName("volume")]  public float Volume { get; set; } = 1.0f;
        [JsonPropertyName("pitch")]   public float Pitch  { get; set; } = 0.0f;
        [JsonPropertyName("loop")]    public bool  Loop   { get; set; } = false;
        [JsonPropertyName("variance")] public VarianceData? Variance { get; set; }
    }

    class VarianceData
    {
        [JsonPropertyName("volume")] public float Volume { get; set; } = 0.0f;
        [JsonPropertyName("pitch")]  public float Pitch  { get; set; } = 0.0f;
    }
}