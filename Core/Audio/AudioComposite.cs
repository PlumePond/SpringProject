using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Diagnostics;
using Silk.NET.OpenAL;
using Microsoft.Xna.Framework;

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

    public uint Play(Vector2 position)
    {
        if (AudioClips.Count == 0) return 0;

        AudioClip audioClip = AudioClips[_random.Next(AudioClips.Count)];

        float volume = Math.Clamp(Volume + (float)(_random.NextDouble() * 2 - 1) * VolumeVariance, 0f, 1f);
        float pitch  = Math.Clamp(Pitch  + (float)_random.NextDouble() * PitchVariance, 0.1f, 4f);

        uint source = _al.GenSource();
        AudioManager.SetSourcePosition(source, position);
        AudioManager.GetChannel(_channel)?.ConnectSource(source);
        _al.SetSourceProperty(source, SourceInteger.Buffer, audioClip.Buffer);
        _al.SetSourceProperty(source, SourceFloat.Gain, volume);
        _al.SetSourceProperty(source, SourceFloat.Pitch, pitch);
        _al.SetSourceProperty(source, SourceBoolean.Looping, Loop);
        _al.SourcePlay(source);

        return source;
    }

    public uint Play()
    {
        if (AudioClips.Count == 0) return 0;

        AudioClip audioClip = AudioClips[_random.Next(AudioClips.Count)];

        float volume = Math.Clamp(Volume + (float)(_random.NextDouble() * 2 - 1) * VolumeVariance, 0f, 1f);
        float pitch  = Math.Clamp(Pitch  + (float)_random.NextDouble() * PitchVariance, 0.1f, 4f);

        uint source = _al.GenSource();
        
        // get listener position and play at that location for centered audio
        _al.GetListenerProperty(ListenerVector3.Position, out float lx, out float ly, out float lz);
        Vector2 listenerPos = new Vector2(lx / AudioManager.AudioScale, ly / AudioManager.AudioScale);
        AudioManager.SetSourcePosition(source, listenerPos);
        
        AudioManager.GetChannel(_channel)?.ConnectSource(source);
        _al.SetSourceProperty(source, SourceInteger.Buffer, audioClip.Buffer);
        _al.SetSourceProperty(source, SourceFloat.Gain, volume);
        _al.SetSourceProperty(source, SourceFloat.Pitch, pitch);
        _al.SetSourceProperty(source, SourceBoolean.Looping, Loop);
        _al.SourcePlay(source);

        return source;
    }

    public void ReloadBuffers()
    {
        foreach (var clip in AudioClips)
        {
            clip.ReloadBuffers();
        }
    }
}