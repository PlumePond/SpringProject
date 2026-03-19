using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Audio;
using SpringProject.Core.UI;
using SpringProject.Core.UserInput;

namespace SpringProject.Core.Content;

public static class Loader
{
    public static void Load(string root, GraphicsDevice graphicsDevice)
    {
        Main.Settings = SettingsLoader.Load("settings.json");
        LevelObjectLoader.Load($"{root}/LevelObjects", graphicsDevice);

        AudioManager.SetSounds(AudioCompositeLoader.Load($"{root}/Audio"));
        FontManager.SetFonts(FontLoader.Load($"{root}/Fonts"));
        Input.SetInputStates(InputLoader.Load($"{root}/Input"));
        TextureManager.SetTextures(TextureLoader.Load($"{root}/Textures", graphicsDevice));
    }
}