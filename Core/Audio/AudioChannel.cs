
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
    readonly List<(uint slot, uint effect)> _effects = new();
    readonly List<uint> _filters = new();
    readonly List<uint> _connectedSources = new();

    public void AddEffect(uint slot, uint effect)
    {
        _effects.Add((slot, effect));
        // Debug.Log($"AudioChannel '{Name}': Added effect slot {slot}, total effects: {_effects.Count}");

        // reconnect sources to apply new effect
        foreach (var source in _connectedSources)
        {
            ConnectSource(source);
        }
    }

    public void RemoveEffect(uint slot, uint effect)
    {
        _effects.Remove((slot, effect));
        // Debug.Log($"AudioChannel '{Name}': Removed effect slot {slot}, remaining effects: {_effects.Count}");

        // reconnect sources to remove effect
        foreach (var source in _connectedSources)
        {
            ConnectSource(source);
        }
    }

    // public void AddFilter(uint filter)
    // {
    //     _filters.Add(filter);
    //     RefreshSources();
    // }

    // public void RemoveFilter(uint filter)
    // {
    //     _filters.Remove(filter);
    //     RefreshSources();
    // }

    public void ConnectSource(uint source)
    {
        if (!_connectedSources.Contains(source))
        {
            _connectedSources.Add(source);
        }

        var efx = AudioManager.Efx;
        // if (efx == null)
        // {
        //     Debug.Log($"AudioChannel '{Name}': EFX not available, cannot connect reverb.");
        //     return;
        // }

        // if (_effects.Count == 0)
        // {
        //     Debug.Log($"AudioChannel '{Name}': No effects to connect for source {source}");
        //     return;
        // }

        //Debug.Log($"AudioChannel '{Name}': Connecting source {source} to {_effects.Count} effect(s)");

        // wire source to all effect slots in this channel, up to 4 sends
        for (int i = 0; i < _effects.Count && i < 4; i++)
        {
            // Slot index, send index, filter (0 = no filter)
            efx.SetSourceProperty(source, EFXSourceInteger3.AuxiliarySendFilter, (int)_effects[i].slot, i, 0);
            //Debug.Log($"  -> Send {i}: slot={_effects[i].slot}, effect={_effects[i].effect}");
        }

        // clear any leftover sends beyond current effect count
        for (int i = _effects.Count; i < 4; i++)
        {
            efx.SetSourceProperty(source, EFXSourceInteger3.AuxiliarySendFilter, 0, i, 0);
        }
    }

    public void DisconnectSource(uint source)
    {
        _connectedSources.Remove(source);

        var efx = AudioManager.Efx;
        if (efx == null) return;

        // clear all sends for this source
        for (int i = 0; i < 4; i++)
        {
            efx.SetSourceProperty(source, EFXSourceInteger3.AuxiliarySendFilter, 0, i, 0);
        }
    }
}