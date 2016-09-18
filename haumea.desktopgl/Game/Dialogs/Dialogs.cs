using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Haumea.Rendering;
using Haumea.Geometric;
using Haumea.Components;

namespace Haumea.Dialogs
{
    public interface IDialog : IView
    {
        // Indicates if the dialog should be terminated
        bool Terminate { get; set; }

        // How big is the dialog?
        // Like paradox games, I will limit my self to static dialog sizes,
        // so this is known at compile time.
        Vector2 Dimensions { get; }

        // The offset from the center of the screen.
        // Note that the dialog doesn't have to think about the position,
        // it just have to use it in the draw.
        Vector2 Offset { get; set; }
    }

    // Null-object for IDialog
    internal class NullDialog : IDialog
    {
        public bool    Terminate   { get; set; } = false;
        public Vector2 Dimensions  { get;      } = Vector2.Zero;
        public Vector2 Offset      { get; set; } = Vector2.Zero;


        public void LoadContent(ContentManager content) {}
        public void Update(InputState input) {}
        public void Draw(SpriteBatch spriteBatch, Renderer renderer) {}
    }
        
    internal static class DialogHelpers
    {
        public static AABB CalculateBox(IDialog dialog)
        {
            Vector2 corner = dialog.Offset - dialog.Dimensions / 2;
            var aabb = new AABB(corner, corner + dialog.Dimensions);
            return aabb;
        }
    }

    public class Confirm : IDialog
    {
        public bool    Terminate  { get; set; } = false;
        public Vector2 Dimensions { get; }      = new Vector2(250, 100);
        public Vector2 Offset     { get; set; }

        private Action _onSuccess;
        private Action _onFail;
        private readonly string _msg;

        private SpriteFont _font;

        public Confirm(string msg, Action onSuccess)
        {
            _onSuccess = onSuccess;
            _onFail    = () => {};
            _msg = msg;
        }

        public Confirm(string msg, Action onSuccess, Action onFail)
        {
            _onSuccess = onSuccess;
            _onFail    = onFail;
            _msg = msg;
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("LogFont");
        }

        public void Update(InputState input)
        {
            if (input.WentActive(Keys.Y))
            {
                Terminate = true;
                _onSuccess();
            } else if (input.WentActive(Keys.N))
            {
                Terminate = true;
                _onFail();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            AABB box = DialogHelpers.CalculateBox(this).Move(spriteBatch.GetScreenDimensions() / 2);
            spriteBatch.Draw(box, Color.WhiteSmoke, 1, Color.Black);
            spriteBatch.DrawString(_font, _msg, box.TopLeft + new Vector2(10, 10), Color.Black);
        }
    }

    public class Prompt : IDialog
    {
        private static Regex _lastWordRgx = new Regex(@" [^ ]*$");

        public bool    Terminate  { get; set; }
        public Vector2 Dimensions { get;      } = new Vector2(250, 100);
        public Vector2 Offset     { get; set; }

        private SpriteFont _font;
        private StringBuilder _userInput;
        private Action<string> _onDone;
        private int _caretPos;

        public Prompt(Action<string> onDone)
        {
            _onDone = onDone;
            _userInput = new StringBuilder();
            _caretPos = 0;
            Input.OnTextInput += HandleTextInput;

            // FIXME: Temporary - GetClipboard doesn't return anything the first time it's called (wtf)
            GetClipboard();
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("LogFont");
        }

        public void Update(InputState input)
        {
            if (input.WentActive(Keys.Enter))
            {
                Terminate = true;
                UnsubscribeUserInput();
            }
            else if (input.WentActive(Modifiers.Control) && input.WentActive(Keys.Back))
            {
                string str =_userInput.ToString();
                _lastWordRgx.Replace(str, "");
                _userInput = new StringBuilder(str);
            }
            else if (input.WentActive(Keys.Back) && _caretPos > 0)
            {
                _userInput.Remove(_caretPos - 1, 1);
                DecCaret();
            }
            else if (input.WentActive(Keys.Left))  DecCaret();
            else if (input.WentActive(Keys.Right)) IncCaret();
            else if (input.WentActive(Keys.Delete) && _caretPos < _userInput.Length && _userInput.Length > 0)
            {
                _userInput.Remove(_caretPos, 1);
            }
            else if (input.IsActive(Modifiers.Control) && input.WentActive(Keys.V))
            {
                string clipboard = GetClipboard();
                Console.WriteLine("|" + clipboard + "|");
                _userInput.Append(clipboard);
                _caretPos += clipboard.Length;
            }

            input.ConsumeKeyboard();
        }

        public void Draw(SpriteBatch spriteBatch, Renderer renderer)
        {
            AABB box = DialogHelpers.CalculateBox(this).Move(spriteBatch.GetScreenDimensions() / 2);
            spriteBatch.Draw(box, Color.WhiteSmoke, 1, Color.Black);

            string inputed = _userInput.ToString().Insert(_caretPos, "|");
            string text = "Input: " + inputed;
            Vector2 textPos = box.TopLeft + new Vector2(10, 10);
            spriteBatch.DrawString(_font, text, textPos, Color.Black);

            float offset = _font.MeasureString("Input: ").X;
            Vector2 dim    = new Vector2(200, _font.MeasureString(" ").Y + 2);

            spriteBatch.Draw(new AABB(textPos + new Vector2(offset, 0), textPos + dim).Borders(1), Color.Black);
        }

        private void IncCaret()
        {
            _caretPos = Math.Min(_userInput.Length, _caretPos + 1);
        }

        private void DecCaret()
        {
            _caretPos = Math.Max(0, _caretPos - 1);
        }

        private void HandleTextInput(char c)
        {
            _userInput.Insert(_caretPos, c);
            IncCaret();
        }

        // NOTE: Not calling this on a finished dialog will cause memory leaks,
        // ..... because the dialog will never be garbage collected.
        private void UnsubscribeUserInput()
        {
            Input.OnTextInput -= HandleTextInput;
        }

        private static String GetClipboard()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                using(var process = new Process())
                {
                    process.StartInfo.FileName = "xclip";
                    process.StartInfo.Arguments = "-selection c -o";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;

                    process.Start();

                    StringBuilder clipboard = new StringBuilder();

                    while (!process.HasExited) {
                        clipboard.Append(process.StandardOutput.ReadToEnd());
                    }

                    return clipboard.ToString();    
                }
            }
            else
            {
                return "";
            }
        }
    }
}