using System;
using Silk.NET.OpenAL.Extensions.Creative;

namespace SpringProject.Core.Audio;

public class AudioEffect : IDisposable
{
    public EffectExtension _efx { get; protected set; }
    protected AudioChannel _channel;
    public uint Effect { get; protected set; }
    public uint Slot { get; protected set; }

    public virtual void Reload()
    {
        
    }

    public virtual void Dispose()
    {
        
    }
}