using System;
using System.Text.Json.Serialization;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using static Symbiosis.Manager.CollisionManager;

namespace Symbiosis.Entity;

public struct FrogEnemy : IBinarySerializable
{
    // Game state
    public Vector2 Position = Vector2.Zero;
    public bool Active = false;

    [JsonIgnore] public Circle BoundingCircle => new Circle { Center = Position, Radius = _radius };
    float _rotation = 0;
    
    const int _radius = 25;
    const int _speed = 1;
    static readonly AnimatedSprite _animation = Game1.Atlas.CreateAnimatedSprite("snake-move-animation");

    public FrogEnemy()
    {
        _animation.CenterOrigin();
    }

    public void Init(Vector2 position)
    {
        Active = true;
        Position = position;
    }

    public void Update(Vector2 frogPosition, bool isRespawning)
    {
        var direction = Vector2.Normalize(frogPosition - Position);
        if (isRespawning)
            direction *= -1;
        Position += direction * _speed;
        _rotation = (float) Math.Atan2(direction.X, -direction.Y);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        _animation.Update(gameTime);
        _animation.Rotation = _rotation;
        _animation.Draw(spriteBatch, Position);
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
