using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class InputState
{
    public bool Pressed { get; protected set; } = false;
    public bool Holding { get; protected set; } = false;
    public bool Released { get; protected set; } = false;

    public Point Point { get; protected set; } = Point.Zero;

    protected KeyboardState _currentKeyboardState;
    protected KeyboardState _previousKeyboardState;

    protected Keys _key;

    public InputState(Keys key)
    {
        _key = key;
    }

    public virtual void Update()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        Pressed = _currentKeyboardState.IsKeyDown(_key) && !_previousKeyboardState.IsKeyDown(_key);
        Holding = _currentKeyboardState.IsKeyDown(_key);
        Released = !_currentKeyboardState.IsKeyDown(_key) && _previousKeyboardState.IsKeyDown(_key);
    }
}