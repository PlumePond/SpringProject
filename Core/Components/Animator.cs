using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Components;
using SpringProject.Core.Editor;

namespace SpringProject.Core;

public class Animator : Component
{
    public Animation CurrentAnimation { get; private set; }
    public int CurrentFrame { get; private set; } = 0;
    public Dictionary<string, Animation> Animations { get; private set; }

    public Action<int> IterateFrameEvent;

    float _timer = 0.0f;

    Queue<string> _queue = new Queue<string>();

    Sprite _sprite;
    bool _hasAdvanced = false;

    public override void Start()
    {
        base.Start();

        Animations = new Dictionary<string, Animation>();
        _sprite = LevelObject.GetComponent<Sprite>();

        if (_sprite == null)
        {
            throw new ArgumentException($"Animator on '{LevelObject.data.name}' requires the Sprite Component, but it is missing!");
        }
    }

    public void Add(string name, Animation animation)
    {
        if (Animations.ContainsKey(name))
        {
            throw new ArgumentException($"Animation with name '{name}' already exists for level object '{LevelObject.data.name}'.");
        }
        Animations.Add(name, animation);
    }

    public void Set(string name)
    {
        if (!Animations.TryGetValue(name, out var animation))
        {
            throw new KeyNotFoundException($"Animation '{name}' not found for level object '{LevelObject.data.name}'.");
        }

        CurrentAnimation = animation;
        CurrentFrame = 0; // reset to the first frame
        _timer = 0.0f; // reset timer

        // apply the first frame immediately instead of waiting for the timer
        Point sourcePos = new Point(0, CurrentAnimation.Index * LevelObject.frame.Y);
        Rectangle sourceRect = new Rectangle(sourcePos, LevelObject.frame);
        _sprite.SetSourceRect(sourceRect);

        _hasAdvanced = false;
    }

    public void Queue(string name)
    {
        if (!Animations.TryGetValue(name, out var animation))
        {
            throw new KeyNotFoundException($"Animation '{name}' not found for level object '{LevelObject.data.name}'.");
        }
        
        _queue.Enqueue(name);
    }

    public override void EditorUpdate(GameTime gameTime)
    {
        Update(gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        // increment timer
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        float frameInterval = CurrentAnimation.FrameInterval;
        int frameCount = CurrentAnimation.FrameCount;
        bool loop = CurrentAnimation.Loop;

        if (_timer >= frameInterval)
        {
            IterateFrameEvent?.Invoke(CurrentFrame + 1);

            // dequeue current queued animation as soon as the current animation is over
            if (CurrentFrame >= frameCount - 1)
            {
                if (_queue.TryDequeue(out var animation))
                {
                    Set(animation);
                    return;
                }
            }

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
            
            Point sourcePos = new Point(CurrentFrame * LevelObject.frame.X, CurrentAnimation.Index * LevelObject.frame.Y);
            Rectangle sourceRect = new Rectangle(sourcePos, LevelObject.frame);
            _sprite.SetSourceRect(sourceRect);
        }
    }
}