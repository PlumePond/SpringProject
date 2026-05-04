
using System.Collections.Generic;
using System.Linq;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.Creative;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Audio;

public class AudioChannel(string name)
{
    public string Name { get; } = name;
    
    // each effect gets its own slot
    readonly List<AudioEffect> _effects = new();
    readonly List<uint> _filters = new();
    readonly List<AudioSource> _connectedSources = new();

    public void AddEffect(AudioEffect effect)
    {
        _effects.Add(effect);

        // reconnect sources to apply new effect
        ReconnectSources();
    }

    public void RemoveEffect(AudioEffect effect)
    {
        _effects.Remove(effect);

        // reconnect sources to remove effect
        ReconnectSources();
    }

    public void Reload()
    {
        foreach (var effect in _effects)
        {
            effect.Reload();
        }
        ReconnectSources();
    }

    public void ReconnectSources()
    {
        foreach (var source in _connectedSources)
        {
            ConnectSource(source);
        }
    }

    public void ConnectSource(AudioSource source)
    {
        if (!_connectedSources.Contains(source))
        {
            _connectedSources.Add(source);
        }

        var efx = AudioManager.Efx;

        // wire source to all effect slots in this channel, up to 4 sends
        for (int i = 0; i < _effects.Count && i < 4; i++)
        {
            // slot index, send index, filter (0 = no filter)
            efx.SetSourceProperty(source.Id, EFXSourceInteger3.AuxiliarySendFilter, (int)_effects[i].Slot, i, 0);
        }

        // clear any leftover sends beyond current effect count
        for (int i = _effects.Count; i < 4; i++)
        {
            efx.SetSourceProperty(source.Id, EFXSourceInteger3.AuxiliarySendFilter, 0, i, 0);
        }
    }

    public void DisconnectSource(AudioSource source)
    {
        _connectedSources.Remove(source);

        var efx = AudioManager.Efx;
        if (efx == null) return;

        // clear all sends for this source
        for (int i = 0; i < 4; i++)
        {
            efx.SetSourceProperty(source.Id, EFXSourceInteger3.AuxiliarySendFilter, 0, i, 0);
        }
    }
}