using System.Text.Json.Serialization;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
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
    Vector2 _direction = Vector2.Zero;
    
    const int _radius = 16;
    const int _eggEnemyRadius = 2;
    const float _speed = 0.25f;
    static readonly AnimatedSprite _animation = Game1.Atlas.CreateAnimatedSprite("ant-move-animation");

    public EggEnemyCluster()
    {
        _animation.CenterOrigin();
    }

    public void Init(Vector2 position)
    {
        Active = true;
        Position = position;
        _direction = Vector2.Normalize(Spider.Home - Position);
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
        Position += _direction * _speed;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        for (var i = 0; i < EggEnemies.Length; i++)
        {
            if (EggEnemies[i].Active)
            {
                _animation.Update(gameTime);
                _animation.Draw(spriteBatch, Position + EggEnemies[i].RelativePosition);
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
