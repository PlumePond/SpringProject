using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class JoystickStickBinding : InputBinding
{
    public JoystickStickBinding()
    {
        
    }

    protected JoystickState _currentJoystickState = Joystick.GetState(0);
    protected JoystickState _previousJoystickState = Joystick.GetState(0);

    public override void Update()
    {
        _previousJoystickState = _currentJoystickState;
        _currentJoystickState = Joystick.GetState(0);

        if (!_currentJoystickState.IsConnected) return;
        if (_previousJoystickState.Buttons == null) return;

        float threshold = 0.25f;

        var prevRawX = _previousJoystickState.Axes[0] / 32767f;
        var prevRawY = _previousJoystickState.Axes[1] / 32767f;
        var prevX = prevRawX > threshold ? 1 : prevRawX < -threshold ? -1 : 0;
        var prevY = prevRawY > threshold ? 1 : prevRawY < -threshold ? -1 : 0;

        var rawX = _currentJoystickState.Axes[0] / 32767f;
        var rawY = _currentJoystickState.Axes[1] / 32767f;
        var x = rawX > threshold ? 1 : rawX < -threshold ? -1 : 0;
        var y = rawY > threshold ? 1 : rawY < -threshold ? -1 : 0;

        Pressed = (prevX == 0 && x != 0) || (prevY == 0 && y != 0);
        Holding = x != 0 || y != 0;
        Released = !Holding && ((prevX != 0 && x == 0) || (prevY != 0 && y == 0));

        Point = new Point(x, y);
        Vector = new Vector2(rawX, rawY);
    }
}