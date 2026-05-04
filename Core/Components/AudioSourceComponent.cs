using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Silk.NET.OpenAL;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;
using Silk.NET.OpenAL.Extensions.EXT;
using Silk.NET.OpenAL.Extensions.Creative;

namespace SpringProject.Core.Components;

public class AudioSourceComponent : Component
{
    readonly AL _al;

    AudioSource _source;
    string _soundName = "";
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
        set { _volume = value; _source?.SetGain(_volume); }
    }
    float _volume = 1.0f;

    [Parameter("Pitch", 0.1f, 4f)]
    public float Pitch
    {
        get => _pitch;
        set { _pitch = value; _source?.SetPitch(_pitch); }
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

    public AudioSourceComponent()
    {
        _al = AudioManager.GetAL();
    }

    public override void Start()
    {
        _source = new AudioSource();

        _source.SetLooping(Loop);
        _source.SetGain(Volume);
        _source.SetPitch(Pitch);
        _source.SetReferenceDistance(1f);
        _source.SetMaxDistance(16f);
        _source.SetRolloffFactor(1f);

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

        // stop current sound if playing
        if (_source.isPlaying)
        {
            _source.Stop();
        }

        // For a looping source, just use the first buffer —
        // random variation on loop restarts doesn't make sense here
        AudioClip audioClip = composite.AudioClips[0];
        _source.SetAudioClip(audioClip);
        _source.SetLooping(Loop);

        // re-apply spatial properties after buffer bind
        _source.SetSourceRelative(false);
        _source.SetReferenceDistance(1f);
        _source.SetMaxDistance(16f);
        _source.SetRolloffFactor(1f);

        // start playing the new sound
        _source.Play();
    }

    public void Play()
    {
        if (_source == null) return;
        if (_source.isPlaying) return;

        // sync position before playing so the first frame isn't heard from origin
        if (LevelObject != null)
        {
            var pos = LevelObject.hitbox.Center.ToVector2();
            _source.SetPosition(pos);
        }

        _source.Play();
    }

    public void Stop()
    {
        if (_source == null) return;
        if (!_source.isPlaying) return;

        _source.Stop();
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
        if (_source == null) return;
        if (LevelObject == null) return;

        var pos = LevelObject.transform.position.ToVector2();
        _source.SetPosition(pos);

        // get listener position for manual attenuation
        _al.GetListenerProperty(ListenerVector3.Position, out float lx, out float ly, out float lz);
        Vector2 listenerPos = new Vector2(lx / AudioManager.AudioScale, ly / AudioManager.AudioScale);

        // Calculate manual attenuation
        float attenuation = AudioManager.CalculateAttenuation(pos, listenerPos, RefDistance, MaxDistance * 16, Rolloff);
        _source.SetGain(Volume * attenuation);
    }

    public override void OnDestroy()
    {
        Stop();

        if (!string.IsNullOrEmpty(_channel))
        {
            AudioManager.GetChannel(_channel)?.DisconnectSource(_source);
        }

        if (_source != null)
        {
            AudioManager.RemoveSource(_source);
            _source = null;
        }

        base.OnDestroy();
    }
}