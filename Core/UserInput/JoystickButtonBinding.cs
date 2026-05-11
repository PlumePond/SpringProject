using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class JoystickButtonBinding : InputBinding
{
    protected int _button;

    public JoystickButtonBinding(int button)
    {
        _button = button;
    }

    protected JoystickState _currentJoystickState = Joystick.GetState(0);
    protected JoystickState _previousJoystickState = Joystick.GetState(0);

    public override void Update()
    {
        if (!Joystick.GetState(0).IsConnected) return;
        if (_previousJoystickState.Buttons == null) return;

        _previousJoystickState = _currentJoystickState;
        _currentJoystickState = Joystick.GetState(0);
        
        var prevButtonState = _previousJoystickState.Buttons[_button];
        var currentButtonState = _currentJoystickState.Buttons[_button];

        var wasDown = prevButtonState == ButtonState.Pressed;
        var isDown = currentButtonState == ButtonState.Pressed;

        Pressed = isDown && !wasDown;
        Holding = isDown;
        Released = !isDown && wasDown;
    }
}