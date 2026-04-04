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

namespace SpringProject.Core.Content.Types.LevelObjects;

public class Player : Entity
{
    const float SPEED = 25.0f;
    const float FRICTION = 0.8f;
    const float JUMP_FORCE = 4f;

    public static Player Instance;
    public static ParticleSystem ParticleSystem;

    public override void Initialize(LevelObjectData data, Grid grid, Point position)
    {
        base.Initialize(data, grid, position);

        Animator.Add("idle", new Animation(0, 4, 0.15f, true));
        Animator.Add("walk", new Animation(2, 8, 0.1f, true));
        Animator.Add("jump", new Animation(3, 6, 0.075f, false));
        Animator.Add("fall", new Animation(4, 1, 0.1f, false));

        Animator.Set("idle");

        StateMachine.Add("idle", new Idle(this));
        StateMachine.Add("walk", new Walk(this));
        StateMachine.Add("jump", new Jump(this));
        StateMachine.Add("fall", new Fall(this));
        
        StateMachine.Set("idle");

        ParticleSystem = new ParticleSystem();

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

        ParticleSystem.Add("big_dust", bigDustData);
        ParticleSystem.Add("small_dust", smallDustData);

        Instance = this;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Velocity.X *= FRICTION;
        ParticleSystem.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        ParticleSystem.Draw(spriteBatch);
    }

    class Idle : State
    {
        public Idle(Entity entity) : base(entity)
        {
        }

        public override void Enter()
        {
            _animator.Set("idle");
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

    class Walk : State
    {
        public Walk(Entity entity) : base(entity)
        {
        }

        public override void Enter()
        {
            _animator.Set("walk");
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Point moveInput = Input.Get("move").Point;

            _entity.Velocity.X += moveInput.X * SPEED * deltaTime;

            if (moveInput.X > 0)
            {
                _entity.SetFlipX(false);
            }
            else if (moveInput.X < 0)
            {
                _entity.SetFlipX(true);
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

        public override void IterateFrame(int frame)
        {
            Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 6);

            if (frame == 2)
            {
                AudioManager.Get("hop_left").Play();
                ParticleSystem.Burst("small_dust", pos, 2, 3, 5f, 0f);
            }
            else if (frame == 6)
            {
                AudioManager.Get("hop_right").Play();
                ParticleSystem.Burst("small_dust", pos, 2, 3, 5f, 0f);
            }
        }
    }

    class Jump : State
    {
        public Jump(Entity entity) : base(entity)
        {
        }

        public override void Enter()
        {
            _animator.Set("jump");
            _entity.Velocity.Y = -JUMP_FORCE;
            AudioManager.Get("jump").Play();
            AudioManager.Get($"step_{_entity.FootstepMaterial.ToString().ToLower()}").Play();

            Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 5);
            ParticleSystem.Burst("big_dust", pos, 2, 3, 10f, 7f);
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Point moveInput = Input.Get("move").Point;

            _entity.Velocity.X += moveInput.X * SPEED * deltaTime;

            if (moveInput.X > 0)
            {
                _entity.SetFlipX(false);
            }
            else if (moveInput.X < 0)
            {
                _entity.SetFlipX(true);
            }

            if (_entity.Velocity.Y > 0.1f)
            {
                _stateMachine.Set("fall");
            }

            if (Input.Get("jump").Released)
            {
                _entity.Velocity.Y *= 0.3f;
            }
        }
    }

    class Fall : State
    {
        public Fall(Entity entity) : base(entity)
        {
        }

        public override void Enter()
        {
            _animator.Set("fall");
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Point moveInput = Input.Get("move").Point;

            _entity.Velocity.X += moveInput.X * SPEED * deltaTime;

            if (_entity.Grounded)
            {
                _stateMachine.Set("idle");
                AudioManager.Get($"step_{_entity.FootstepMaterial.ToString().ToLower()}").Play();

                Vector2 pos = new Vector2(_entity.transform.position.X, _entity.transform.position.Y + 10);
                ParticleSystem.Burst("small_dust", pos, 4, 5, 15f, 1f);
            }
        }
    }
}