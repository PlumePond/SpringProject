using Silk.NET.OpenAL.Extensions.Creative;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace SpringProject.Core.Audio;

public class ReverbEffect : AudioEffect
{
    float _density;
    float _diffusion;
    float _gain;
    float _decayTime;
    float _lateDelay;

    public ReverbEffect(AudioChannel channel)
    {
        _channel = channel;
        Initialize();
        _channel.AddEffect(this);
    }

    void Initialize()
    {
        _efx = AudioManager.Efx;

        Effect = _efx.GenEffect();
        _efx.SetEffectProperty(Effect, EffectInteger.EffectType, (int)EffectType.Reverb);

        Slot = _efx.GenAuxiliaryEffectSlot();
        _efx.SetAuxiliaryEffectSlotProperty(Slot, EffectSlotInteger.Effect, (int)Effect);
    }

    public void SetProperties(float density, float diffusion, float gain, float decayTime, float lateDelay)
    {
        _density = density;
        _diffusion = diffusion;
        _gain = gain;
        _decayTime = decayTime;
        _lateDelay = lateDelay;

        ApplyProperties();
    }

    void ApplyProperties()
    {
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbDensity, _density);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbDiffusion, _diffusion);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbGain, _gain);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbDecayTime, _decayTime);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbLateReverbDelay, _lateDelay);

        // reattach effect to slot so changes take effect
        _efx.SetAuxiliaryEffectSlotProperty(Slot, EffectSlotInteger.Effect, (int)Effect);
    }

    public override void Dispose()
    {
        _efx.DeleteAuxiliaryEffectSlot(Slot);
        _channel.RemoveEffect(this);
        _efx.DeleteEffect(Effect);
    }

    public override void Reload()
    {
        Initialize();
        ApplyProperties();
    }
}