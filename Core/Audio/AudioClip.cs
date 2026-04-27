using Silk.NET.OpenAL;

namespace SpringProject.Core.Audio;

public class AudioClip(string name, uint buffer, byte[] pcmData, int sampleRate, BufferFormat format)
{
    public string Name { get; } = name;
    public uint Buffer { get; private set; } = buffer;
    public byte[] PcmData { get; } = pcmData;
    public int SampleRate { get; } = sampleRate;
    public BufferFormat Format { get; } = format;

    public void ReloadBuffers()
    {
        // re-upload PCM data to OpenAL buffer (used when device is lost and recreated)
        Buffer = AudioManager.GetAL().GenBuffer();
        AudioManager.GetAL().BufferData(Buffer, Format, PcmData, SampleRate);
    }
}