using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System;

namespace SpringProject.Core.Audio;

public class AudioManager
{
    public static Dictionary<string, AudioComposite> Sounds { get; private set; } = new Dictionary<string, AudioComposite>();

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

    public static void SetSounds(Dictionary<string, AudioComposite> sounds)
    {
        Sounds = sounds;
    }
}