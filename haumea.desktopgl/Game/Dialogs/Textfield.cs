using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Haumea.Geometric;

namespace Haumea.Dialogs
{
    public class Textfield : IDialogComponent, IDisposable
    {
        private static readonly Regex _lastWordRgx = new Regex(@"^[^ ]* *$|(?<= )[^ ]+ *$");

        public bool Focus { get; set; }

        private StringBuilder _userInput;
        private int _caretPos;
        private SpriteFont _font;
        private readonly int _width;

        public Textfield(SpriteFont font, int width)
        {
            _userInput = new StringBuilder();
            _caretPos = 0;
            _font = font;
            _width = width;

            Input.OnTextInput += HandleTextInput;

            // FIXME: Temporary - GetClipboard doesn't return anything the first time it's called (wtf)
            GetClipboard();
        }

        public void Update(InputState input, Vector2 v0)
        {
            if (input.IsActive(Modifiers.Control) && input.WentActive(Keys.Back))
            {
                string str =_userInput.ToString();
                string matchable = str.Substring(0, _caretPos);
                var match = _lastWordRgx.Match(matchable);
                _userInput = new StringBuilder(matchable.Substring(0, matchable.Length - match.Length));
                _userInput.Append(str.Substring(_caretPos));
                _caretPos -= match.Length;
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
                _userInput.Append(clipboard);
                _caretPos += clipboard.Length;
            }
            else if (input.WentActive(Keys.End))  _caretPos = _userInput.Length;
            else if (input.WentActive(Keys.Home)) _caretPos = 0;

            input.ConsumeKeyboard();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 v0)
        {
            string text = _userInput.ToString().Insert(_caretPos, "|");
            spriteBatch.DrawString(_font, text, v0, Color.Black);

            Vector2 dim = new Vector2(_width, _font.MeasureString(" ").Y) + 4 * Vector2.One;

            spriteBatch.Draw(new AABB(v0 - 2 * Vector2.One, v0 + dim).Borders(1), Color.Black);
        }
         
        // NOTE: Not calling this on a finished dialog will cause memory leaks,
        // ..... because the dialog will never be garbage collected.
        public void Dispose()
        {
            Input.OnTextInput -= HandleTextInput;
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

