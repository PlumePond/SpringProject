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
using SpringProject.Core.UserInput;
using System.Runtime.InteropServices;
using SpringProject.Core.Audio;
using SpringProject.Core.Particles;
using SpringProject.Core.Commands;
using SpringProject.Core.Components;
using System.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace SpringProject.Core.Content.Types.LevelObjects;

public class Player : Entity
{
    [Parameter("Speed", 0f, 1f)] public float Speed = 0.45f;
    [Parameter("Internal Friction", 0f, 1f)] public float InternalFriction = 0.80f;
    [Parameter("External Friction", 0f, 1f)] public float ExternalFriction = 0.80f;
    [Parameter("Ice Friction", 0f, 1f)] public float IceFriction = 0.97f;
    [Parameter("Ice Speed", 0f, 1f)] public float IceSpeed = 0.08f;
    [Parameter("Jump Force", 0f, 8f)] public float JumpForce = 4.0f;
    [Parameter("Pounce Force X", 0f, 10f)] public float PounceForceX = 3.0f;
    [Parameter("Pounce Force Y", 0f, 10f)] public float PounceForceY = 2.0f;
    [Parameter("Slide Friction")] public float SlideFriction = 0.97f;
    [Parameter("God Mode")] public bool GodMode = false;
    [Parameter("Nickname")] public string Nickname = "";
    [Parameter("Coyote Time")] public float CoyoteTime = 0.15f;
    [Parameter("Jump Buffer")] public float JumpBuffer = 0.20f;
    [Parameter("Max Pounces")] public int MaxPounces = 1;

    float _coyoteTimer = 0.0f;
    public int pouncesLeft { get; private set; } = 1;

    public bool HasCoyoteTime => _coyoteTimer < CoyoteTime;

    public static Player Instance;
    public ParticleSystem ParticleSystem;
    public Rigidbody Rigidbody;
    public StateMachine<Player> StateMachine;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Rigidbody = GetComponent<Rigidbody>();
        StateMachine = AddComponent<StateMachine<Player>>();

        Animator.Add("idle", new Animation(0, 4, 0.15f, true));
        Animator.Add("walk", new Animation(1, 8, 0.1f, true));
        Animator.Add("jump", new Animation(2, 6, 0.075f, false));
        Animator.Add("fall", new Animation(3, 1, 0.1f, false));
        Animator.Add("turn", new Animation(5, 3, 0.075f, false));
        Animator.Add("pounce", new Animation(8, 4, 0.1f, false));
        Animator.Add("slide", new Animation(9, 1, 0.1f, false));

        Animator.Set("idle");

        Animator.IterateFrameEvent += StateMachine.IterateFrame;

        StateMachine.Add<Idle>("idle");
        StateMachine.Add<Walk>("walk");
        StateMachine.Add<Jump>("jump");
        StateMachine.Add<Fall>("fall");
        StateMachine.Add<Turn>("turn");
        StateMachine.Add<Pounce>("pounce");
        StateMachine.Add<Slide>("slide");
        
        StateMachine.Set("idle");

        ParticleSystem = AddComponent<ParticleSystem>();

        var bigDustData = new ParticleData
        {
            texture = TextureManager.Get("particle_dust_big"),
            frameCount = 5,
            frameInterval = 0.2f,
            frameSize = new Point(12, 12),
            loop = false,
            startSpeed = 0.5f
        };

        var smallDustData = new ParticleData
        {
            texture = TextureManager.Get("particle_dust_small"),
            frameCount = 3,
            frameInterval = 0.25f,
            frameSize = new Point(12, 12),
            loop = false,
            startSpeed = 0.1f
        };

        ParticleSystem.AddType("big_dust", bigDustData);
        ParticleSystem.AddType("small_dust", smallDustData);

