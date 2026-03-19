using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class KeyBinding : InputBinding
{
    protected Keys _key;

    public KeyBinding(Keys key)
    {
        _key = key;
    }

    protected KeyboardState _currentKeyboardState;
    protected KeyboardState _previousKeyboardState;

    public override void Update()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        Pressed = _currentKeyboardState.IsKeyDown(_key) && _previousKeyboardState.IsKeyUp(_key);
        Holding = _currentKeyboardState.IsKeyDown(_key);
        Released = _currentKeyboardState.IsKeyUp(_key) && _previousKeyboardState.IsKeyDown(_key);
    }
}