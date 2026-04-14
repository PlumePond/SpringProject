using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Content;

public static class TextureManager
{
    public static Dictionary<string, Texture2D> Textures { get; private set; } = new Dictionary<string, Texture2D>();
    public static Action<Texture2D> TextureUpdatedEvent;
    static string _path = Path.Combine("Data", "Textures");

    public static readonly Texture2D MissingTexture;

    static TextureManager()
    {
        RuntimeReloader.FileChangedEvent += OnFileChanged;
    }

    public static Texture2D Get(string name)
    {
        if (Textures.TryGetValue(name, out Texture2D texture))
        {
            return texture;
        }
        else
        {
            Console.WriteLine($"Texture '{name}' not found.");
            return null;
        }
    }

    public static void SetTextures(Dictionary<string, Texture2D> textures)
    {
        Textures = textures;
    }

    static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(e.FullPath)) return; // skip directories

        var texName = Path.GetFileNameWithoutExtension(e.FullPath);
        
        foreach (var kvp in Textures)
        {
            var path = Path.Combine(_path, kvp.Key + ".png");
            if (!path.Equals(e.FullPath)) continue;

            using (var stream = File.OpenRead(e.FullPath))
            {
                Textures[texName] = Texture2D.FromStream(Main.Graphics, stream);
            }
        }
    }
}