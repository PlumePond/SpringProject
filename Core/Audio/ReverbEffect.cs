using Silk.NET.OpenAL.Extensions.Creative;
using Microsoft.Xna.Framework;
using System;

namespace SpringProject.Core.Audio;

public class ReverbEffect : IDisposable
{
    readonly EffectExtension _efx;
    public uint Effect;
    public uint Slot;

    public ReverbEffect(EffectExtension efx)
    {
        _efx = efx;

        Effect = efx.GenEffect();
        efx.SetEffectProperty(Effect, EffectInteger.EffectType, (int)EffectType.Reverb);

        Slot = efx.GenAuxiliaryEffectSlot();
        efx.SetAuxiliaryEffectSlotProperty(Slot, EffectSlotInteger.Effect, (int)Effect);
    }

    public void SetProperties(float density, float diffusion, float gain, float decayTime, float lateDelay)
    {
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbDensity, density);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbDiffusion, diffusion);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbGain, gain);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbDecayTime, decayTime);
        _efx.SetEffectProperty(Effect, EffectFloat.ReverbLateReverbDelay, lateDelay);

        // Reattach effect to slot so changes take effect
        _efx.SetAuxiliaryEffectSlotProperty(Slot, EffectSlotInteger.Effect, (int)Effect);
    }

    public void Dispose()
    {
        _efx.DeleteAuxiliaryEffectSlot(Slot);
        _efx.DeleteEffect(Effect);
    }
}