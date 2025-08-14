using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using System;
using System.Text.Json.Serialization;
using static Symbiosis.Manager.CollisionManager;

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

    [JsonIgnore] public float Rotation = 0;
    Vector2 _direction = Vector2.Zero;
    float _movementDistanceSquared = 0;
    [JsonIgnore] public bool IsLocalPlayer = isLocalPlayer;
    [JsonIgnore] public Circle BoundingCircle { get => new Circle { Center = Position, Radius = _radius }; }

    const int _radius = 24;
    static readonly Vector2 _spriteCenter = new Vector2(32, 32);
    public static readonly Vector2 Home = new Vector2(Game1.ResolutionWidth/2, Game1.ResolutionHeight/2);
    public static readonly Circle HomeBoundingCircle = new Circle { Center = Home, Radius = 31 };
    static readonly Vector2 _homeCenter = new Vector2(32, 32);
    static readonly Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("spider");
    static readonly Texture2D _homeTexture = Game1.GameContent.Load<Texture2D>("hole");

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
                Rotation = (float)Math.Atan2(_direction.X, -_direction.Y);
            }
        }

        if (Movement == SpiderMovement.Going)
        {
            Position += _direction * 4;

            if ((Position - Home).LengthSquared() >= _movementDistanceSquared)
            {
                Position = Target;
                Rotation += MathHelper.Pi;
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
                Rotation = 0;
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
            Rotation,
            _spriteCenter,
            1,
            SpriteEffects.None,
            0
        );
        spriteBatch.Draw(
            _homeTexture,
            Home,
            null,
            Color.Black,
            0,
            _homeCenter,
            1,
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
        Rotation = 0;
        if (Movement != SpiderMovement.None)
        {
            Rotation = (float)Math.Atan2(_direction.X, -_direction.Y);
            if (Movement == SpiderMovement.Returning)
                Rotation += MathHelper.Pi;
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
