using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core.Components;

public class StateMachine<T> : Component where T : LevelObject
{
    public State<T> CurrentState { get; private set; }
    public Dictionary<string, State<T>> States { get; private set; } = new Dictionary<string, State<T>>();
    public T Entity;

    public override void Start()
    {
        Entity = (T)LevelObject;
    }

    public void Add<S>(string name) where S : State<T>
    {
        S state = Activator.CreateInstance<S>();
        if (States.ContainsKey(name))
        {
            throw new ArgumentException($"State with name '{name}' already exists for '{LevelObject.data.name}'.");
        }
        States.Add(name, state);

        state.Initialize(Entity, this);
    }

    public void Set(string name)
    {
        if (!States.TryGetValue(name, out var newState))
        {
            throw new KeyNotFoundException($"State '{name}' not found for '{LevelObject.data.name}'.");
        }

        if (CurrentState == newState)
        {
            Debug.Log("New state is the same as the current one!");
            return;
        }

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public override void Update(GameTime gameTime)
    {
        CurrentState?.Update(gameTime);
    }

    public override void FixedUpdate(GameTime gameTime)
    {
        CurrentState?.FixedUpdate(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        CurrentState?.Draw(spriteBatch);
    }

    public override void DrawDebug(SpriteBatch spriteBatch)
    {
        CurrentState?.DrawDebug(spriteBatch);
    }

    public void IterateFrame(int frame)
    {
        CurrentState.IterateFrame(frame);
    }
}