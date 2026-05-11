using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using System;
using SpringProject.Core.Editor;
using SpringProject.Core.UserInput;
using SpringProject.Core.Audio;
using SpringProject.Core.Particles;
using SpringProject.Core.Components;
using NativeFileDialogCore;

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
    [Parameter("Swim Speed", 0f, 10f)] public float SwimSpeed = 0.15f;
    [Parameter("Water Friction")] public float WaterFriction = 0.90f;

    float _coyoteTimer = 0.0f;
    public int pouncesLeft { get; private set; } = 1;

    public bool HasCoyoteTime => _coyoteTimer < CoyoteTime;

    public static Player Instance;
    public ParticleSystem ParticleSystem;
    public Rigidbody Rigidbody;
    public StateMachine<Player> StateMachine;

    Rectangle _surfacedCheck;
    public bool Surfaced { get; private set; } = false;
    public bool IgnoreWaterTransition { get; set; } = false;
    public int WaterSurfaceY { get; private set; } = 0;

    int _watersTouching = 0;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Rigidbody = GetComponent<Rigidbody>();
        StateMachine = AddComponent<StateMachine<Player>>();

        GetComponent<Collider>().CollisionEnter += OnCollisionEnter;
        GetComponent<Collider>().CollisionExit += OnCollisionExit;

        Animator.Add("idle", new Animation(0, 4, 0.15f, true));
        Animator.Add("walk", new Animation(1, 8, 0.1f, true));
        Animator.Add("jump", new Animation(2, 6, 0.075f, false));
        Animator.Add("fall", new Animation(3, 1, 0.1f, false));
        Animator.Add("turn", new Animation(5, 3, 0.075f, false));
        Animator.Add("pounce", new Animation(8, 4, 0.1f, false));
        Animator.Add("slide", new Animation(9, 1, 0.1f, false));
        Animator.Add("swim_idle", new Animation(6, 4, 0.25f, true));
        Animator.Add("swim_move", new Animation(7, 4, 0.15f, true));
        Animator.Add("water_surface_idle", new Animation(10, 4, 0.15f, true));
        Animator.Add("dolphin_dive", new Animation(11, 6, 0.1f, false));
        Animator.Add("swim_surface_gasp", new Animation(12, 3, 0.15f, false));
        Animator.Add("swim_turn", new Animation(13, 3, 0.075f, false));
        Animator.Add("water_surface_move", new Animation(14, 4, 0.15f, true));

        Animator.Set("idle");

        Animator.IterateFrameEvent += StateMachine.IterateFrame;

        StateMachine.Add<Idle>("idle");
        StateMachine.Add<Walk>("walk");
        StateMachine.Add<Jump>("jump");
        StateMachine.Add<Fall>("fall");
        StateMachine.Add<Turn>("turn");
        StateMachine.Add<Pounce>("pounce");
        StateMachine.Add<Slide>("slide");
        StateMachine.Add<SwimIdle>("swim_idle");
        StateMachine.Add<SwimMove>("swim_move");
        StateMachine.Add<WaterSurfaceIdle>("water_surface_idle");
        StateMachine.Add<WaterSurfaceMove>("water_surface_move");
        StateMachine.Add<DolphinDive>("dolphin_dive"); 
        StateMachine.Add<SwimTurn>("swim_turn");
        
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

        var bubbleData = new ParticleData
        {
            texture = TextureManager.Get("particle_bubble"),
            frameCount = 1,
            frameInterval = 0.25f,
            frameSize = new Point(4, 4),
            loop = false,
            lifespan = 3,
            startSpeed = 12f
        };

        ParticleSystem.AddType("big_dust", bigDustData);
        ParticleSystem.AddType("small_dust", smallDustData);
        ParticleSystem.AddType("bubble", bubbleData);

        Instance = this;

        GetComponent<Sprite>().SetLayerDepth(0.2f);
        
        // random color
        // GetComponent<Sprite>()?.SetOverrideColor(new Color(Main.Random.NextSingle(), Main.Random.NextSingle(), Main.Random.NextSingle()));
    }

    void OnCollisionEnter(Collider collider)
    {
        if (collider.LevelObject.HasTag("water") && !IgnoreWaterTransition)
        {
            if (_watersTouching <= 0)
            {
                WaterSurfaceY = collider.LevelObject.hitbox.Top; // get the top of the water surface
                StateMachine.Set("swim_idle");
                var sound = AudioManager.Get("splash_small");
                sound.SetChannel("sfx");
                sound.Play();
            }
            
            _watersTouching++;
        }
    }

    void OnCollisionExit(Collider collider)
    {
        if (collider.LevelObject.HasTag("water") && !IgnoreWaterTransition)
        {
            _watersTouching--;

            if (_watersTouching <= 0)
            {
                StateMachine.Set("idle");
            }
        }
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

        SurfacedCheck();
    }

    void SurfacedCheck()
    {
        var size = new Point(hitbox.Width, 8);
        _surfacedCheck = new Rectangle(hitbox.Center.X - size.X / 2, hitbox.Top, size.X, size.Y);

        var objects = grid.GetObjectsInRect(_surfacedCheck, layer);
        foreach (var obj in objects)
        {
            if (obj.HasTag("water"))
            {
                Surfaced = false;
                return;
            }
        }
        Surfaced = true;
    }

    public override void DrawDebug(SpriteBatch spriteBatch, Font font)
    {
        base.DrawDebug(spriteBatch, font);
        Debug.DrawRectangle(spriteBatch, _surfacedCheck, Surfaced ? Color.Green : Color.Red);
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
            _entity.Rigidbody.GravityEnabled = true;
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

            if (!Input.Get("jump").Holding)
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

    class SwimIdle : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("swim_idle");
            _entity.Rigidbody.GravityEnabled = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.Get("move").Holding)
            {
                _stateMachine.Set("swim_move");
            }

            if (_entity.Surfaced && _entity.Rigidbody.Velocity.Y < -0.1f) 
            {
                _stateMachine.Set("water_surface_idle");
                _entity.Animator.Set("swim_surface_gasp");
                AudioManager.Get("gasp").Play();
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Rigidbody.InternalVelocity *= _entity.WaterFriction;
        }

        public override void IterateFrame(int frame)
        {
            if (frame == 4)
            {
                Vector2 pos = _entity.hitbox.Center.ToVector2();
                int dir = _entity.transform.flipX ? -1 : 1;
                pos += new Vector2(dir * 6, -2);
                _entity.ParticleSystem.Burst("bubble", pos, 1, 1, 3f, 3f);
            }
        }
    }

    class SwimMove : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("swim_move");
        }

        public override void Update(GameTime gameTime)
        {
            var moveDir = Input.Get("move").Point;
            if (Input.Get("move").Released)
            {
                _stateMachine.Set("swim_idle");
            }

            if (_entity.Surfaced && _entity.Rigidbody.Velocity.Y < -0.1f) 
            {
                _stateMachine.Set("water_surface_idle");
                _entity.Animator.Set("swim_surface_gasp");
                AudioManager.Get("gasp").Play();
            }

            if (moveDir.X > 0 && _entity.transform.flipX)
            {
                _stateMachine.Set("swim_turn");
            }
            else if (moveDir.X < 0 && !_entity.transform.flipX)
            {
                _stateMachine.Set("swim_turn");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Rigidbody.InternalVelocity += Input.Get("move").Point.ToVector2() * _entity.SwimSpeed;
            _entity.Rigidbody.InternalVelocity *= _entity.WaterFriction;
        }

        public override void IterateFrame(int frame)
        {
            if (frame == 1 || frame == 3)
            {
                var sound = AudioManager.Get("swim_kick");
                sound.SetChannel("sfx");
                sound.Play();

                // _entity.ParticleSystem.Burst("bubble", _entity.transform.position.ToVector2(), 1, 3, 1f, 1f);
                Vector2 pos = _entity.hitbox.Center.ToVector2();
                _entity.ParticleSystem.Burst("bubble", pos, 3, 6, 10f, 10f);
            }
            if (frame == 2 || frame == 4)
            {
                var sound = AudioManager.Get("swim_paddle");
                sound.SetChannel("sfx");
                sound.Play();
            }
        }
    }

    class WaterSurfaceIdle : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Queue("water_surface_idle", true);

            _entity.Rigidbody.InternalVelocity.Y = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            var moveDir = Input.Get("move").Point.ToVector2().X;
            if (moveDir > 0 && _entity.transform.flipX)
            {
                _entity.SetFlipX(false);
            }
            else if (moveDir < 0 && !_entity.transform.flipX)
            {
                _entity.SetFlipX(true);
            }

            if (Input.Get("jump").Pressed)
            {
                _stateMachine.Set("dolphin_dive");
            }

            if (!_entity.Surfaced)
            {
                _stateMachine.Set("swim_idle");
            }

            if (Input.Get("move").Holding)
            {
                _stateMachine.Set("water_surface_move");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            int bobHeight = 10;
            int height = (int)MathF.Max(_entity.transform.position.Y, _entity.WaterSurfaceY - bobHeight);
            _entity.SetPosition(new Point(_entity.transform.position.X, height));

            _entity.Rigidbody.InternalVelocity *= _entity.WaterFriction;

            _entity.Rigidbody.InternalVelocity.Y = Math.Max(_entity.Rigidbody.InternalVelocity.Y, 0f);
            _entity.Rigidbody.ExternalVelocity.Y = Math.Max(_entity.Rigidbody.ExternalVelocity.Y, 0f);

            Debug.Log($"Player Y: {_entity.transform.position.Y}, Surface Y: {_entity.WaterSurfaceY}");
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            Debug.DrawRectangle(spriteBatch, new Rectangle(new Point(_entity.hitbox.Center.X - 32, _entity.WaterSurfaceY), new Point(64, 3)), Color.Yellow);
        }
    }

    class DolphinDive : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("dolphin_dive");
            _entity.Rigidbody.InternalVelocity.Y = -3;
            
            var direction = _entity.transform.flipX ? -1 : 1;
            var force = new Vector2(2 * direction, 0);
            _entity.Rigidbody.InternalVelocity += force;

            var sound = AudioManager.Get("splash_small");
                sound.SetChannel("sfx");
                sound.Play();

            _entity.IgnoreWaterTransition = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!_entity.Surfaced)
            {
                _stateMachine.Set("swim_idle");
                var sound = AudioManager.Get("splash_small");
                sound.SetChannel("sfx");
                sound.Play();
            }

            if (_entity.Grounded)
            {
                _stateMachine.Set("idle");
                _entity._watersTouching = 0;
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Rigidbody.InternalVelocity.Y += 0.13f;
            _entity.Rigidbody.InternalVelocity.X *= 0.98f;
        }

        public override void Exit()
        {
            _entity.IgnoreWaterTransition = false;
        }
    }

    class SwimTurn : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("swim_turn");
        }

        public override void Update(GameTime gameTime)
        {
            var moveDir = Input.Get("move").Point;
            if (Input.Get("move").Released)
            {
                _stateMachine.Set("swim_idle");
                _entity.SetFlipX(!_entity.transform.flipX);
            }

            if (_entity.Surfaced && _entity.Rigidbody.Velocity.Y < -0.1f) 
            {
                _stateMachine.Set("water_surface_idle");
                _entity.Animator.Set("swim_surface_gasp");
                AudioManager.Get("gasp").Play();
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            _entity.Rigidbody.InternalVelocity += Input.Get("move").Point.ToVector2() * _entity.SwimSpeed;
            _entity.Rigidbody.InternalVelocity *= _entity.WaterFriction;
        }

        public override void IterateFrame(int frame)
        {
            if (frame >= _entity.Animator.CurrentAnimation.FrameCount)
            {
                _entity.SetFlipX(!_entity.transform.flipX);
                _stateMachine.Set("swim_idle");
            }
        }
    }

    class WaterSurfaceMove : State<Player>
    {
        public override void Enter()
        {
            _entity.Animator.Set("water_surface_move");

            _entity.Rigidbody.InternalVelocity.Y = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            var moveDir = Input.Get("move").Point.ToVector2().X;
            if (moveDir > 0 && _entity.transform.flipX)
            {
                _entity.SetFlipX(false);
            }
            else if (moveDir < 0 && !_entity.transform.flipX)
            {
                _entity.SetFlipX(true);
            }

            if (Input.Get("jump").Pressed)
            {
                _stateMachine.Set("dolphin_dive");
            }

            if (!_entity.Surfaced)
            {
                _stateMachine.Set("swim_idle");
            }

            if (Input.Get("move").Released)
            {
                _stateMachine.Set("water_surface_idle");
            }
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            int bobHeight = 10;
            int height = (int)MathF.Max(_entity.transform.position.Y, _entity.WaterSurfaceY - bobHeight);
            _entity.SetPosition(new Point(_entity.transform.position.X, height));

            var dir = Input.Get("move").Point.ToVector2();
            dir.Y = Math.Max(dir.Y, 0);
            _entity.Rigidbody.InternalVelocity += dir * _entity.SwimSpeed;
            _entity.Rigidbody.InternalVelocity *= _entity.WaterFriction;

            _entity.Rigidbody.InternalVelocity.Y = Math.Max(_entity.Rigidbody.InternalVelocity.Y, 0f);
            _entity.Rigidbody.ExternalVelocity.Y = Math.Max(_entity.Rigidbody.ExternalVelocity.Y, 0f);

            Debug.Log($"Player Y: {_entity.transform.position.Y}, Surface Y: {_entity.WaterSurfaceY}");
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            Debug.DrawRectangle(spriteBatch, new Rectangle(new Point(_entity.hitbox.Center.X - 32, _entity.WaterSurfaceY), new Point(64, 3)), Color.Yellow);
        }
    }
}