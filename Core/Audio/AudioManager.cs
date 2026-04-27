using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System;
using Silk.NET.OpenAL;
using Microsoft.Xna.Framework;
using SpringProject.Core.Debugging;
using Silk.NET.OpenAL.Extensions.EXT;
using Silk.NET.OpenAL.Extensions.Creative;
using Silk.NET.OpenAL.Extensions.Soft;
using Silk.NET.OpenAL.Extensions.EXT.Enumeration;

namespace SpringProject.Core.Audio;

public static class AudioManager
{
    struct ContinuousSourceSnapshot
    {
        public string SoundName;
        public Vector2? Position; // null = non-positional
    }

    public static Dictionary<string, AudioComposite> Sounds { get; private set; } = new();
    static readonly Dictionary<string, AudioChannel> _channels = new();

    // active source handles that need cleanup when they finish
    static readonly List<uint> _activeSources = new();
    public static AL _al;
    static ALContext _alc;
    static unsafe Context* _context;
    static unsafe Device* _device;
    public static EffectExtension Efx { get; private set; }

    static string _currentDeviceName = "";
    static float _deviceCheckTimer = 0f;
    const float DEVICE_CHECK_INTERVAL = 2f; // check every 2 seconds for device changes

    public const float AudioScale = 1f / 16f; 

    public static unsafe void Initialize()
    {
        _alc = ALContext.GetApi(true);
        _al  = AL.GetApi(true);

        _device = _alc.OpenDevice("");
        if (_device == null) throw new Exception("Failed to open OpenAL audio device.");

        _context = _alc.CreateContext(_device, null);
        _alc.MakeContextCurrent(_context);

        _currentDeviceName = GetDeviceName(); // query after context is current
        
        _al.DistanceModel(DistanceModel.None);

        if (!_al.TryGetExtension<EffectExtension>(out var efx))
        {
            Debug.Log("OpenAL EFX extension is not supported! Reverb and other audio effects will not work.");
            return;
        }

        Debug.Log($"AudioManager: Device on startup: '{_currentDeviceName}'");

        Efx = efx;
    }

    static unsafe string GetDeviceName()
    {
        if (_alc.TryGetExtension<EnumerateAll>(null, out var enumerateAll))
        {
            var name = enumerateAll.GetString(null, GetEnumerateAllContextString.AllDevicesSpecifier);
            return name ?? "";
        }

        // Fallback to basic specifier
        var fallback = _alc.GetContextProperty(null, GetContextString.DeviceSpecifier);
        return fallback ?? "";
    }

    static unsafe bool IsDeviceConnected()
    {
        const int ALC_CONNECTED = 0x313;
        int connected = 1;
        _alc.GetContextProperty(_device, (GetContextInteger)ALC_CONNECTED, 1, &connected);
        return connected == 1; // 1 = connected, 0 = disconnected
    }

    static unsafe void SetDevice(string deviceName)
    {
        StopAll();

        _alc.MakeContextCurrent(null);
        _alc.DestroyContext(_context);
        _alc.CloseDevice(_device);

        _device = _alc.OpenDevice(deviceName);
        if (_device == null)
        {
            Debug.Log("Failed to open requested device, falling back to default.");
            _device = _alc.OpenDevice("");
        }

        _context = _alc.CreateContext(_device, null);
        _alc.MakeContextCurrent(_context);
        _al.DistanceModel(DistanceModel.None);

        if (_al.TryGetExtension<EffectExtension>(out var efx))
        {
            Efx = efx;
        }
        else
        {
            Efx = null;
        }

        _currentDeviceName = GetDeviceName();
        Debug.Log($"Audio device switched to: '{_currentDeviceName}'.");

        // Re-upload all audio buffers to the new context
        ReloadAllSounds();
    }

    static void ReloadAllSounds()
    {
        foreach (var composite in Sounds.Values)
        {
            composite.ReloadBuffers();
        }
        Debug.Log("Audio buffers reloaded for new device context.");
    }

    static void CheckForDeviceChange()
    {
        string newDefault = GetDeviceName();
        bool deviceDisconnected = !IsDeviceConnected();

        if (deviceDisconnected || newDefault != _currentDeviceName)
        {
            SetDevice(newDefault);
        }
    }

    public static AudioChannel CreateChannel(string name)
    {
        var channel = new AudioChannel(name);
        _channels[name] = channel;
        Debug.Log($"Created audio channel: {name}");
        return channel;
    }

    public static AudioChannel GetChannel(string name)
    {
        _channels.TryGetValue(name, out var channel);
        return channel;
    }

    public static void SetListenerPosition(Vector2 position)
    {
        _al.SetListenerProperty(ListenerVector3.Position, position.X * AudioScale, position.Y * AudioScale, -8f);
    }

    public static float CalculateAttenuation(Vector2 sourcePos, Vector2 listenerPos, float refDistance, float maxDistance, float rolloff)
    {
        float distance = Vector2.Distance(sourcePos, listenerPos);
        
        if (distance <= refDistance)
            return 1f;
        
        if (distance >= maxDistance)
            return 0f;
        
        // Inverse distance rolloff
        float attenuation = refDistance / (refDistance + rolloff * (distance - refDistance));
        return Math.Clamp(attenuation, 0f, 1f);
    }

    public static void SetSourcePosition(uint source, Vector2 position)
    {
        _al.SetSourceProperty(source, SourceVector3.Position, position.X * AudioScale, position.Y * AudioScale, 0f);
    }

    public static AL GetAL()
    {
        return _al;
    }

    public static void SetSounds(Dictionary<string, AudioComposite> sounds)
    {
        Sounds = sounds;
    }

    public static AudioComposite Get(string name)
    {
        if (Sounds.TryGetValue(name, out AudioComposite composite))
        {
            return composite;
        }
        else
        {
            Console.WriteLine($"Audio composite '{name}' not found.");
            return null;
        }
    }

    static public uint Play(string name)
    {
        var composite = Get(name);

        if (composite == null) return 0;

        uint source = composite.Play();
        if (source != 0)
        {
            _activeSources.Add(source);
        }

        return source;
    }

    static public uint Play(string name, Vector2 position)
    {
        var composite = Get(name);

        if (composite == null) return 0;

        uint source = composite.Play(position);
        if (source != 0)
        {
            _activeSources.Add(source);
        }

        return source;
    }

    static public void Update(GameTime gameTime)
    {
        Cleanup();

        // periodically check for device changes
        _deviceCheckTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_deviceCheckTimer >= DEVICE_CHECK_INTERVAL)
        {
            _deviceCheckTimer = 0f;
            CheckForDeviceChange();
        }
    }

    static void Cleanup()
    {
        for (int i = _activeSources.Count - 1; i >= 0; i--)
        {
            uint source = _activeSources[i];
            
            // check if source is still valid
            _al.GetSourceProperty(source, GetSourceInteger.SourceState, out int state);
            
            // clean up stopped, paused, or invalid sources
            if (state == (int)SourceState.Stopped || state == (int)SourceState.Paused)
            {
                _al.DeleteSource(source);
                _activeSources.RemoveAt(i);
            }
            else if (state == 0) // Invalid/initial state means source may have failed
            {
                _al.DeleteSource(source);
                _activeSources.RemoveAt(i);
            }
        }
    }

    static public void StopAll()
    {
        foreach (uint source in _activeSources)
        {
            _al.SourceStop(source);
            _al.DeleteSource(source);
        }
        _activeSources.Clear();
    }
}