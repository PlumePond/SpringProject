using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput; 

public static class Input
{
    static Dictionary<string, InputState> _inputStates = new Dictionary<string, InputState>();

    // add a new input state with the given name and key
    public static void AddState(string name, Keys key)
    {
        _inputStates[name] = new InputState(key);
    }

    public static void AddDirectionState(string name, Keys up, Keys down, Keys left, Keys right)
    {
        _inputStates[name] = new DirectionInputState(up, down, left, right);
    }

    public static void Update()
    {
        // update all input states
        foreach (var inputState in _inputStates.Values)
        {
            inputState.Update();
        }
    }

    public static InputState Get(string name)
    {
        return _inputStates[name];
    }
}