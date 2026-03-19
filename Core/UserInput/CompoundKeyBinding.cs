using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class CompoundKeyBinding : InputBinding
{
    protected KeyboardState _currentKeyboardState;
    protected KeyboardState _previousKeyboardState;
    
    Keys _up;
    Keys _down;
    Keys _left;
    Keys _right;

    public CompoundKeyBinding(Keys up, Keys down, Keys left, Keys right)
    {
        _up = up;
        _down = down;
        _left = left;
        _right = right;
    }

    public override void Update()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        bool upPressed = _currentKeyboardState.IsKeyDown(_up) && _previousKeyboardState.IsKeyUp(_up);
        bool downPressed = _currentKeyboardState.IsKeyDown(_down) && _previousKeyboardState.IsKeyUp(_down);
        bool leftPressed = _currentKeyboardState.IsKeyDown(_left) && _previousKeyboardState.IsKeyUp(_left);
        bool rightPressed = _currentKeyboardState.IsKeyDown(_right) && _previousKeyboardState.IsKeyUp(_right);

        bool upHolding = _currentKeyboardState.IsKeyDown(_up);
        bool downHolding = _currentKeyboardState.IsKeyDown(_down);
        bool leftHolding = _currentKeyboardState.IsKeyDown(_left);
        bool rightHolding = _currentKeyboardState.IsKeyDown(_right);

        bool upReleased = _currentKeyboardState.IsKeyUp(_up) && _previousKeyboardState.IsKeyDown(_up);
        bool downReleased = _currentKeyboardState.IsKeyUp(_down) && _previousKeyboardState.IsKeyDown(_down);
        bool leftReleased = _currentKeyboardState.IsKeyUp(_left) && _previousKeyboardState.IsKeyDown(_left);
        bool rightReleased = _currentKeyboardState.IsKeyUp(_right) && _previousKeyboardState.IsKeyDown(_right);

        Pressed = upPressed || downPressed || leftPressed || rightPressed;
        Holding = upHolding || downHolding || leftHolding || rightHolding;
        Released = upReleased || downReleased || leftReleased || rightReleased;

        int x = 0;
        int y = 0;

        if (upPressed) y -= 1;
        if (downPressed) y += 1;
        if (leftPressed) x -= 1;
        if (rightPressed) x += 1;

        Point = new Point(x, y);
    }
}