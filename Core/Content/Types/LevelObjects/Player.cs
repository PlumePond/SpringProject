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

namespace SpringProject.Core.Content.Types.LevelObjects;

public class Player : Entity
{
    [Parameter("Speed", 0f, 1f)] public float Speed = 0.4f;
    [Parameter("Internal Friction", 0f, 1f)] public float InternalFriction = 0.8f;
    [Parameter("External Friction", 0f, 1f)] public float ExternalFriction = 0.8f;
    [Parameter("Ice Friction", 0f, 1f)] public float IceFriction = 0.95f;
    [Parameter("Ice Speed", 0f, 1f)] public float IceSpeed = 0.4f;
    [Parameter("Jump Force", 0f, 8f)] public float JumpForce = 4.0f;
    [Parameter("God Mode")] public bool GodMode = false;
    [Parameter("Nickname")] public string Nickname = "";

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

        Animator.Set("idle");

        Animator.IterateFrameEvent += StateMachine.IterateFrame;

        StateMachine.Add<Idle>("idle");
        StateMachine.Add<Walk>("walk");
        StateMachine.Add<Jump>("jump");
        StateMachine.Add<Fall>("fall");
        StateMachine.Add<Turn>("turn");
        
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

        ApplyFriction();
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

            if (Input.Get("jump").Pressed && _entity.Grounded)
            {
                _stateMachine.Set("jump");
            }

            if (!_entity.Grounded)
            {
                _stateMachine.Set("fall");
            }
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
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyMovement();
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
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyMovement();
        }
    }

    class Fall : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("fall");
        }

        public override void Update(GameTime gameTime)
        {
            if (_entity.Grounded)
            {
                _stateMachine.Set("idle");
                var landSound = AudioManager.Get($"step_{_entity.FootstepMaterial.ToString().ToLower()}");
                landSound.SetChannel("sfx");
                landSound.Play();

                Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 10);
                _entity.ParticleSystem.Burst("small_dust", pos, 4, 5, 15f, 1f);
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.ApplyMovement();
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
            if (Input.Get("jump").Pressed && _entity.Grounded)
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
}