using System;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;

namespace Symbiosis.UI;

public struct ButtonState : IBinarySerializable
{
    public bool IsHighlighted;
    public bool IsPressed;
    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref IsHighlighted);
        reader.Read(ref IsPressed);
    }
    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in IsHighlighted);
        writer.Write(in IsPressed);
    }
}

public class Button
{
    static readonly SpriteFont _font = Game1.GameContent.Load<SpriteFont>("PublicPixel");
    static readonly Texture2D _buttonTexture = Game1.GameContent.Load<Texture2D>("button");
    static readonly Vector2 _buttonCenter = new Vector2(_buttonTexture.Width / 2f, _buttonTexture.Height / 2f);

    event EventHandler<EventArgs> _clicked;
    Vector2 _position;
    Rectangle _clickBound;
    string _text;

    public Button(Vector2 position, string text, EventHandler<EventArgs> onClick)
    {
        _text = text;
        _position = position;
        Vector2 textSize = _font.MeasureString(text);
        _clickBound = new Rectangle(
            (int)(position.X - textSize.X / 2), 
            (int)(position.Y - textSize.Y / 2), 
            (int)textSize.X, 
            (int)textSize.Y
        );
        _clicked += onClick;
    }

    public void Update(PlayerInputs inputs, PlayerInputs previousInputs, ref ButtonState state)
    {
        if (_clickBound.Contains(inputs.CursorPosition.X, inputs.CursorPosition.Y))
        {
            state.IsHighlighted = true;
            
            if (!previousInputs.DigitalInputs.HasFlag(DigitalInputs.Click) && 
                inputs.DigitalInputs.HasFlag(DigitalInputs.Click))
                state.IsPressed = true;
            
            else if (state.IsPressed && 
                     previousInputs.DigitalInputs.HasFlag(DigitalInputs.Click) && 
                     !inputs.DigitalInputs.HasFlag(DigitalInputs.Click))
            {
                _clicked?.Invoke(this, EventArgs.Empty);
                state.IsPressed = false;
            }
        }
        else
        {
            state.IsHighlighted = false;
            state.IsPressed = false;
        }
    }
    
    public void Draw(SpriteBatch spriteBatch, ButtonState state)
    {
        var center = new Vector2(_clickBound.Width / 2f, _clickBound.Height / 2f);
        var color = Color.White;
        if (state.IsHighlighted) color = Color.LightGray;
        if (state.IsPressed) color = Color.DarkGray;
        spriteBatch.Draw(_buttonTexture, _position, null, color, 0, _buttonCenter, 1, SpriteEffects.None, 0);
        spriteBatch.DrawString(_font, _text, _position, color, 0, center, 1, SpriteEffects.None, 0);
    }
}
