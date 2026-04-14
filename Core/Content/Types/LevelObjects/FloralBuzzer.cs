using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.AI;
using SpringProject.Core.Audio;
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

    protected override float Gravity => 0.0f;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Animator.Add("fly", new Animation(0, 4, 0.08f, true));
        Animator.Set("fly");
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
            
        HandleTargetting(gameTime);
        HandlePathfinding(gameTime);

        if (Velocity.X < 0 && !transform.flipX)
        {
            SetFlipX(true);
        }
        else if (Velocity.X > 0 && transform.flipX)
        {
            SetFlipX(false);
        }

        if (Velocity.Length() > MAX_SPEED)
        {
            Velocity = Vector2.Normalize(Velocity) * MAX_SPEED;
        }
    }

    protected override void FollowPath()
    {
        if (_path == null) return;

        var difference = (_path[_currentNode].Point - hitbox.Center).ToVector2();
        if (difference.LengthSquared() < 1f) return;

        Velocity += Vector2.Normalize(difference) * ACCELERATION;
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        base.DrawDebug(spriteBatch, font);

        if (_path == null) return;

        foreach (var node in _path)
        {
            node.Draw(spriteBatch);
        }
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