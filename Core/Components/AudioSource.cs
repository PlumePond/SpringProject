using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Silk.NET.OpenAL;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;
using Silk.NET.OpenAL.Extensions.EXT;
using Silk.NET.OpenAL.Extensions.Creative;

namespace SpringProject.Core.Components;

public class AudioSource : Component
{
    readonly AL _al;

    uint _source;
    string _soundName = "";
    bool _playing = false;
    string _channel = "";
    
    [Parameter("Sound")] public string Sound
    {
        get => _soundName;
        set
        {
            if (_soundName != value)
            {
                SetSound(value);
            }
        }
    }

    [Parameter("Volume", 0f, 1f)]
    public float Volume
    {
        get => _volume;
        set { _volume = value; if (_source != 0) _al.SetSourceProperty(_source, SourceFloat.Gain, _volume); }
    }
    float _volume = 1.0f;

    [Parameter("Pitch", 0.1f, 4f)]
    public float Pitch
    {
        get => _pitch;
        set { _pitch = value; if (_source != 0) _al.SetSourceProperty(_source, SourceFloat.Pitch, _pitch); }
    }
    float _pitch = 1.0f;

    public bool Loop { get; set; } = true;

    [Parameter("Reference Distance", 0f, 100f)] public float RefDistance { get; set; } = 8f;
    [Parameter("Max Distance", 0f, 200f)] public float MaxDistance { get; set; } = 64f;
    [Parameter("Rolloff", 0f, 10f)] public float Rolloff { get; set; } = 1f;

    [Parameter("Channel")] 
    public string Channel
    {
        get => _channel;
        set
        {
            if (_channel != value)
            {
                _channel = value;
                ConnectToChannel();
            }
        }
    }

    uint _reverbSlot;
    uint _reverbEffect;

    public AudioSource()
    {
        _al = AudioManager.GetAL();
    }

    public override void Start()
    {
        _source = _al.GenSource();
        _al.SetSourceProperty(_source, SourceBoolean.Looping, Loop);
        _al.SetSourceProperty(_source, SourceFloat.Gain, Volume);
        _al.SetSourceProperty(_source, SourceFloat.Pitch, Pitch);
        _al.SetSourceProperty(_source, SourceBoolean.SourceRelative, false); // world space
        _al.SetSourceProperty(_source, SourceFloat.ReferenceDistance, 1f);
        _al.SetSourceProperty(_source, SourceFloat.MaxDistance, 16f);
        _al.SetSourceProperty(_source, SourceFloat.RolloffFactor, 1f);

        ConnectToChannel();
    }

    void ConnectToChannel()
    {
        //Debug.Log($"AudioSource: Connecting to channel '{Channel}'");

        if (!string.IsNullOrEmpty(_channel))
        {
            AudioManager.GetChannel(_channel)?.DisconnectSource(_source);
        }

        if (!string.IsNullOrEmpty(Channel))
        {
            AudioManager.GetChannel(Channel)?.ConnectSource(_source);
        }
    }

    void SetSound(string soundName)
    {
        _soundName = soundName;

        var composite = AudioManager.Get(soundName);
        if (composite == null || composite.AudioClips.Count == 0) return;

        // Stop current sound if playing
        if (_playing)
        {
            _al.SourceStop(_source);
            _playing = false;
        }

        // For a looping source, just use the first buffer —
        // random variation on loop restarts doesn't make sense here
        AudioClip audioClip = composite.AudioClips[0];
        _al.SetSourceProperty(_source, SourceInteger.Buffer, (int)audioClip.Buffer);

        // Use the AudioSource's own Loop property (can be set in editor), not the composite's
        _al.SetSourceProperty(_source, SourceBoolean.Looping, Loop);

        // Re-apply spatial properties after buffer bind
        _al.SetSourceProperty(_source, SourceBoolean.SourceRelative, false);
        _al.SetSourceProperty(_source, SourceFloat.ReferenceDistance, 1f);
        _al.SetSourceProperty(_source, SourceFloat.MaxDistance, 16f);
        _al.SetSourceProperty(_source, SourceFloat.RolloffFactor, 1f);

        // Start playing the new sound
        _al.SourcePlay(_source);
        _playing = true;
    }

    public void Play()
    {
        if (_source == 0 || _playing) return;

        // Sync position before playing so the first frame isn't heard from origin
        if (LevelObject != null)
        {
            var pos = LevelObject.hitbox.Center.ToVector2();
            AudioManager.SetSourcePosition(_source, pos);
        }

        _al.SourcePlay(_source);
        _playing = true;
    }

    public void Stop()
    {
        if (_source == 0 || !_playing) return;
        _al.SourceStop(_source);
        _playing = false;
    }

    public override void Update(GameTime gameTime)
    {
        UpdateProperties();
        SyncChannel();
    }

    public override void EditorUpdate(GameTime gameTime)
    {
        UpdateProperties();
        SyncChannel();
    }

    void SyncChannel()
    {
        // Check if channel property changed and needs to be reconnected
        if (_channel != Channel)
        {
            ConnectToChannel();
        }
    }

    void UpdateProperties()
    {
        if (_source == 0 || LevelObject == null) return;

        var pos = LevelObject.transform.position.ToVector2();
        _al.SetSourceProperty(_source, SourceVector3.Position, pos.X * AudioManager.AudioScale, pos.Y * AudioManager.AudioScale, 0f);

        // Get listener position for manual attenuation
        _al.GetListenerProperty(ListenerVector3.Position, out float lx, out float ly, out float lz);
        Vector2 listenerPos = new Vector2(lx / AudioManager.AudioScale, ly / AudioManager.AudioScale);

        // Calculate manual attenuation
        float attenuation = AudioManager.CalculateAttenuation(pos, listenerPos, RefDistance, MaxDistance * 16, Rolloff);
        _al.SetSourceProperty(_source, SourceFloat.Gain, Volume * attenuation);
    }

    public override void OnDestroy()
    {
        Stop();

        if (!string.IsNullOrEmpty(_channel))
        {
            AudioManager.GetChannel(_channel)?.DisconnectSource(_source);
        }

        if (_source != 0)
        {
            _al.DeleteSource(_source);
            _source = 0;
        }

        base.OnDestroy();
    }
}