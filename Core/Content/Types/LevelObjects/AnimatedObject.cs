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

namespace SpringProject.Core.Content.Types.LevelObjects;

public class AnimatedObject : LevelObject
{
    Animator _animator;

    [Parameter("Frame Interval")]
    public float FrameInterval
    {
        get =>  _animator?.CurrentAnimation?.FrameInterval ?? 0.1f;
        set
        {
            if (_animator != null && _animator.CurrentAnimation != null)
            {
                _animator.CurrentAnimation.FrameInterval = value;
            }
        }
    }

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        _animator = AddComponent<Animator>();
        var frameCount = data.size.X / data.frame.X;
        _animator.Add("idle", new Animation(0, frameCount, FrameInterval, true));
        _animator.Set("idle");
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        foreach (var component in Components)
        {
            component.Draw(spriteBatch);
        }
    }
}