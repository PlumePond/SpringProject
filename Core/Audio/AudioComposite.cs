using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System;
using Silk.NET.OpenAL;
using Microsoft.Xna.Framework;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Audio;

public class AudioComposite
{
    static readonly Random _random = new Random();

    public string Name { get; set; }
    public List<AudioClip> AudioClips { get; set; } = new ();
    public float Volume { get; set; } = 1.0f;
    public float Pitch { get; set; } = 1.0f;
    public float VolumeVariance { get; set; } = 0.0f;
    public float PitchVariance { get; set; } = 0.0f;
    public bool Loop { get; set; } = false;

    readonly AL _al;

    string _channel = "";

    public AudioComposite(string name, float volume, float pitch, float volumeVariance, float pitchVariance, bool loop, AL al)
    {
        Name = name;
        Volume = volume;
        Pitch = pitch;
        VolumeVariance = volumeVariance;
        PitchVariance = pitchVariance;
        Loop = loop;
        _al = al;
    }

    public void SetChannel(string channel)
    {
        _channel = channel;
    }

    public AudioSource Play(Vector2? position = null)
    {
        if (AudioClips.Count == 0) return null;

        AudioClip audioClip = AudioClips[_random.Next(AudioClips.Count)];

        float volume = Math.Clamp(Volume + (float)(_random.NextDouble() * 2 - 1) * VolumeVariance, 0f, 1f);
        float pitch  = Math.Clamp(Pitch  + (float)_random.NextDouble() * PitchVariance, 0.1f, 4f);

        var source = new AudioSource();
        source.SetChannel(AudioManager.GetChannel(_channel));
        source.SetAudioClip(audioClip);

        if (position != null)
        {
            source.SetPosition(position.Value);
        }
        
        source.SetGain(volume);
        source.SetPitch(pitch);
        source.Play();

        return source;
    }

    public void Reload()
    {
        foreach (var clip in AudioClips)
        {
            clip.Reload();
        }
    }
}