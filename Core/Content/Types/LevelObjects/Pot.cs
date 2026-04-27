using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringProject.Core.Editor;
using SpringProject.Core.Audio;
using SpringProject.Core.Components;

namespace SpringProject.Core.Content.Types.LevelObjects;

public class Pot : LevelObject
{
    Animator _animator;

    //[Parameter("Sound")] public DropdownList Sound { get; set; } = new DropdownList(() => AudioManager.Sounds.Select(s => new DropdownOption(s.Key, s.Value)).ToList());

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        _animator = AddComponent<Animator>();

        _animator.Add("default", new Animation(0, 1, 0.1f, false));
        _animator.Add("wiggle", new Animation(1, 5, 0.1f, false));

        _animator.Set("default");

        AddComponent<Collider>().CollisionEnter += OnCollisionEnter;
    }

    void OnCollisionEnter(Collider other)
    {
        if (other.LevelObject.GetComponent<Rigidbody>() == null) return;

        _animator.Set("wiggle");
        _animator.Queue("default");
        var sound = AudioManager.Get("pot_touch");
        sound.SetChannel("sfx");
        sound.Play(transform.position.ToVector2());
    }

    public override void OnRemoved()
    {
        GetComponent<Collider>().CollisionEnter -= OnCollisionEnter;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        base.DrawDebug(spriteBatch, font);

        // string debugText = $"{_position}";
        // Vector2 textPos = bounds.Center.ToVector2();
        // Vector2 textOrigin = font.MeasureString(debugText) * 0.5f;
        // spriteBatch.DrawString(font, debugText, textPos, Color.White, 0, textOrigin, Vector2.One * 0.25f);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}