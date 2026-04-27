using Silk.NET.OpenAL.Extensions.Creative;
using Microsoft.Xna.Framework;
using System;

namespace SpringProject.Core.Audio;

public class LowPassEffect : IDisposable
{
    readonly EffectExtension _efx;
    public uint Effect;
    public uint Slot;

    public LowPassEffect(EffectExtension efx)
    {
        _efx = efx;

        Effect = efx.GenFilter();
        efx.SetFilterProperty(Effect, FilterInteger.FilterType, (int)FilterType.Lowpass);
        efx.SetFilterProperty(Effect, FilterFloat.LowpassGain, 1f);
        efx.SetFilterProperty(Effect, FilterFloat.LowpassGainHF, 1f);

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