        Instance = this;
    }

    public override void Update(GameTime gameTime)
    {
        if (Grounded)
        {
            _coyoteTimer = 0.0f;
            pouncesLeft = MaxPounces;
        }
        else
        {
            _coyoteTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        base.FixedUpdate(gameTime);

        if (MathF.Abs(Rigidbody.InternalVelocity.X) < 0.05f)
        {
            Rigidbody.InternalVelocity.X = 0f;
        }

        if (Rigidbody.InternalVelocity.Y > 10.0f)
        {
            Rigidbody.InternalVelocity.Y = 10.0f;
        }
    }

    public void ApplyFriction()
    {
        var groundFriction = FootstepMaterial == Material.Ice ? IceFriction : InternalFriction;
        Rigidbody.InternalVelocity.X *= groundFriction;

        Rigidbody.ExternalVelocity.X *= Grounded ? groundFriction : ExternalFriction;
    }

    public void ApplyMovement()
    {
        Point moveInput = Input.Get("move").Point;
        var speed = FootstepMaterial == Material.Ice ? IceSpeed : Speed;
        Rigidbody.InternalVelocity.X += moveInput.X * speed;
    }

    class Idle : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("idle");
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.Get("move").Holding)
            {
                _stateMachine.Set("walk");
            }

            if (Input.Get("jump").Pressed && _entity.HasCoyoteTime)
            {
                _stateMachine.Set("jump");
            }

            if (!_entity.Grounded)
            {
                _stateMachine.Set("fall");
            }

            if (Input.Get("pounce").Pressed)
            {
                _stateMachine.Set("pounce");
                _entity.Rigidbody.InternalVelocity += new Vector2(0, -_entity.PounceForceY);
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyFriction();
        }
    }

    class Walk : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("walk");
        }

        public override void Update(GameTime gameTime)
        {
            Point moveInput = Input.Get("move").Point;

            if (moveInput.X > 0 && _entity.transform.flipX)
            {
                _stateMachine.Set("turn");
            }
            else if (moveInput.X < 0 && !_entity.transform.flipX)
            {
                _stateMachine.Set("turn");
            }

            if (Input.Get("jump").Pressed && _entity.Grounded)
            {
                _stateMachine.Set("jump");
            }

            if (Input.Get("move").Released)
            {
                _stateMachine.Set("idle");
            }

            if (!_entity.Grounded)
            {
                _stateMachine.Set("fall");
            }

            if (Input.Get("pounce").Pressed)
            {
                _stateMachine.Set("pounce");
                _entity.Rigidbody.InternalVelocity += new Vector2(0, -_entity.PounceForceY);
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyMovement();
            _entity.ApplyFriction();
        }

        public override void IterateFrame(int frame)
        {
            Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 6);

            if (frame == 2)
            {
                var hopSound = AudioManager.Get("hop_left");
                hopSound.SetChannel("sfx");
                hopSound.Play();
                _entity.ParticleSystem.Burst("small_dust", pos, 2, 3, 5f, 0f);
            }
            else if (frame == 6)
            {
                var hopSound = AudioManager.Get("hop_right");
                hopSound.SetChannel("sfx");
                hopSound.Play();
                _entity.ParticleSystem.Burst("small_dust", pos, 2, 3, 5f, 0f);
            }
        }
    }

    class Jump : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("jump");
            _entity.Rigidbody.InternalVelocity.Y = -_entity.JumpForce;

            var jumpSound = AudioManager.Get("jump");
            jumpSound.SetChannel("sfx");
            jumpSound.Play();

            var footSound = AudioManager.Get($"step_{_entity.FootstepMaterial.ToString().ToLower()}");
            footSound.SetChannel("sfx");
            footSound.Play();
            Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 5);
            _entity.ParticleSystem.Burst("big_dust", pos, 2, 3, 10f, 7f);
        }

        public override void Update(GameTime gameTime)
        {
            Point moveInput = Input.Get("move").Point;

            if (moveInput.X > 0)
            {
                _entity.SetFlipX(false);
            }
            else if (moveInput.X < 0)
            {
                _entity.SetFlipX(true);
            }

            if (_entity.Rigidbody.Velocity.Y > 0f)
            {
                _stateMachine.Set("fall");
            }

            if (Input.Get("jump").Released)
            {
                _entity.Rigidbody.InternalVelocity.Y *= 0.3f;
            }

            if (Input.Get("pounce").Pressed)
            {
                _stateMachine.Set("pounce");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyMovement();
            _entity.ApplyFriction();
        }
    }

    class Fall : State<Player>
    {
        float _jumpBufferTimer = 0f;

        public override void Enter()
        {
            _entity.Animator.Set("fall");
            _jumpBufferTimer = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.Get("jump").Pressed)
            {
                _jumpBufferTimer = _entity.JumpBuffer;
            }

            _jumpBufferTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_entity.Grounded)
            {
                if (_jumpBufferTimer > 0)
                {
                    _stateMachine.Set("jump");
                }

                _stateMachine.Set("idle");
                var landSound = AudioManager.Get($"step_{_entity.FootstepMaterial.ToString().ToLower()}");
                landSound.SetChannel("sfx");
                landSound.Play();

                Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 10);
                _entity.ParticleSystem.Burst("small_dust", pos, 4, 5, 15f, 1f);
            }

            if (Input.Get("pounce").Pressed && _entity.pouncesLeft > 0)
            {
                _stateMachine.Set("pounce");
                _entity.Rigidbody.InternalVelocity += new Vector2(0, -_entity.PounceForceY);
            }

            if (Input.Get("jump").Pressed && _entity.HasCoyoteTime)
            {
                _stateMachine.Set("jump");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyMovement();
            _entity.ApplyFriction();
        }
    }

    class Turn : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("turn");
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.Get("jump").Pressed && _entity.HasCoyoteTime)
            {
                _stateMachine.Set("jump");
            }

            if (!_entity.Grounded)
            {
                _stateMachine.Set("fall");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyMovement();
            _entity.ApplyFriction();
        }

        public override void IterateFrame(int frame)
        {
            if (frame >= _entity.Animator.CurrentAnimation.FrameCount)
            {
                _entity.SetFlipX(!_entity.transform.flipX);
                _stateMachine.Set("idle");
            }
        }
    }
    
    class Pounce : State<Player>
    {
        float _elapsedTime;

        const float MinPounceTime = 0.2f;

        public override void Enter()
        {
            _entity.Animator.Set("pounce");
            var direction = _entity.transform.flipX ? -1 : 1;
            var force = new Vector2(_entity.PounceForceX * direction, 0);
            _entity.Rigidbody.InternalVelocity += force;
            _elapsedTime = 0f;
            _entity.pouncesLeft--;
            
            var pounceSound = AudioManager.Get("pounce");
                pounceSound.SetChannel("sfx");
                pounceSound.Play();
        }

        public override void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_entity.Grounded && _elapsedTime >= MinPounceTime)
            {
                _stateMachine.Set("slide");
                var footSound = AudioManager.Get($"step_{_entity.FootstepMaterial.ToString().ToLower()}");
                footSound.SetChannel("sfx");
                footSound.Play();

                var landSound = AudioManager.Get("slide_land");
                landSound.SetChannel("sfx");
                landSound.Play();

                Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 10);
                _entity.ParticleSystem.Burst("small_dust", pos, 4, 5, 15f, 1f);
            }

            if (Input.Get("jump").Pressed)
            {
                _stateMachine.Set("fall");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Rigidbody.ExternalVelocity.X *= _entity.SlideFriction;
            _entity.Rigidbody.InternalVelocity.X *= _entity.SlideFriction;
        }
    }

    class Slide : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("slide");
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.Get("jump").Pressed && _entity.HasCoyoteTime)
            {
                _stateMachine.Set("jump");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Rigidbody.ExternalVelocity.X *= _entity.SlideFriction;
            _entity.Rigidbody.InternalVelocity.X *= _entity.SlideFriction;
        }
    }
}