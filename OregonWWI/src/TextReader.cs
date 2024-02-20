using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OregonWWI
{
    internal class TextReader
    {
        private StringBuilder _buffer = new StringBuilder();
        Keys[] pressedKeys = Array.Empty<Keys>();

        private int maxCharCount;

        public TextReader(int maxCharCount)
        {
            this.maxCharCount = maxCharCount;
        }

        public void Clear()
        {
            _buffer.Clear();
        }

        public void Update()
        {
            Keys[] currentPressedKeys = Keyboard.GetState().GetPressedKeys();

            string s = "";
            currentPressedKeys.ToList().ForEach(c => s += c.ToString());


            foreach (Keys key in currentPressedKeys)
            {
                if (!pressedKeys.Contains(key))
                {
                    string character = ConvertKeyToChar(key);
                    if (character == "a")
                    {
                        character = "" + (char)(key - (InputHelper.Down(Keys.LeftShift) || InputHelper.Down(Keys.RightShift) || currentPressedKeys.Any(k => (int)k == 16) ? 0 : -32));
                    }

                    if (_buffer.Length <= maxCharCount)
                    {
                        _buffer.Append(character);
                    }

                    if (key == Keys.Back && _buffer.Length != 0)
                    {
                        _buffer.Remove(_buffer.Length - 1, 1);
                    }
                }
            }
            pressedKeys = currentPressedKeys;
        }

        private string ConvertKeyToChar(Keys key)
        {
            return key switch
            {
                Keys.NumPad0 or Keys.D0 => '0',
                Keys.NumPad1 or Keys.D1 => '1',
                Keys.NumPad2 or Keys.D2 => '2',
                Keys.NumPad3 or Keys.D3 => '3',
                Keys.NumPad4 or Keys.D4 => '4',
                Keys.NumPad5 or Keys.D5 => '5',
                Keys.NumPad6 or Keys.D6 => '6',
                Keys.NumPad7 or Keys.D7 => '7',
                Keys.NumPad8 or Keys.D8 => '8',
                Keys.NumPad9 or Keys.D9 => '9',
                >= Keys.A and <= Keys.Z => 'a',
                _ => "",
            } + "";
        }

        public override string ToString()
        {
            return _buffer.ToString();
        }
    }
    /// <summary>
    /// A utility class for handling input states
    /// </summary>
    internal static class InputHelper
    {
        /// <summary>
        /// Gets the current state of the keyboard.
        /// </summary>
        public static KeyboardState KeyboardState { get; private set; }

        /// <summary>
        /// Gets the current state of the mouse.
        /// </summary>
        public static MouseState MouseState { get; private set; }

        /// <summary>
        /// Gets the current state of the touchpad
        /// </summary>
        public static TouchCollection Touches { get; private set; }



        /// <summary>
        /// Gets the state of the keyboard on the previous tick.
        /// </summary>
        public static KeyboardState PrevKeyboardState { get; private set; }

        /// <summary>
        /// Gets the state of the mouse on the previous tick.
        /// </summary>
        public static MouseState PrevMouseState { get; private set; }

        /// <summary>
        /// Gets the current state of the touchpad
        /// </summary>
        public static TouchCollection PrevTouches { get; private set; }



        /// <summary>
        /// Gets the state of the mouse on the previous tick.
        /// </summary>
        public static int DeltaScroll => MouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue;

        /// <summary>
        /// Gets the state of the mouse on the previous tick.
        /// </summary>
        public static Vector2 MouseLocation => MouseState.Position.ToVector2();

        /// <summary>
        /// Updates the input states by capturing the current keyboard and mouse states.
        /// This method should be called at the beginning of each frame
        /// </summary>
        public static void TickUpdate()
        {
            PrevKeyboardState = KeyboardState;
            PrevMouseState = MouseState;
            PrevTouches = Touches;

            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            Touches = TouchPanel.GetState();
        }

        /// <summary>
        /// Checks if a specific key has a rising edge (transition from released to pressed) in the current frame.
        /// </summary>
        /// <param name="key">The key to check for a rising edge.</param>
        /// <returns>True if the key has a rising edge, otherwise false.</returns>
        public static bool RisingEdge(Keys key)
        {
            return KeyboardState.IsKeyDown(key) && !PrevKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a specific key has a falling edge (transition from pressed to released) in the current frame.
        /// </summary>
        /// <param name="key">The key to check for a falling edge.</param>
        /// <returns>True if the key has a falling edge, otherwise false.</returns>
        public static bool FallingEdge(Keys key)
        {
            return !KeyboardState.IsKeyDown(key) && PrevKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the left or right mouse button has a rising edge (transition from released to pressed) in the current frame.
        /// </summary>
        /// <param name="IsLeftButton">True for the left mouse button, false for the right mouse button.</param>
        /// <returns>True if the specified mouse button has a rising edge, otherwise false.</returns>
        public static bool RisingEdge(MouseButton Button)
        {
            if (Button == MouseButton.Left)
            {
                return MouseState.LeftButton != PrevMouseState.LeftButton && MouseState.LeftButton == ButtonState.Pressed;
            }
            return MouseState.RightButton != PrevMouseState.RightButton && MouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if the left or right mouse button has a falling edge (transition from pressed to released) in the current frame.
        /// </summary>
        /// <param name="IsLeftButton">True for the left mouse button, false for the right mouse button.</param>
        /// <returns>True if the specified mouse button has a falling edge, otherwise false.</returns>
        public static bool FallingEdge(MouseButton Button)
        {
            if (Button == MouseButton.Left)
            {
                return MouseState.LeftButton != PrevMouseState.LeftButton && MouseState.LeftButton == ButtonState.Released;
            }
            return MouseState.RightButton != PrevMouseState.RightButton && MouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if a specific key is currently being held down.
        /// </summary>
        /// <param name="key">The key to check for being held down.</param>
        /// <returns>True if the key is currently held down, otherwise false.</returns>
        public static bool Down(Keys key)
        {
            return KeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the left or right mouse button is currently being held down.
        /// </summary>
        /// <param name="IsLeftButton">True for the left mouse button, false for the right mouse button.</param>
        /// <returns>True if the specified mouse button is currently held down, otherwise false.</returns>
        public static bool Down(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                return MouseState.LeftButton == ButtonState.Pressed;
            }
            return MouseState.RightButton == ButtonState.Pressed;
        }
    }

    /// <summary>
    /// Indicates the mouse button pressed
    /// </summary>
    internal enum MouseButton
    {
        Left = 0, Right = 1,
    }
}
