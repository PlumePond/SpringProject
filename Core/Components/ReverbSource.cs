using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Silk.NET.OpenAL.Extensions.Creative;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Components;

public class ReverbSource : Component
{
    uint _slot;
    uint _effect;
    string _registeredChannel = "";

    [Parameter("Channel")] public string Channel { get; set; } = "";
    [Parameter("Density", 0f, 1f)] public float Density { get; set; } = 1f;
    [Parameter("Diffusion", 0f, 1f)] public float Diffusion { get; set; } = 1f;
    [Parameter("Gain", 0f, 1f)] public float Gain { get; set; } = 1f;
    [Parameter("Decay Time", 0.1f, 20f)] public float DecayTime { get; set; } = 1f;
    [Parameter("Late Delay", 0f, 0.1f)] public float LateDelay { get; set; } = 0.01f;

    public override void Start()
    {
        RegisterEffect();
    }

    public override void Update(GameTime gameTime)
    {
        // Re-register effect if channel changed
        if (_registeredChannel != Channel)
        {
            UnregisterEffect();
            RegisterEffect();
        }

        ApplyProperties();
    }

    public override void EditorUpdate(GameTime gameTime)
    {
        // re-register effect if channel or properties changed

        if (_registeredChannel != Channel)
        {
            UnregisterEffect();
            RegisterEffect();
        }

        ApplyProperties();
    }

    void RegisterEffect()
    {
        var efx = AudioManager.Efx;
        if (efx == null)
        {
            Debug.Log("ReverbSource: EFX not available.");
            return;
        }
        if (string.IsNullOrEmpty(Channel))
        {
            //Debug.Log("ReverbSource: Channel not set.");
            return;
        }

        var channel = AudioManager.GetChannel(Channel);
        if (channel == null)
        {
            //Debug.Log($"ReverbSource: Channel '{Channel}' does not exist.");
            return;
        }

        _effect = efx.GenEffect();
        efx.SetEffectProperty(_effect, EffectInteger.EffectType, (int)EffectType.Reverb);

        _slot = efx.GenAuxiliaryEffectSlot();

        ApplyProperties();

        // attatch effect to slot
        efx.SetAuxiliaryEffectSlotProperty(_slot, EffectSlotInteger.Effect, (int)_effect);

        channel.AddEffect(_slot, _effect);
        _registeredChannel = Channel;
        //Debug.Log($"ReverbSource: Registered reverb on channel '{Channel}' - slot={_slot}, effect={_effect}");
    }

    void UnregisterEffect()
    {
        if (_slot == 0) return;

        var channel = AudioManager.GetChannel(_registeredChannel);
        channel?.RemoveEffect(_slot, _effect);
        
        AudioManager.Efx?.DeleteAuxiliaryEffectSlot(_slot);
        AudioManager.Efx?.DeleteEffect(_effect);

        _slot = 0;
        _effect = 0;
        _registeredChannel = "";
    }

    void ApplyProperties()
    {
        var efx = AudioManager.Efx;
        if (efx == null) return;
        if (_effect == 0) return;

        efx.SetEffectProperty(_effect, EffectFloat.ReverbDensity, Density);
        efx.SetEffectProperty(_effect, EffectFloat.ReverbDiffusion, Diffusion);
        efx.SetEffectProperty(_effect, EffectFloat.ReverbGain, Gain);
        efx.SetEffectProperty(_effect, EffectFloat.ReverbDecayTime, DecayTime);
        efx.SetEffectProperty(_effect, EffectFloat.ReverbLateReverbDelay, LateDelay);

        // reattach effect to slot so changes take effect
        if (_slot != 0)
        {
            efx.SetAuxiliaryEffectSlotProperty(_slot, EffectSlotInteger.Effect, (int)_effect);
        }
    }

    public override void OnDestroy()
    {
        UnregisterEffect();
        base.OnDestroy();
    }
}