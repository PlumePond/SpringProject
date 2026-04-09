using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Audio;
using SpringProject.Core.Content.Types;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Content;

public static class Loader
{
    public static void Load(string root, GraphicsDevice graphicsDevice)
    {
        // load types
        LevelObjectTypeLoader.Load();

        // load settings
        Main.Settings = SettingsLoader.Load("settings.json");

        // load data
        LevelObjectLoader.Load(Path.Combine(root, "LevelObjects"), graphicsDevice);
        AudioManager.SetSounds(AudioCompositeLoader.Load(Path.Combine(root, "Audio")));
        FontManager.SetFonts(FontLoader.Load(Path.Combine(root, "Fonts")));
        Input.SetInputStates(InputLoader.Load(Path.Combine(root, "Input")));
        TextureManager.SetTextures(TextureLoader.Load(Path.Combine(root, "Textures"), graphicsDevice));
    }
}