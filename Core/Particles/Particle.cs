using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.Particles;

public class Particle
{
    readonly ParticleData _data;
    Vector2 _position;
    float _remainingLifespan;
    float _lifespanAmount;
    Color _color;
    float _opacity;
    float _speed;
    public bool finished { get; private set; } = false;

    float _timer = 0.0f;
    int _currentFrame = 0;

    Vector2 _velocity = Vector2.Zero;

    public Particle(ParticleData data, Vector2 position)
    {
        _data = data;
        _position = position;
        _remainingLifespan = data.lifespan;
        _lifespanAmount = 1f;
        _color = data.startColor;
        _opacity = data.startOpacity;
    }

    public void Update(GameTime gameTime)
    {
        _remainingLifespan -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_remainingLifespan <= 0)
        {
            finished = true;
            return;
        }

        _lifespanAmount = MathHelper.Clamp(_remainingLifespan / _data.lifespan, 0 , 1);
        _color = Color.Lerp(_data.endColor, _data.startColor, _lifespanAmount);
        _opacity = MathHelper.Clamp(MathHelper.Lerp(_data.endOpacity, _data.startOpacity, _lifespanAmount), 0, 1);
        _speed = MathHelper.Lerp(_data.endSpeed, _data.startSpeed, _lifespanAmount);

        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // update frames stuff;
        if (_data.frameCount > 0)
        {
            if (_timer >= _data.frameInterval)
            {
                if (_data.loop)
                {
                    _timer -= _data.frameInterval;
                    _currentFrame++;

                    // reset to first frame if looping
                    if (_currentFrame >= _data.frameCount)
                    {
                        _currentFrame = 0;
                    }
                }
                else if (_currentFrame < _data.frameCount - 1)
                {
                    _timer -= _data.frameInterval;
                    _currentFrame++;
                }
            }
        }

        _velocity += _data.gravity;

        _position += _data.direction * _speed;
        _position += _velocity;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Point point = new Point(_currentFrame * _data.frameSize.X, 0);
        Rectangle sourceRect = new Rectangle(point, _data.frameSize);
        spriteBatch.Draw(_data.texture, _position, sourceRect, _color * _opacity, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
    }
}