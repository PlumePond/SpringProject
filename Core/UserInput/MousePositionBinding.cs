using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class MousePositionBinding : InputBinding
{
    protected MouseButton _button;

    MouseState _mouseState;

    public MousePositionBinding()
    {
        
    }

    public override void Update()
    {
        _mouseState = Mouse.GetState();

        Point = _mouseState.Position;
        Vector = _mouseState.Position.ToVector2();
    }
}