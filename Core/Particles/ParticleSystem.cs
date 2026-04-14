using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpringProject.Core.Particles;

public class ParticleSystem
{
    public List<Particle> Particles;
    public Dictionary<string, ParticleData> ParticleTypes;

    public ParticleSystem()
    {
        ParticleTypes = new Dictionary<string, ParticleData>();
        Particles = new List<Particle>();
    }

    // adds a particle type to the particle system using particle data
    public void AddType(string name, ParticleData data)
    {
        if (!ParticleTypes.ContainsKey(name))
        {
            ParticleTypes.Add(name, data);
        }
        else
        {
            throw new Exception($"Particle with name '{name}' already exists.");
        }
    }

    // creates an instant burst of particles. useful for explosions, dust, things of the sort
    public void Burst(string name, Vector2 position, int min, int max, float rangeX, float rangeY)
    {
        if (ParticleTypes.TryGetValue(name, out ParticleData data))
        {
            int count = Main.Random.Next(min, max + 1);
        
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = new Vector2(Main.Random.NextSingle() - 0.5f, Main.Random.NextSingle() - 0.5f);

                offset.X *= rangeX;
                offset.Y *= rangeY;
                
                AddParticle(data, position + offset);
            }
        }
        else
        {
            throw new Exception($"Cannot find particle '{name}'.");
        }
    }

    // adds a particle to the system at a position
    public void AddParticle(ParticleData data, Vector2 position)
    {
        Particle particle = new Particle(data, position);
        Particles.Add(particle);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var particle in Particles)
        {
            particle.Update(gameTime);
        }

        // remove any particles whose lifecycles are complete
        Particles.RemoveAll(p => p.finished);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var particle in Particles)
        {
            particle.Draw(spriteBatch);
        }
    }
}