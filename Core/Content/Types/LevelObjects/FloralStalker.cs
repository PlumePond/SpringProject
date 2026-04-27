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

public class FloralStalker : Entity
{
    [Parameter("Acceleration", 0f, 1f)] public float Acceleration = 0.1f;
    [Parameter("Player Check Interval", 0f, 5f)] public float PlayerCheckInterval = 1.0f;
    [Parameter("Max Speed", 0f, 10f)] public float MaxSpeed = 3.0f;
    [Parameter("Player Check Radius", 0f, 256f)] public float PlayerCheckRadius = 3.0f;
    [Parameter("Jump Force", 0f, 30f)] public float JumpForce = 25.0f;
    
    float _playerCheckTimer = 0.0f;

    protected override float Gravity => 8.0f;

    public ParticleSystem ParticleSystem;

    Point _wallCheckSize;
    bool _touchingWall;

    public Rectangle WallCheckRect;
    public Rigidbody Rigidbody;
    public StateMachine<FloralStalker> StateMachine;
    Pathfinder Pathfinder;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Rigidbody = GetComponent<Rigidbody>();
        Pathfinder = AddComponent<Pathfinder>();

        Animator.Add("idle", new Animation(0, 1, 0.15f, false));
        Animator.Add("run", new Animation(2, 5, 0.1f, true));
        Animator.Add("aggro", new Animation(1, 2, 0.2f, false));
        Animator.Set("idle");

        StateMachine = AddComponent<StateMachine<FloralStalker>>();

        StateMachine.Add<Idle>("idle");
        StateMachine.Add<Run>("run");
        StateMachine.Add<Jump>("jump");
        StateMachine.Add<Fall>("fall");
        StateMachine.Add<Aggro>("aggro");
        StateMachine.Set("idle");

        _wallCheckSize = new Point(10, data.hitbox.Height - 4);

        ParticleSystem = new ParticleSystem();

        var smallDustData = new ParticleData
        {
            texture = TextureManager.Get("particle_dust_small"),
            frameCount = 5,
            frameInterval = 0.2f,
            frameSize = new Point(12, 12),
            loop = false,
            startSpeed = 0.5f
        };

        ParticleSystem.AddType("small_dust", smallDustData);

        Pathfinder.FollowPathEvent += FollowPath;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
            
        WallCheck();
        HandleTargetting(gameTime);

        if (Rigidbody.Velocity.X < 0 && !transform.flipX)
        {
            SetFlipX(true);
        }
        else if (Rigidbody.Velocity.X > 0 && transform.flipX)
        {
            SetFlipX(false);
        }

        if (Rigidbody.Velocity.Length() > MaxSpeed)
        {
            Rigidbody.InternalVelocity = Vector2.Normalize(Rigidbody.Velocity) * MaxSpeed;
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

    void FollowPath(Node currentNode)
    {
        var difference = (currentNode.Point - hitbox.Center).ToVector2();
        if (difference.LengthSquared() < 1f) return;

        Rigidbody.InternalVelocity.X += Vector2.Normalize(difference).X * Acceleration;

        if (difference.Y < 0 && Grounded && _touchingWall)
        {
            StateMachine.Set("jump");
        }
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        base.DrawDebug(spriteBatch, font);

        Debug.DrawRectangle(spriteBatch, WallCheckRect, _touchingWall ? Color.Green : Color.Red);
    }

    void WallCheck()
    {
        if (transform.flipX)
        {
            WallCheckRect = new Rectangle(hitbox.Left - _wallCheckSize.X, hitbox.Center.Y - _wallCheckSize.Y / 2, _wallCheckSize.X, _wallCheckSize.Y);
        }
        else
        {
            WallCheckRect = new Rectangle(hitbox.Right, hitbox.Center.Y - _wallCheckSize.Y / 2, _wallCheckSize.X, _wallCheckSize.Y);
        }

        if (grid.RectInsideObject(WallCheckRect, layer, out var levelObject, this))
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
        if (_playerCheckTimer > PlayerCheckInterval)
        {
            if (Player.Instance != null)
            {
                _target = Player.Instance.transform;
            }
            _playerCheckTimer = 0.0f;
        }
    }

    class Idle : State<FloralStalker>
    {
        public override void Enter()
        {
            base.Enter();
            _entity.Animator.Set("idle");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_entity.Pathfinder.Target != null)
            {
                if (_entity.hitbox.Center.Distance(_entity.Pathfinder.Target.position) < _entity.PlayerCheckRadius)
                {
                    _stateMachine.Set("aggro");
                }
            }
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            Debug.DrawCircle(spriteBatch, _entity.hitbox.Center, _entity.PlayerCheckRadius, Color.Blue, 1, 32);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    class Run : State<FloralStalker>
    {
        public override void Enter()
        {
            base.Enter();
            _entity.Animator.Set("run");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (!_entity.Grounded && _entity.Rigidbody.Velocity.Y > 0.1f)
            {
                _stateMachine.Set("fall");
            }
        }

        public override void IterateFrame(int frame)
        {
            if (frame == 0 || frame == 4 || frame == 2)
            {
                AudioManager.Get("fast_critter_footstep").Play(_entity.transform.position.ToVector2());
                float offset = -10;
                float flippedOffset = (_entity.transform.flipX ? -1 : 1) * offset;
                var offsetVector = new Vector2(flippedOffset, 0);
                _entity.ParticleSystem.Burst("small_dust", _entity.hitbox.Center.ToVector2() + offsetVector, 1, 3, 5f, 5f);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    class Jump : State<FloralStalker>
    {
        public override void Enter()
        {
            base.Enter();

            Debug.Log("jump!");
            _entity.Rigidbody.InternalVelocity.Y = _entity.JumpForce;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_entity.Rigidbody.Velocity.Y > 0.0f)
            {
                _stateMachine.Set("fall");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Pathfinder.HandlePathfinding(gameTime);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    class Fall : State<FloralStalker>
    {
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
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Pathfinder.HandlePathfinding(gameTime);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }

    class Aggro : State<FloralStalker>
    {
        float _counter = 0f;

        public override void Enter()
        {
            base.Enter();

            _entity.Animator.Set("aggro");
            _counter = 0f;
            _entity.SetFlipX(_entity.Pathfinder.Target.position.X < _entity.transform.position.X);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _counter += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_counter > _entity.Animator.CurrentAnimation.Length)
            {
                _stateMachine.Set("run");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Pathfinder.HandlePathfinding(gameTime);
        }
    }
}