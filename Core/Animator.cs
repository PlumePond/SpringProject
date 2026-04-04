using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Editor;

namespace SpringProject.Core;

public class Animator
{
    public Animation CurrentAnimation { get; private set; }
    public int CurrentFrame { get; private set; } = 0;
    public Dictionary<string, Animation> Animations { get; private set; }

    public Action<int> IterateFrame;

    LevelObject _levelObject;
    Texture2D _sprite;
    Point _size;

    float _timer = 0.0f;

    public Animator(LevelObject levelObject)
    {
        _levelObject = levelObject;
        _sprite = _levelObject.data.sprite;
        _size = _levelObject.bounds.Size;

        Animations = new Dictionary<string, Animation>();
    }

    public void Add(string name, Animation animation)
    {
        if (Animations.ContainsKey(name))
        {
            throw new ArgumentException($"Animation with name '{name}' already exists for level object '{_levelObject.data.name}'.");
        }
        Animations.Add(name, animation);
    }

    public void Set(string name)
    {
        if (!Animations.TryGetValue(name, out var animation))
        {
            throw new KeyNotFoundException($"Animation '{name}' not found for level object '{_levelObject.data.name}'.");
        }

        CurrentAnimation = animation;
        CurrentFrame = 0; // reset to the first frame
        _timer = 0.0f; // reset timer
    }

    public void Update(GameTime gameTime)
    {
        // increment timer
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        float frameInterval = CurrentAnimation.FrameInterval;
        int frameCount = CurrentAnimation.FrameCount;
        bool loop = CurrentAnimation.Loop;

        if (_timer >= frameInterval)
        {
            if (loop)
            {
                _timer -= frameInterval;
                CurrentFrame++;

                // reset to first frame if looping
                if (CurrentFrame >= frameCount)
                {
                    CurrentFrame = 0;
                }
            }
            else if (CurrentFrame < frameCount - 1)
            {
                _timer -= frameInterval;
                CurrentFrame++;
            }

            IterateFrame?.Invoke(CurrentFrame);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Point framedSize = _levelObject.data.frame != Point.Zero ? _levelObject.data.frame : _levelObject.data.size;
        Vector2 drawPos = new Vector2(_levelObject.bounds.X + _levelObject.bounds.Width / 2f, _levelObject.bounds.Y + _levelObject.bounds.Height / 2f);
        Vector2 origin = new Vector2(framedSize.X / 2f, framedSize.Y / 2f);
        float radians = _levelObject.transform.rotation * (float)Math.PI / 180f;

        SpriteEffects effects = SpriteEffects.None;
        if (_levelObject.transform.flipX) effects |= SpriteEffects.FlipHorizontally;
        if (_levelObject.transform.flipY) effects |= SpriteEffects.FlipVertically;


        Vector2 drawScale = new Vector2((float)_levelObject.size.X / _levelObject.data.sprite.Width, (float)_levelObject.size.Y / _levelObject.data.sprite.Height);

        Color objectColor = _levelObject.selected ? Color.LightGoldenrodYellow * _levelObject.color : _levelObject.color;

        Point sourcePos = new Point(CurrentFrame * _levelObject.frame.X, CurrentAnimation.Index * _levelObject.frame.Y);
        Rectangle sourceRect = new Rectangle(sourcePos, _levelObject.frame);

        spriteBatch.Draw(_levelObject.data.sprite, drawPos, sourceRect, objectColor * _levelObject.tint, radians, origin, drawScale, effects, 0f);

        if (_levelObject.hovered)
        {
            spriteBatch.Draw(_levelObject.data.outline, drawPos, sourceRect, Color.White, radians, origin, drawScale, effects, 0f);
        }
        else if (_levelObject.selected)
        {
            spriteBatch.Draw(_levelObject.data.outline, drawPos, sourceRect, Color.Yellow, radians, origin, drawScale, effects, 0f);
        }
    }
}