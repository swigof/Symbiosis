using System;
using System.Text.Json.Serialization;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Graphics;
using static Symbiosis.Manager.CollisionManager;

namespace Symbiosis.Entity;

public struct FrogEnemy : IBinarySerializable
{
    // Game state
    public Vector2 Position = Vector2.Zero;
    public bool Active = false;
    public bool Dead = false;
    public Vector2 DeadDirection = Vector2.One;

    [JsonIgnore] public Circle MainBoundingCircle => new Circle { Center = Position, Radius = 64 };
    [JsonIgnore] public Circle HeadBounds => new Circle
    {
        Center = Position + _direction * 60, 
        Radius = 4
    };
    AnimatedSprite _animation = Game1.Atlas.CreateAnimatedSprite("snake-move-animation");
    float _rotation = 0;
    Vector2 _direction = Vector2.Zero;
    
    const int _speed = 1;
    static readonly int[] _boundingCircleOffsets = [-38, -15, 8, 31, 41];

    public FrogEnemy()
    {
        _animation.CenterOrigin();
    }

    public void Init(Vector2 position)
    {
        Active = true;
        Position = position;
        Dead = false;
    }
    
    public Circle[] GetBoundingCircles()
    {
        Circle[] circles = new Circle[_boundingCircleOffsets.Length];
        for (var i = 0; i < circles.Length; i++)
        {
            circles[i] = new Circle {
                Center = Position + _direction * _boundingCircleOffsets[i],
                Radius = 23
            };
        }
        return circles;
    }

    public void Update(Vector2 frogPosition, bool isRespawning)
    {
        if (!Dead)
        {
            _direction = Vector2.Normalize(frogPosition - Position);
            if (isRespawning)
                _direction *= -1;
            Position += _direction * _speed;
            _rotation = (float)Math.Atan2(_direction.X, -_direction.Y);
        }
        else
        {
            Position += DeadDirection * 5;
            _rotation += 0.1f;
        }
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
        reader.Read(ref Dead);
        reader.Read(ref DeadDirection);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in Position);
        writer.Write(in Active);
        writer.Write(in Dead);
        writer.Write(in DeadDirection);
    }
}
