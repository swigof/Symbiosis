using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using static Symbiosis.Collision;

namespace Symbiosis;

public sealed class Player : IBinarySerializable
{
    // Game State
    public Vector2 Position;
    public int Radius;

    public Texture2D Texture;
    public Circle BoundingCircle
    {
        get => new Circle { Center = Position, Radius = Radius};
    }

    public Player()
    {
        Position = new Vector2(0, 0);
        Radius = 15;
        Texture = Game1.GameContent.Load<Texture2D>("player");
    }

    public void Update(SynchronizedInput<PlayerInputs> inputs)
    {
        if (inputs.Input.DigitalInputs.HasFlag(DigitalInputs.Up))
            Position.Y--;
        if (inputs.Input.DigitalInputs.HasFlag(DigitalInputs.Down))
            Position.Y++;
        if (inputs.Input.DigitalInputs.HasFlag(DigitalInputs.Left))
            Position.X--;
        if (inputs.Input.DigitalInputs.HasFlag(DigitalInputs.Right))
            Position.X++;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, Position, null, Color.White);
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref Position);
        reader.Read(ref Radius);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in Position);
        writer.Write(in Radius);
    }
}
