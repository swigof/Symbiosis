using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using System;
using System.Text.Json.Serialization;
using static Symbiosis.Collision;

namespace Symbiosis.Entity;

public enum SpiderMovement : byte
{
    None,
    Going,
    Returning
}

public struct Spider(bool isLocalPlayer) : IBinarySerializable
{
    // Game State
    public Vector2 Position = Home;
    public SpiderMovement Movement = SpiderMovement.None;
    public Vector2 Target = Vector2.Zero;

    float _rotation = 0;
    Vector2 _direction = Vector2.Zero;
    float _movementDistanceSquared = 0;
    [JsonIgnore] public bool IsLocalPlayer = isLocalPlayer;
    [JsonIgnore] public Circle BoundingCircle { get => new Circle { Center = Position, Radius = _radius }; }

    const int _radius = 24;
    static readonly Vector2 _spriteCenter = new Vector2(16, 16);
    public static readonly Vector2 Home = new Vector2(400, 200);
    static readonly Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("spider");

    public void Update(PlayerInputs inputs)
    {
        if (Movement == SpiderMovement.None)
        {
            if (inputs.DigitalInputs.HasFlag(DigitalInputs.Click))
            {
                Movement = SpiderMovement.Going;
                Target = new Vector2(inputs.CursorPosition.X, inputs.CursorPosition.Y);
                var movementVector = Target - Home;
                _movementDistanceSquared = movementVector.LengthSquared();
                _direction = Vector2.Normalize(movementVector);
                _rotation = (float)Math.Atan2(_direction.X, -_direction.Y);
            }
        }

        if (Movement == SpiderMovement.Going)
        {
            Position += _direction * 4;

            if ((Position - Home).LengthSquared() >= _movementDistanceSquared)
            {
                Position = Target;
                _rotation = _rotation + MathHelper.Pi;
                Movement = SpiderMovement.Returning;
            }
        }
        else if (Movement == SpiderMovement.Returning)
        {
            Position -= _direction * 4;

            if ((Position - Target).LengthSquared() >= _movementDistanceSquared)
            {
                Movement = SpiderMovement.None;
                Position = Home;
                _rotation = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _idleTexture,
            Position,
            null,
            Color.White,
            _rotation,
            _spriteCenter,
            2,
            SpriteEffects.None,
            0
        );
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        byte movementByte = 0;

        reader.Read(ref Position);
        reader.Read(ref movementByte);
        reader.Read(ref Target);

        Movement = (SpiderMovement)movementByte;
        var movementVector = Target - Home;
        _movementDistanceSquared = movementVector.LengthSquared();
        _direction = Vector2.Normalize(movementVector);
        _rotation = 0;
        if (Movement != SpiderMovement.None)
        {
            _rotation = (float)Math.Atan2(_direction.X, -_direction.Y);
            if (Movement == SpiderMovement.Returning)
                _rotation = _rotation + MathHelper.Pi;
        }
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        byte movementByte = (byte)Movement;

        writer.Write(in Position);
        writer.Write(in movementByte);
        writer.Write(in Target);
    }
}
