using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpringProject.Core.Debugging;
using SpringProject.Core.Editor;

namespace SpringProject.Core;

public class StateMachine
{
    public State CurrentState { get; private set; }
    public Dictionary<string, State> States { get; private set; }

    LevelObject _levelObject;

    public StateMachine(LevelObject levelObject)
    {
        _levelObject = levelObject;
        States = new Dictionary<string, State>();
    }

    public void Add(string name, State state)
    {
        if (States.ContainsKey(name))
        {
            throw new ArgumentException($"State with name '{name}' already exists for level object '{_levelObject.data.name}'.");
        }
        States.Add(name, state);
    }

    public void Set(string name)
    {
        if (!States.TryGetValue(name, out var newState))
        {
            throw new KeyNotFoundException($"State '{name}' not found for level object '{_levelObject.data.name}'.");
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

    public void Update(GameTime gameTime)
    {
        CurrentState?.Update(gameTime);
    }
}