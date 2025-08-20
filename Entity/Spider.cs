using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using System;
using System.Text.Json.Serialization;
using Symbiosis.Graphics;
using static Symbiosis.Manager.CollisionManager;

namespace Symbiosis.Entity;

public enum SpiderMovement : byte
{
    None,
    Going,
    Returning,
    Attacking
}

public struct Spider : IBinarySerializable
{
    // Game State
    public Vector2 Position = Home;
    public SpiderMovement Movement = SpiderMovement.None;
    public Vector2 Target = Vector2.Zero;
    public int AttackFrame = 0;

    [JsonIgnore] public float Rotation = 0;
    Vector2 _direction = Vector2.Zero;
    float _movementDistanceSquared = 0;
    Sprite _idle = Game1.Atlas.CreateSprite("spider-idle");
    AnimatedSprite _moveAnimation = Game1.Atlas.CreateAnimatedSprite("spider-move-animation");
    AnimatedSprite _attackAnimation = Game1.Atlas.CreateAnimatedSprite("spider-attack-animation");
    [JsonIgnore] public bool IsLocalPlayer = false;
    [JsonIgnore] public Circle BoundingCircle { get => new Circle { Center = Position, Radius = _radius }; }

    const int _radius = 17;
    public static readonly Vector2 Home = new Vector2(Game1.ResolutionWidth/2, Game1.ResolutionHeight/2);

    public Spider(bool isLocalPlayer)
    {
        IsLocalPlayer = isLocalPlayer;
        _idle.CenterOrigin();
        _moveAnimation.CenterOrigin();
        _attackAnimation.CenterOrigin();
    }
    
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
                Target -= _direction * 10;
                Rotation = (float)Math.Atan2(_direction.X, -_direction.Y);
            }
        }

        if (Movement == SpiderMovement.Going)
        {
            Position += _direction * 4;

            if ((Position - Home).LengthSquared() >= _movementDistanceSquared)
            {
                Position = Target;
                Movement = SpiderMovement.Attacking;
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
        else if (Movement == SpiderMovement.Attacking)
        {
            if(AttackFrame <= 15)
                AttackFrame++;
            else
            {
                AttackFrame = 0;
                _attackAnimation.Reset();
                Rotation += MathHelper.Pi;
                Movement = SpiderMovement.Returning;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Movement == SpiderMovement.None)
            _idle.Draw(spriteBatch, Position);
        else if (Movement is SpiderMovement.Going or SpiderMovement.Returning)
        {
            _moveAnimation.Update(gameTime);
            _moveAnimation.Rotation = Rotation;
            _moveAnimation.Draw(spriteBatch, Position);
        }
        else if (Movement is SpiderMovement.Attacking)
        {
            _attackAnimation.Update(gameTime);
            _attackAnimation.Rotation = Rotation;
            _attackAnimation.Draw(spriteBatch, Position);
        }
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        byte movementByte = 0;

        reader.Read(ref Position);
        reader.Read(ref movementByte);
        reader.Read(ref Target);
        reader.Read(ref AttackFrame);

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
        writer.Write(in AttackFrame);
    }
}
