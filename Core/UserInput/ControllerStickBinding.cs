using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public enum ControllerStick
{
    Left,
    Right
}

public class ControllerStickBinding : InputBinding
{
    ControllerStick _stick;

    public ControllerStickBinding(ControllerStick stick)
    {
        _stick = stick;
    }

    protected GamePadState _currentGamePadState;
    protected GamePadState _previousGamePadState;

    public override void Update()
    {
        _previousGamePadState = _currentGamePadState;
        _currentGamePadState = GamePad.GetState(PlayerIndex.One);

        var prevStick = _stick == ControllerStick.Left ? _previousGamePadState.ThumbSticks.Left : _previousGamePadState.ThumbSticks.Right;
        var currentStick = _stick == ControllerStick.Left ? _currentGamePadState.ThumbSticks.Left : _currentGamePadState.ThumbSticks.Right;

        float threshold = 0.25f;

        var prevRawX = prevStick.X;
        var prevRawY = prevStick.Y;
        var prevX = prevRawX > threshold ? 1 : prevRawX < -threshold ? -1 : 0;
        var prevY = prevRawY > threshold ? 1 : prevRawY < -threshold ? -1 : 0;

        var rawX = currentStick.X;
        var rawY = currentStick.Y;
        var x = rawX > threshold ? 1 : rawX < -threshold ? -1 : 0;
        var y = rawY > threshold ? 1 : rawY < -threshold ? -1 : 0;

        Pressed = (prevX == 0 && x != 0) || (prevY == 0 && y != 0);
        Holding = x != 0 || y != 0;
        Released = !Holding && ((prevX != 0 && x == 0) || (prevY != 0 && y == 0));

        Point = new Point(x, -y);
        Vector = new Vector2(rawX, -rawY);
    }
}