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

public class FloralStalker : Entity
{
    const float PLAYER_CHECK_INTERVAL = 1.0f;
    const float ACCELERATION = 0.1f;
    const float MAX_SPEED = 3.0f;
    
    float _playerCheckTimer = 0.0f;

    protected override float Gravity => 8.0f;

    public ParticleSystem particleSystem;

    Point _wallCheckSize;
    bool _touchingWall;
    Rectangle _wallCheck;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Animator.Add("idle", new Animation(0, 1, 0.15f, false));
        Animator.Add("run", new Animation(2, 5, 0.1f, true));
        Animator.Add("aggro", new Animation(1, 2, 0.2f, false));
        Animator.Set("idle");

        StateMachine.Add("idle", new Idle(this, this));
        StateMachine.Add("run", new Run(this, this));
        StateMachine.Add("jump", new Jump(this, this));
        StateMachine.Add("fall", new Fall(this, this));
        StateMachine.Add("aggro", new Aggro(this, this));
        StateMachine.Set("idle");

        _wallCheckSize = new Point(10, data.hitbox.Height - 4);

        particleSystem = new ParticleSystem();

        var smallDustData = new ParticleData
        {
            texture = TextureManager.Get("particle_dust_small"),
            frameCount = 5,
            frameInterval = 0.2f,
            frameSize = new Point(12, 12),
            loop = false,
            startSpeed = 0.5f
        };

        particleSystem.AddType("small_dust", smallDustData);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        particleSystem.Draw(spriteBatch);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
            
        WallCheck();
        HandleTargetting(gameTime);

        particleSystem.Update(gameTime);

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

        // if (_target != null)
        // {
        //     if (transform.position.Distance(_target.position) < 0.1f) return;
        //     var direction = Vector2.Normalize((_target.position - transform.position).ToVector2());
        //     Velocity += direction * ACCELERATION;
        //     if (Velocity.Length() > MAX_SPEED)
        //     {
        //         Velocity = Vector2.Normalize(Velocity) * MAX_SPEED;
        //     }
        // }
    }

    protected override void FollowPath()
    {
        var difference = (_path[_currentNode].Point - hitbox.Center).ToVector2();
        if (difference.LengthSquared() < 1f) return;

        Velocity.X += Vector2.Normalize(difference).X * ACCELERATION;

        if (difference.Y < 0 && Grounded && _touchingWall)
        {
            StateMachine.Set("jump");
        }
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        base.DrawDebug(spriteBatch, font);

        Debug.DrawRectangle(spriteBatch, _wallCheck, _touchingWall ? Color.Green : Color.Red);

        if (_path == null)
        {
            Debug.Log("Path is null!");
            return;
        }

        foreach (var node in _path)
        {
            node.Draw(spriteBatch);
        }
    }

    void WallCheck()
    {
        if (transform.flipX)
        {
            _wallCheck = new Rectangle(hitbox.Left - _wallCheckSize.X, hitbox.Center.Y - _wallCheckSize.Y / 2, _wallCheckSize.X, _wallCheckSize.Y);
        }
        else
        {
            _wallCheck = new Rectangle(hitbox.Right, hitbox.Center.Y - _wallCheckSize.Y / 2, _wallCheckSize.X, _wallCheckSize.Y);
        }

        if (grid.RectInsideObject(_wallCheck, layer, out var levelObject, this))
        {
            _touchingWall = true;
            FootstepMaterial = levelObject.data.material;
        }
        else
        {
            _touchingWall = false;
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

    internal class Idle : State
    {
        FloralStalker _floralStalker;
        const float RADIUS = 128f;

        public Idle(Entity entity, FloralStalker floralStalker) : base(entity)
        {
            _floralStalker = floralStalker;
        }

        public override void Enter()
        {
            base.Enter();
            _animator.Set("idle");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_floralStalker._target != null)
            {
                if (_entity.hitbox.Center.Distance(_floralStalker._target.position) < RADIUS)
                {
                    _stateMachine.Set("aggro");
                }
            }
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            Debug.DrawCircle(spriteBatch, _entity.hitbox.Center, RADIUS, Color.Blue, 3, 32);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    internal class Run : State
    {
        FloralStalker _floralStalker;
        public Run(Entity entity, FloralStalker floralStalker) : base(entity)
        {
            _floralStalker = floralStalker;
        }

        public override void Enter()
        {
            base.Enter();
            _animator.Set("run");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            _floralStalker.HandlePathfinding(gameTime);
            if (!_entity.Grounded && _entity.Velocity.Y > 0.1f)
            {
                _stateMachine.Set("fall");
            }
        }

        public override void IterateFrame(int frame)
        {
            if (frame == 0 || frame == 4 || frame == 2)
            {
                AudioManager.Get("fast_critter_footstep").Play();
                float offset = -10;
                float flippedOffset = (_entity.transform.flipX ? -1 : 1) * offset;
                var offsetVector = new Vector2(flippedOffset, 0);
                _floralStalker.particleSystem.Burst("small_dust", _entity.hitbox.Center.ToVector2() + offsetVector, 1, 3, 5f, 5f);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    internal class Jump : State
    {
        FloralStalker _floralStalker;

        public Jump(Entity entity, FloralStalker floralStalker) : base(entity)
        {
            _floralStalker = floralStalker;
        }

        public override void Enter()
        {
            base.Enter();

            Debug.Log("jump!");
            _entity.Velocity.Y = -25f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_entity.Velocity.Y > 0.0f)
            {
                _stateMachine.Set("fall");
            }

            _floralStalker.HandlePathfinding(gameTime);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    internal class Fall : State
    {
        FloralStalker _floralStalker;

        public Fall(Entity entity, FloralStalker floralStalker) : base(entity)
        {
            _floralStalker = floralStalker;
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_entity.Grounded)
            {
                _stateMachine.Set("run");
            }

            _floralStalker.HandlePathfinding(gameTime);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    internal class Aggro : State
    {
        FloralStalker _floralStalker;

        public Aggro(Entity entity, FloralStalker floralStalker) : base(entity)
        {
            _floralStalker = floralStalker;
        }

        float _counter = 0f;

        public override void Enter()
        {
            base.Enter();

            _animator.Set("aggro");
            _counter = 0f;
            _floralStalker.SetFlipX(_floralStalker._target.position.X < _entity.transform.position.X);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _counter += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_counter > _animator.CurrentAnimation.Length)
            {
                _stateMachine.Set("run");
            }

            _floralStalker.HandlePathfinding(gameTime);
        }
    }
}