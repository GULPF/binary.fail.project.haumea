using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Haumea_Core
{
    public static class Input
    {
        private static MouseState _oldMouseState;
        private static KeyboardState _oldKbState;

        public static InputState GetState(Func<Vector2, Vector2> mouseTranslator)
        {
            MouseState newMouseState = Mouse.GetState();
            KeyboardState newKbState = Keyboard.GetState();

            Vector2 mousePos = newMouseState.Position.ToVector2();
            Vector2 mouseWorldPos = mouseTranslator(mousePos);
              
            InputState hstate = new InputState(newMouseState, _oldMouseState,
                newKbState, _oldKbState, mouseWorldPos);
            _oldMouseState = newMouseState;
            _oldKbState    = newKbState;
                
            return hstate;
        }
    }

    public struct InputState
    {
        private MouseState _mouseState;
        private MouseState _oldMouseState;

        private KeyboardState _kbState;
        private KeyboardState _oldKbState;

        public Point ScreenMouse { get; }
        public Vector2 Mouse { get; }

        public InputState(MouseState mouseState, MouseState oldMouseState,
            KeyboardState kbState, KeyboardState oldKbState, Vector2 mouseWorldPos)
        {
            _mouseState = mouseState;
            _oldMouseState = oldMouseState;
            _kbState = kbState;
            _oldKbState = oldKbState;

            ScreenMouse = _mouseState.Position;
            Mouse = mouseWorldPos;
        }

        public bool IsActive(Keys key)
        {
            return _kbState.IsKeyDown(key);
        }

        public bool WentActive(Keys key)
        {
            return _kbState.IsKeyDown(key) && !_oldKbState.IsKeyDown(key);
        }

        public bool WentInactive(Keys key)
        {
            return !_kbState.IsKeyDown(key) && _oldKbState.IsKeyDown(key);
        }

        public bool IsActive(Buttons button)
        {
            switch (button)
            {
            case Buttons.LeftButton:   return _mouseState.LeftButton   == ButtonState.Pressed;
            case Buttons.MiddleButton: return _mouseState.MiddleButton == ButtonState.Pressed;
            case Buttons.RightButton:  return _mouseState.RightButton  == ButtonState.Pressed;
            case Buttons.XButton1:     return _mouseState.XButton1     == ButtonState.Pressed;
            case Buttons.XButton2:     return _mouseState.XButton2     == ButtonState.Pressed;
            }

            return false; // can't happen
        }

        public bool WentActive(Buttons button)
        {
            switch (button)
            {
            case Buttons.LeftButton:
                return _mouseState.LeftButton   == ButtonState.Pressed
                    && _oldMouseState.LeftButton == ButtonState.Released;
            case Buttons.MiddleButton:
                return _mouseState.MiddleButton == ButtonState.Pressed
                    && _oldMouseState.MiddleButton == ButtonState.Released;;
            case Buttons.RightButton:
                return _mouseState.RightButton  == ButtonState.Pressed
                    && _oldMouseState.RightButton == ButtonState.Released;;
            case Buttons.XButton1:
                return _mouseState.XButton1     == ButtonState.Pressed
                    && _oldMouseState.XButton1 == ButtonState.Released;;
            case Buttons.XButton2:
                return _mouseState.XButton2     == ButtonState.Pressed
                    && _oldMouseState.XButton2 == ButtonState.Released;;
            }

            return false; // can't happen
        }

        public bool WentInactive(Buttons button)
        {
            switch (button)
            {
            case Buttons.LeftButton:
                return _mouseState.LeftButton   == ButtonState.Released
                    && _oldMouseState.LeftButton == ButtonState.Pressed;
            case Buttons.MiddleButton:
                return _mouseState.MiddleButton == ButtonState.Released
                    && _oldMouseState.MiddleButton == ButtonState.Pressed;
            case Buttons.RightButton:
                return _mouseState.RightButton  == ButtonState.Released
                    && _oldMouseState.RightButton == ButtonState.Pressed;
            case Buttons.XButton1:
                return _mouseState.XButton1     == ButtonState.Released
                    && _oldMouseState.XButton1 == ButtonState.Pressed;
            case Buttons.XButton2:
                return _mouseState.XButton2     == ButtonState.Released
                    && _oldMouseState.XButton2 == ButtonState.Pressed;
            }

            return false; // can't happen
        }
    }

    public enum Buttons { LeftButton, MiddleButton, RightButton, XButton1, XButton2 }
}

