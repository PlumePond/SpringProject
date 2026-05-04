using System;
using Microsoft.Xna.Framework;
using Silk.NET.OpenAL;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Audio;

public class AudioSource
{
    public uint Id { get; private set; }
    public bool isPlaying { get; private set; }
    public AudioChannel Channel { get; private set; }

    AudioClip _audioClip;
    bool _sourceRelative;
    float _pitch;
    float _gain;
    bool _looping;
    Vector2 _position;
    float _referenceDistance;
    float _maxDistance;
    float _rolloffFactor;
    float _offset;

    AL _al;

    public AudioSource()
    {
        _al = AudioManager.GetAL();

        Id = _al.GenSource();
        SetSourceRelative(true);
    }

    public void SetAudioClip(AudioClip audioClip)
    {
        _al.SetSourceProperty(Id, SourceInteger.Buffer, audioClip.Buffer);
        _audioClip = audioClip;
    }

    public void SetChannel(AudioChannel channel)
    {
        channel?.ConnectSource(this);
        Channel = channel;
    }

    public void SetSourceRelative(bool sourceRelative)
    {
        _al.SetSourceProperty(Id, SourceBoolean.SourceRelative, sourceRelative);
        _sourceRelative = sourceRelative;
    }

    public void SetPitch(float pitch)
    {
        _al.SetSourceProperty(Id, SourceFloat.Pitch, pitch);
        _pitch = pitch;
    }

    public void SetGain(float gain)
    {
        _al.SetSourceProperty(Id, SourceFloat.Gain, gain);
        _gain = gain;
    }

    public void SetLooping(bool looping)
    {
        _al.SetSourceProperty(Id, SourceBoolean.Looping, looping);
        _looping = looping;
    }

    public void SetPosition(Vector2 position)
    {
        AudioManager.SetSourcePosition(Id, position);
        SetSourceRelative(false);
        _position = position;
    }

    public void SetReferenceDistance(float referenceDistance)
    {
        _al.SetSourceProperty(Id, SourceFloat.ReferenceDistance, referenceDistance);
        _referenceDistance = referenceDistance;
    }

    public void SetMaxDistance(float maxDistance)
    {
        _al.SetSourceProperty(Id, SourceFloat.MaxDistance, maxDistance);
        _maxDistance = maxDistance;
    }

    public void SetRolloffFactor(float rolloffFactor)
    {
        _al.SetSourceProperty(Id, SourceFloat.RolloffFactor, rolloffFactor);
        _rolloffFactor = rolloffFactor;
    }

    public void Play()
    {
        _al.SourcePlay(Id);
        isPlaying = true;

        if (!AudioManager.HasSource(this))
        {
            AudioManager.AddSource(this);
        }
    }

    public void Pause()
    {
        _al.SourcePause(Id);
        isPlaying = false;
    }

    public void Stop()
    {
        _al.SourceStop(Id);
        isPlaying = false;
    }

    public void PreReload()
    {
        Debug.Log("Prereload!");
        _al.GetSourceProperty(Id, SourceFloat.SecOffset, out _offset);
    }

    public void Reload()
    {
        _al = AudioManager.GetAL();
        Id = _al.GenSource();

        _al.SetSourceProperty(Id, SourceInteger.Buffer, _audioClip.Buffer);
        Channel?.ConnectSource(this);
        _al.SetSourceProperty(Id, SourceBoolean.SourceRelative, _sourceRelative);
        _al.SetSourceProperty(Id, SourceFloat.Pitch, _pitch);
        _al.SetSourceProperty(Id, SourceFloat.Gain, _gain);
        _al.SetSourceProperty(Id, SourceBoolean.Looping, _looping);
        AudioManager.SetSourcePosition(Id, _position);
        _al.SetSourceProperty(Id, SourceFloat.ReferenceDistance, _referenceDistance);
        _al.SetSourceProperty(Id, SourceFloat.MaxDistance, _maxDistance);
        _al.SetSourceProperty(Id, SourceFloat.RolloffFactor, _rolloffFactor);

        if (isPlaying)
        {
            _al.SourcePlay(Id);
            _al.SetSourceProperty(Id, SourceFloat.SecOffset, _offset);
        }
    }
}