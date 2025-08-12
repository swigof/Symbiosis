using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Symbiosis.Entity;

public struct FrogEnemy : IBinarySerializable
{
    // Game state
    public Vector2 Position = Vector2.Zero;
    public bool Active = false;

    const int _radius = 8;
    const int _speed = 1;
    static readonly Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("8pxcircle");
    static readonly Vector2 _spriteCenter = new Vector2(4, 4);

    public FrogEnemy() { }

    public void Init()
    {
        Active = true;
        Position = Vector2.Zero;
    }

    public void Update(Vector2 frogPosition)
    {
        Position += Vector2.Normalize(frogPosition - Position) * _speed;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _idleTexture,
            Position,
            null,
            Color.Red,
            0,
            _spriteCenter,
            1,
            SpriteEffects.None,
            0
        );
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref Position);
        reader.Read(ref Active);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in Position);
        writer.Write(in Active);
    }
}
