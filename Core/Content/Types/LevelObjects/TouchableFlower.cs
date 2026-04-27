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

public class TouchableFlower : LevelObject
{
    Animator _animator;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        _animator = AddComponent<Animator>();

        _animator.Add("default", new Animation(0, 1, 0.1f, false));
        _animator.Add("wiggle", new Animation(1, 2, 0.2f, false));

        _animator.Set("default");

        
        AddComponent<BoxCollider>().CollisionEnter += OnCollisionEnter;
    }

    void OnCollisionEnter(Collider other)
    {
        var rigidBody = other.LevelObject.GetComponent<Rigidbody>();
        if (rigidBody == null) return;

        if (rigidBody.Velocity.X > 0 && transform.flipX)
        {
            transform.flipX = false;
        }
        else if (rigidBody.Velocity.X < 0 && !transform.flipX)
        {
            transform.flipX = true;
        }

        _animator.Set("wiggle");
        _animator.Queue("default");
        AudioManager.Get(data.placeSound).Play();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
}