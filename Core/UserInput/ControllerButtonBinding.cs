using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class ControllerButtonBinding : InputBinding
{
    protected Buttons _button;

    public ControllerButtonBinding(Buttons button)
    {
        _button = button;
    }

    protected GamePadState _currentGamePadState;
    protected GamePadState _previousGamePadState;

    public override void Update()
    {
        _previousGamePadState = _currentGamePadState;
        _currentGamePadState = GamePad.GetState(PlayerIndex.One);

        Pressed = _currentGamePadState.IsButtonDown(_button) && _previousGamePadState.IsButtonUp(_button);
        Holding = _currentGamePadState.IsButtonDown(_button);
        Released = _currentGamePadState.IsButtonDown(_button) && _previousGamePadState.IsButtonUp(_button);
    }
}