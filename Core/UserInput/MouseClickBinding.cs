using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public enum MouseButton
{
    Left,
    Right,
    Middle,
    Button4,
    Button5
}

public class MouseClickBinding : InputBinding
{
    protected MouseButton _button;

    MouseState _mouseState;
    MouseState _prevMouseState;

    public MouseClickBinding(MouseButton button)
    {
        _button = button;
    }

    public override void Update()
    {
        _prevMouseState = _mouseState;
        _mouseState = Mouse.GetState();

        ButtonState buttonState = ButtonState.Released;
        ButtonState prevButtonState = ButtonState.Released;

        switch (_button)
        {
            case MouseButton.Left: 
                buttonState = _mouseState.LeftButton;
                prevButtonState = _prevMouseState.LeftButton;
                break;
            case MouseButton.Right:
                buttonState = _mouseState.RightButton;
                prevButtonState = _prevMouseState.RightButton;
                break;
            case MouseButton.Middle:
                buttonState = _mouseState.MiddleButton;
                prevButtonState = _prevMouseState.MiddleButton;
                break;
            case MouseButton.Button4:
                buttonState = _mouseState.XButton1;
                prevButtonState = _prevMouseState.XButton1;
                break;
            case MouseButton.Button5:
                buttonState = _mouseState.XButton2;
                prevButtonState = _prevMouseState.XButton2;
                break;
            default:
                buttonState = ButtonState.Released;
                prevButtonState = ButtonState.Released;
                break;
        }

        if (!Main.Graphics.Viewport.Bounds.Contains(_mouseState.Position))
        {
            return;
        }

        if (!Main.Instance.IsActive)
        {
            return;
        }
 
        Pressed = buttonState == ButtonState.Pressed && prevButtonState == ButtonState.Released;
        Holding = buttonState == ButtonState.Pressed;
        Released = buttonState == ButtonState.Released && prevButtonState == ButtonState.Pressed;
    }
}