using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpringProject.Core.Debugging;

namespace SpringProject.Core.UserInput; 

public static class Input
{
    static Dictionary<string, InputState> _inputStates = new Dictionary<string, InputState>();

    public static bool MouseHoverConsumed = false;
    public static bool MousePressConsumed = false;

    // add a new input state with the given name and key
    public static void AddState(string name, params InputBinding[] bindings)
    {
        _inputStates[name] = new InputState();
        
        foreach (InputBinding binding in bindings)
        {
            _inputStates[name].AddBinding(binding);
        }
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

    public static void SetInputStates(Dictionary<string, List<InputBinding>> inputStates)
    {
        foreach (var (name, bindings) in inputStates)
        {
            AddState(name, bindings.ToArray());
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
}