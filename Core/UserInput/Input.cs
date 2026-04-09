using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.UserInput; 

public static class Input
{
    static Dictionary<string, InputState> _inputStates = new Dictionary<string, InputState>();

    public static bool MouseHoverConsumed { get; private set; } = false;
    public static bool MousePressConsumed { get; private set; } = false;

    public static bool InputLocked { get; private set; } = false;

    // add a new input state with the given name and key
    public static void AddState(InputState state)
    {
        _inputStates.Add(state.Name, state);
    }

    public static bool StateExists(string name)
    {
        return _inputStates.ContainsKey(name);
    }

    public static void AddBinding(InputBinding binding, string name)
    {
        if (!StateExists(name))
        {
            _inputStates[name].AddBinding(binding);
        }
        else
        {
            Debug.Log($"Warning: Input State '{name}' already exists");
        }
    }

    public static void SetInputStates(List<InputState> inputStates)
    {
        foreach (var state in inputStates)
        {
            AddState(state);
        }
    }

    public static void Update()
    {  
        // at the start of each frame, reset the mouse hovering and mouse press consumed flags
        MousePressConsumed = false;
        MouseHoverConsumed = false;
        
        // update all input states
        foreach (var inputState in _inputStates.Values)
        {
            inputState.Update();
        }
    }

    public static InputState Get(string name)
    {
        name = name.ToLower();
        return _inputStates[name];
    }

    public static void ConsumeHover()
    {
        MouseHoverConsumed = true;
    }

    public static void ConsumePress()
    {
        MousePressConsumed = true;
    }

    public static void SetLocked(bool value)
    {
        InputLocked = value;
    }
}