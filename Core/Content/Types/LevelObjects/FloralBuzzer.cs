using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.AI;
using SpringProject.Core.Audio;
using SpringProject.Core.Components;
using SpringProject.Core.Content.Types.LevelObjects;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;
using SpringProject.Core.Particles;

namespace SpringProject.Core.Content.Types;

public class FloralBuzzer : Entity
{
    const float PLAYER_CHECK_INTERVAL = 1.0f;
    const float ACCELERATION = 0.05f;
    const float MAX_SPEED = 0.5f;
    
    float _playerCheckTimer = 0.0f;

    protected float NodeThresholdX => 16f;
    protected float NodeThresholdY => 16f;
    
    Pathfinder Pathfinder;
    Rigidbody Rigidbody;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Animator.Add("fly", new Animation(0, 4, 0.08f, true));
        Animator.Set("fly");

        Rigidbody = GetComponent<Rigidbody>();
        Pathfinder = AddComponent<Pathfinder>();

        Pathfinder.FollowPathEvent += FollowPath;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
            
        HandleTargetting(gameTime);
        Pathfinder.HandlePathfinding(gameTime);

        if (Rigidbody.Velocity.X < 0 && !transform.flipX)
        {
            SetFlipX(true);
        }
        else if (Rigidbody.Velocity.X > 0 && transform.flipX)
        {
            SetFlipX(false);
        }

        if (Rigidbody.Velocity.Length() > MAX_SPEED)
        {
            Rigidbody.InternalVelocity = Vector2.Normalize(Rigidbody.Velocity) * MAX_SPEED;
        }
    }

    void FollowPath(Node currentNode)
    {
        var difference = (currentNode.Point - hitbox.Center).ToVector2();
        if (difference.LengthSquared() < 1f) return;

        Rigidbody.InternalVelocity += Vector2.Normalize(difference) * ACCELERATION;
    }

    void HandleTargetting(GameTime gameTime)
    {
        if (_target != null) return;

        _playerCheckTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_playerCheckTimer > PLAYER_CHECK_INTERVAL)
        {
            if (Player.Instance != null)
            {
                _target = Player.Instance.transform;
            }
            _playerCheckTimer = 0.0f;
        }
    }
}