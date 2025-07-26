using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Symbiosis;

public sealed class Player : IBinarySerializable
{
    // Game State
    public Vector2 Position;
    public int Radius;

    public Texture2D Texture;

    public Player()
    {
        Position = new Vector2(0, 0);
        Radius = 1;
        Texture = null;
    }

    public void Update()
    {

    }

    public void Draw()
    {

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
