using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Haumea
{
    /// <summary>
    /// Normalizes input handling for keyboard and mouse.
    /// Uses XNA's MouseState and KeyboardState and treates them the same. 
    /// Both keys and buttons are handled with the same three functions:
    ///     - IsActive(key|button)
    ///     - WentActive(key|button)
    ///     - WentInactive(key|button)
    /// </summary>
    public static class Input
    {
        private static MouseState _oldMouseState;
        private static KeyboardState _oldKbState;

        public static event Action<char> OnTextInput;

        public static void BindTextInput(GameWindow window)
        {
            window.TextInput += (sender, e) =>
            {
                var handle = OnTextInput;
                if (handle != null)
                {
                    handle(e.Character);
                }
            };
        }

        public static InputState GetState(Vector2 screenDim, Func<Vector2, Vector2> mouseTranslator)
        {
            MouseState newMouseState = Mouse.GetState();
            KeyboardState newKbState = Keyboard.GetState();

            Vector2 mousePos = newMouseState.Position.ToVector2();
            Vector2 mouseWorldPos = mouseTranslator(mousePos);
              
            var inputState = new InputState(newMouseState, _oldMouseState,
                newKbState, _oldKbState, mouseWorldPos, screenDim);
            _oldMouseState = newMouseState;
            _oldKbState    = newKbState;
                
            return inputState;
        }
    }

    public class InputState
    {
        private MouseState _mouseState;
        private MouseState _oldMouseState;

        private KeyboardState _kbState;
        private KeyboardState _oldKbState;

        public Vector2 ScreenMouse { get; }
        public Vector2 Mouse       { get; }
        public Vector2 MouseDelta  { get; }
        public int     ScrollWheel { get; }

        // This is semi-ridiculous, but it is the cleanest soloution I came up with to enable
        // dialogs to use coordinates relative to the screen center. Whatever.
        // This property also means that the screen dimension can be infered from InputState.
        // This isn't exactly logical (screen dim obv isn't a part of user input), but it
        // most likely doesn't matter? Update() methods should care about screen dim anyway,
        // except for the above mentioned use case.

        /// <summary>
        /// Gets the mouse position relative to the center of the screen.
        /// </summary>
        public Vector2 MouseRelativeToCenter { get; }

        /// <summary>
        /// Anyone with access to the InputState can "consume" the mouse
        /// by calling <code>#ConsumeMouse()</code>. A consumed mouse should be ignored
        /// by most entities. 
        /// </summary>
        public bool IsMouseConsumed { get; private set; }
        public bool IsKeyboardConsumed { get; private set; }

        public InputState(MouseState mouseState, MouseState oldMouseState,
            KeyboardState kbState, KeyboardState oldKbState,
            Vector2 mouseWorldPos, Vector2 screenDim)
        {            
            _mouseState    = mouseState;
            _oldMouseState = oldMouseState;
            _kbState       = kbState;
            _oldKbState    = oldKbState;

            Vector2 nextScreenMouse = _mouseState.Position.ToVector2();
            Vector2 nextMouse       = mouseWorldPos;
            Vector2 center          = screenDim / 2;

            MouseDelta       = nextScreenMouse - _oldMouseState.Position.ToVector2();
            Mouse            = nextMouse;
            ScreenMouse      = nextScreenMouse;
            Mouse            = mouseWorldPos;
            IsMouseConsumed  = false;
            ScrollWheel      = _mouseState.ScrollWheelValue - _oldMouseState.ScrollWheelValue; 
            MouseRelativeToCenter = nextScreenMouse - center;
        }

        public void ConsumeMouse()
        {
            IsMouseConsumed = true;
        }

        public void ConsumeKeyboard()
        {
            IsKeyboardConsumed = true;
        }

        #region keys

        public bool IsActive(Keys key, bool allowMods = true)
        {
            if (IsKeyboardConsumed) return false;
            return _kbState.IsKeyDown(key) && (allowMods || NoMods());
        }

        public bool WentActive(Keys key, bool allowMods = true)
        {
            if (IsKeyboardConsumed) return false;
            return _kbState.IsKeyDown(key) && !_oldKbState.IsKeyDown(key) && (allowMods || NoMods());
        }

        public bool WentInactive(Keys key, bool allowMods = true)
        {
            if (IsKeyboardConsumed) return false;
            return !_kbState.IsKeyDown(key) && _oldKbState.IsKeyDown(key) && (allowMods || NoMods());
        }

        #endregion

        #region modifiers

        public bool IsActive(Modifiers mod)
        {
            if (IsKeyboardConsumed) return false;

            switch (mod)
            {
            case Modifiers.Alt:
                return IsActive(Keys.LeftAlt) || IsActive(Keys.RightAlt);
            case Modifiers.Control:
                return IsActive(Keys.LeftControl) || IsActive(Keys.RightControl);
            case Modifiers.Shift:
                return IsActive(Keys.LeftShift) || IsActive(Keys.RightShift);
            case Modifiers.Super:
                return IsActive(Keys.LeftWindows) || IsActive(Keys.RightWindows);
            }

            return false;
        }

        public bool WentActive(Modifiers mod)
        {
            if (IsKeyboardConsumed) return false;

            switch (mod)
            {
            case Modifiers.Alt:
                return WentActive(Keys.LeftAlt) || WentActive(Keys.RightAlt);
            case Modifiers.Control:
                return WentActive(Keys.LeftControl) || WentActive(Keys.RightControl);
            case Modifiers.Shift:
                return WentActive(Keys.LeftShift) || WentActive(Keys.RightShift);
            case Modifiers.Super:
                return WentActive(Keys.LeftWindows) || WentActive(Keys.RightWindows);
            }

            return false;
        }

        public bool WentInactive(Modifiers mod)
        {
            if (IsKeyboardConsumed) return false;
            
            switch (mod)
            {
            case Modifiers.Alt:
                return WentInactive(Keys.LeftAlt) || WentInactive(Keys.RightAlt);
            case Modifiers.Control:
                return WentInactive(Keys.LeftControl) || WentInactive(Keys.RightControl);
            case Modifiers.Shift:
                return WentInactive(Keys.LeftShift) || WentInactive(Keys.RightShift);
            case Modifiers.Super:
                return WentInactive(Keys.LeftWindows) || WentInactive(Keys.RightWindows);
            }

            return false;
        }

        #endregion

        #region mouse

        public bool IsActive(Buttons button)
        {
            if (IsMouseConsumed) return false;

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
            if (IsMouseConsumed) return false;

            switch (button)
            {
            case Buttons.LeftButton:
                return _mouseState.LeftButton      == ButtonState.Pressed
                    && _oldMouseState.LeftButton   == ButtonState.Released;
            case Buttons.MiddleButton:
                return _mouseState.MiddleButton    == ButtonState.Pressed
                    && _oldMouseState.MiddleButton == ButtonState.Released;;
            case Buttons.RightButton:
                return _mouseState.RightButton     == ButtonState.Pressed
                    && _oldMouseState.RightButton  == ButtonState.Released;;
            case Buttons.XButton1:
                return _mouseState.XButton1        == ButtonState.Pressed
                    && _oldMouseState.XButton1     == ButtonState.Released;;
            case Buttons.XButton2:
                return _mouseState.XButton2        == ButtonState.Pressed
                    && _oldMouseState.XButton2     == ButtonState.Released;;
            }

            return false; // can't happen
        }

        public bool WentInactive(Buttons button)
        {
            if (IsMouseConsumed) return false;

            switch (button)
            {
            case Buttons.LeftButton:
                return _mouseState.LeftButton      == ButtonState.Released
                    && _oldMouseState.LeftButton   == ButtonState.Pressed;
            case Buttons.MiddleButton:
                return _mouseState.MiddleButton    == ButtonState.Released
                    && _oldMouseState.MiddleButton == ButtonState.Pressed;
            case Buttons.RightButton:
                return _mouseState.RightButton     == ButtonState.Released
                    && _oldMouseState.RightButton  == ButtonState.Pressed;
            case Buttons.XButton1:
                return _mouseState.XButton1        == ButtonState.Released
                    && _oldMouseState.XButton1     == ButtonState.Pressed;
            case Buttons.XButton2:
                return _mouseState.XButton2        == ButtonState.Released
                    && _oldMouseState.XButton2     == ButtonState.Pressed;
            }

            return false; // can't happen
        }

        #endregion

        public bool NoMods()
        {
            return !IsActive(Keys.LeftControl) && !IsActive(Keys.RightControl)
                && !IsActive(Keys.LeftAlt)     && !IsActive(Keys.RightAlt) 
                && !IsActive(Keys.LeftWindows) && !IsActive(Keys.RightWindows)
                && !IsActive(Keys.LeftShift)   && !IsActive(Keys.RightShift);
        }
    }

    public enum Buttons { LeftButton, MiddleButton, RightButton, XButton1, XButton2 }
    public enum Modifiers { Control, Shift, Alt, Super }
}

