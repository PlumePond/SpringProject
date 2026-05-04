using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Silk.NET.OpenAL.Extensions.Creative;
using SpringProject.Core.Audio;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Components;

public class ReverbSource : Component
{
    ReverbEffect _reverbEffect = null;
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
        _reverbEffect?.SetProperties(Density, Diffusion, Gain, DecayTime, LateDelay);
    }

    public override void EditorUpdate(GameTime gameTime)
    {
        // re-register effect if channel or properties changed

        if (_registeredChannel != Channel)
        {
            UnregisterEffect();
            RegisterEffect();
        }

        _reverbEffect?.SetProperties(Density, Diffusion, Gain, DecayTime, LateDelay);
    }

    void RegisterEffect()
    {
        var channel = AudioManager.GetChannel(Channel);
        if (channel == null) return;

        _reverbEffect = new ReverbEffect(channel);
        _reverbEffect.SetProperties(Density, Diffusion, Gain, DecayTime, LateDelay);
        _registeredChannel = channel.Name;
    }

    void UnregisterEffect()
    {
        if (_reverbEffect == null) return;
        
        _reverbEffect.Dispose();
        _reverbEffect = null;
        _registeredChannel = "";
    }

    public override void OnDestroy()
    {
        UnregisterEffect();
        base.OnDestroy();
    }
}