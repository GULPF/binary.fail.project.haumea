using System;

using Microsoft.Xna.Framework.Input;

namespace Haumea_Core
{
    public static class HKeyboard
    {
        private static KeyboardState _oldState = new KeyboardState();

        public static HKeyboardState GetState()
        {
            KeyboardState newState = Keyboard.GetState();
            HKeyboardState hstate = new HKeyboardState(newState, _oldState);
            _oldState = newState;
            return hstate;
        }
    }

    public class HKeyboardState
    {
        private KeyboardState _state;
        private KeyboardState _oldState;

        public HKeyboardState(KeyboardState state, KeyboardState oldState)
        {
            _state = state;
            _oldState = oldState;
        }

        public bool WentDown(Keys key)
        {
            return _state.IsKeyDown(key) && !_oldState.IsKeyDown(key);
        }

        public Keys[] GetPressedKeys ()
        {
            return _state.GetPressedKeys();
        }

        public bool IsKeyDown(Keys key)
        {
            return _state.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return _state.IsKeyUp(key);
        }
    }
}

