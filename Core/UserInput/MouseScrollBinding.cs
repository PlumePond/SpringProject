using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class MouseScrollBinding : InputBinding
{
    protected MouseButton _button;

    MouseState _mouseState;

    public MouseScrollBinding()
    {
        
    }

    public override void Update()
    {
        _mouseState = Mouse.GetState();

        Int = _mouseState.ScrollWheelValue;
    }
}