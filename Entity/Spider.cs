using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using static Symbiosis.Collision;

namespace Symbiosis.Entity;

public class Spider(bool isLocalPlayer) : IBinarySerializable
{
    // Game State
    Vector2 _position = Vector2.Zero;
    int _radius = 12;

    Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("spider");
    public bool IsLocalPlayer = isLocalPlayer;
    public Circle BoundingCircle { get => new Circle { Center = _position, Radius = _radius }; }

    public void Update(PlayerInputs inputs)
    {
        if (inputs.DigitalInputs.HasFlag(DigitalInputs.Up))
            _position.Y--;
        if (inputs.DigitalInputs.HasFlag(DigitalInputs.Down))
            _position.Y++;
        if (inputs.DigitalInputs.HasFlag(DigitalInputs.Left))
            _position.X--;
        if (inputs.DigitalInputs.HasFlag(DigitalInputs.Right))
            _position.X++;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_idleTexture, _position, null, Color.White);
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref _position);
        reader.Read(ref _radius);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in _position);
        writer.Write(in _radius);
    }
}
