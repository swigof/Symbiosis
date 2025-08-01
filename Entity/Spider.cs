using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using System;
using static Symbiosis.Collision;

namespace Symbiosis.Entity;

internal enum SpiderMovement : byte
{
    None,
    Going,
    Returning
}

public class Spider(bool isLocalPlayer) : IBinarySerializable
{
    // Game State
    Vector2 _position = Home;
    SpiderMovement _movement = SpiderMovement.None;
    Vector2 _target = Vector2.Zero;

    Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("spider");
    float _rotation = 0;
    Vector2 _direction = Vector2.Zero;
    float _movementDistanceSquared = 0;
    public bool IsLocalPlayer = isLocalPlayer;
    public Circle BoundingCircle { get => new Circle { Center = _position, Radius = _radius }; }

    const int _radius = 24;
    static readonly Vector2 _spriteCenter = new Vector2(16, 16);
    public static readonly Vector2 Home = new Vector2(400, 200);

    public void Update(PlayerInputs inputs)
    {
        if (_movement == SpiderMovement.None)
        {
            if (inputs.DigitalInputs.HasFlag(DigitalInputs.Click))
            {
                _movement = SpiderMovement.Going;
                _target = new Vector2(inputs.CursorPosition.X, inputs.CursorPosition.Y);
                var movementVector = _target - _position;
                _movementDistanceSquared = movementVector.LengthSquared();
                _direction = Vector2.Normalize(movementVector);
                _rotation = (float)Math.Atan2(_direction.X, -_direction.Y);
            }
        }

        if (_movement == SpiderMovement.Going)
        {
            _position += _direction * 4;

            if ((_position - Home).LengthSquared() >= _movementDistanceSquared)
            {
                _position = _target;
                _rotation = _rotation + MathHelper.Pi;
                _movement = SpiderMovement.Returning;
            }
        }
        else if (_movement == SpiderMovement.Returning)
        {
            _position -= _direction * 4;

            if ((_position - _target).LengthSquared() >= _movementDistanceSquared)
            {
                _movement = SpiderMovement.None;
                _position = Home;
                _rotation = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _idleTexture,
            _position,
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

        reader.Read(ref _position);
        reader.Read(ref movementByte);
        reader.Read(ref _target);

        _movement = (SpiderMovement)movementByte;
        var movementVector = _target - _position;
        _movementDistanceSquared = movementVector.LengthSquared();
        _direction = Vector2.Normalize(movementVector);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        byte movementByte = (byte)_movement;

        writer.Write(in _position);
        writer.Write(in movementByte);
        writer.Write(in _target);
    }
}
