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
using SpringProject.Core.Components;

namespace SpringProject.Core.Content.Types.LevelObjects;

public class ConveyorBelt : LevelObject
{
    Animator _animator;
    List<Rigidbody> _objectsOnBelt = new();

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

    [Parameter("Speed", 0.1f, 10.0f)] public float Speed { get; set; } = 0.5f;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        _animator = AddComponent<Animator>();
        var frameCount = data.size.X / data.frame.X;
        _animator.Add("idle", new Animation(0, frameCount, FrameInterval, true));
        _animator.Set("idle");

        var collider = AddComponent<BoxCollider>();
        collider.CollisionEnter += OnCollisionEnter;
        collider.CollisionExit += OnCollisionExit;

        AddComponent<AudioSource>();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        foreach (var rigidBody in _objectsOnBelt)
        {
            if (rigidBody == null) continue;

            var dir = transform.flipX ? -1 : 1;
            rigidBody.ExternalVelocity = new Vector2(Speed * dir, 0);
        }
    }

    void OnCollisionEnter(Collider other)
    {
        Debug.Log("Object entered conveyor belt: " + other.LevelObject.data.name);
        var rigidBody = other.LevelObject.GetComponent<Rigidbody>();
        if (rigidBody == null) return;

        if (!_objectsOnBelt.Contains(rigidBody))
        {
            _objectsOnBelt.Add(rigidBody);
        }
    }

    void OnCollisionExit(Collider other)
    {
        var rigidBody = other.LevelObject.GetComponent<Rigidbody>();
        if (rigidBody == null) return;

        if (_objectsOnBelt.Contains(rigidBody))
        {
            _objectsOnBelt.Remove(rigidBody);
        }
    }
}