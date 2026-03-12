using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Diagnostics;

namespace SpringProject.Core.Audio;

public class AudioComposite
{
    static readonly Random _random = new Random();

    public string Name { get; set; }
    public List<SoundEffect> Sounds { get; set; } = new List<SoundEffect>();
    public float Volume { get; set; } = 1.0f;
    public float Pitch { get; set; } = 0.0f;
    public float VolumeVariance { get; set; } = 0.0f;
    public float PitchVariance { get; set; } = 0.0f;
    public bool Loop { get; set; } = false;

    public AudioComposite(string name, List<SoundEffect> sounds, float volume, float pitch, float volumeVariance, float pitchVariance, bool loop)
    {
        Name = name;
        Sounds = sounds;
        Volume = volume;
        Pitch = pitch;
        VolumeVariance = volumeVariance;
        PitchVariance = pitchVariance;
        Loop = loop;
    }

    public void Play()
    {
        if (Sounds.Count > 0)
        {
            SoundEffect sound = Sounds[_random.Next(Sounds.Count)];

            float volume = Math.Clamp(Volume + (float)(_random.NextDouble() * 2 - 1) * VolumeVariance, 0f, 1f);
            float pitch  = Math.Clamp(Pitch  + (float)(_random.NextDouble() * 2 - 1) * PitchVariance, -1f, 1f);

            sound.Play(volume, pitch, 0f);
        }
    }
}