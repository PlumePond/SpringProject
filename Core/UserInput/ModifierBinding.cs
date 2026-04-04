using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace SpringProject.Core.UserInput;

public class ModifierBinding : InputBinding
{
    Keys[] _keys;
    KeyboardState _currentKeyboardState;
    KeyboardState _previousKeyboardState;

    public ModifierBinding(Keys[] keys)
    {
        _keys = keys;
    }

    public override void Update()
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        // all modifer keys must be held down except the last one
        bool modifiersHeld = true;
        for (int i = 0; i < _keys.Length - 1; i++)
        {
            if (!_currentKeyboardState.IsKeyDown(_keys[i]))
            {
                modifiersHeld = false;
                break;
            }
        }

        // ensure only the keys in the binding are held
        Keys[] heldKeys = _currentKeyboardState.GetPressedKeys();
        foreach (var key in heldKeys)
        {
            if (!_keys.Contains(key))
            {
                modifiersHeld = false;
                break;
            }
        }

        // trigger key, (the final key)
        Keys triggerKey = _keys[_keys.Length - 1];
        bool triggerDown = _currentKeyboardState.IsKeyDown(triggerKey);
        bool triggerWasUp = _previousKeyboardState.IsKeyUp(triggerKey);

        Pressed = modifiersHeld && triggerDown && triggerWasUp;
        Holding = modifiersHeld && triggerDown;
        Released = !triggerWasUp && (!triggerDown || !modifiersHeld);
    }
}