using System;
using Microsoft.Xna.Framework;

namespace haumea.desktopgl
{
    public interface ITextInput
    {
        event Action<TextInputEventArgs> TEvent;
    }

    public class TextInput
    {
        event Action<TextInputEventArgs> TEvent;

        public TextInput(GameWindow window)
        {
            window.TextInput += (sender, e) => 
            {
                var handle = TEvent;
                if (handle != null)
                {
                    handle(e);
                }
            };
        }
    }
}

