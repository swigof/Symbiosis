using System.Text.Json.Serialization;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Symbiosis.Manager.CollisionManager;

namespace Symbiosis.Entity;

public struct EggEnemy
{
    public bool Active;
    public Vector2 RelativePosition;
}

public struct EggEnemyCluster : IBinarySerializable
{
    // Game state
    public Vector2 Position = Vector2.Zero;
    public EggEnemy[] EggEnemies = new EggEnemy[8];
    public bool Active = false;

    [JsonIgnore] public Circle BoundingCircle => new Circle { Center = Position, Radius = _radius };
    const int _radius = 8;
    const int _eggEnemyRadius = 2;
    const int _speed = 1;
    static readonly Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("8pxcircle");
    static readonly Vector2 _spriteCenter = new Vector2(1, 1);

    public EggEnemyCluster() { }

    public void Init(Vector2 position)
    {
        Active = true;
        Position = position;
        for (var i = 0; i < EggEnemies.Length; i++)
        {
            EggEnemies[i].Active = true;
            EggEnemies[i].RelativePosition.X = Game1.Random.NextFloat() * (_radius - _eggEnemyRadius);
            EggEnemies[i].RelativePosition.Y = Game1.Random.NextFloat() * (_radius - _eggEnemyRadius);
        }
    }
    
    public Circle[] GetEggEnemyBoundingCircles()
    {
        var circles = new Circle[EggEnemies.Length];
        for (int i = 0; i < EggEnemies.Length; i++)
            circles[i] = new Circle { Center = Position + EggEnemies[i].RelativePosition, Radius = _eggEnemyRadius };
        return circles;
    }

    public void Update()
    {
        Position += Vector2.Normalize(Spider.Home - Position) * _speed;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (var i = 0; i < EggEnemies.Length; i++)
        {
            if (EggEnemies[i].Active)
            {
                spriteBatch.Draw(
                    _idleTexture,
                    Position + EggEnemies[i].RelativePosition,
                    null,
                    Color.Red,
                    0,
                    _spriteCenter,
                    0.25f,
                    SpriteEffects.None,
                    0
                );
            }
        }
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref Position);
        reader.Read(ref Active);
        for (var i = 0; i < EggEnemies.Length; i++)
        {
            reader.Read(ref EggEnemies[i].Active);
            reader.Read(ref EggEnemies[i].RelativePosition);
        }
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in Position);
        writer.Write(in Active);
        for (var i = 0; i < EggEnemies.Length; i++)
        {
            writer.Write(in EggEnemies[i].Active);
            writer.Write(in EggEnemies[i].RelativePosition);
        }
    }
}
