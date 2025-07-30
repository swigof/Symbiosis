using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using System;
using static Symbiosis.Collision;

namespace Symbiosis.Entity;

internal enum HopDirection : byte
{
    None,
    Left,
    Right,
    Forward,
    Backward
}

public class Frog(bool isLocalPlayer) : IBinarySerializable
{
    // Game State
    Vector2 _position = new Vector2(200, 200);
    int _radius = 12;
    byte _hopDirectionByte = 0;
    HopDirection _hopDirection
    {
        get => (HopDirection)_hopDirectionByte;
        set => _hopDirectionByte = (byte)value;
    }
    int _hopFrame = 0;
    Vector2 _facingDirection = new Vector2(0, -1);

    Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("frog");
    public bool IsLocalPlayer = isLocalPlayer;
    public Circle BoundingCircle { get => new Circle { Center = _position, Radius = _radius }; }

    private const int _hopFrameLength = 10;
    private const int _hopFrameLengthWithDelay = _hopFrameLength + 15;
    private const float _hopFrameRotation = MathHelper.Pi / (_hopFrameLength * 8);
    private readonly Vector2 _spriteCenter = new Vector2(16, 16);

    public void Update(PlayerInputs inputs)
    {
        if (_hopDirection == HopDirection.None)
        {
            if (inputs.DigitalInputs.HasFlag(DigitalInputs.Down))
            {
                _hopDirection = HopDirection.Backward;
                _hopFrame = 0;
            }
            else if (inputs.DigitalInputs.HasFlag(DigitalInputs.Up))
            {
                _hopDirection = HopDirection.Forward;
                _hopFrame = 0;
            }
            else if (inputs.DigitalInputs.HasFlag(DigitalInputs.Left))
            {
                _hopDirection = HopDirection.Left;
                _hopFrame = 0;
            }
            else if (inputs.DigitalInputs.HasFlag(DigitalInputs.Right))
            {
                _hopDirection = HopDirection.Right;
                _hopFrame = 0;
            }
        }

        if (_hopDirection != HopDirection.None)
        {
            if (_hopFrame < _hopFrameLength)
            {
                var distance = MathHelper.CatmullRom(0, 1, 5, 0, _hopFrame / (float)_hopFrameLength);
                if (_hopDirection == HopDirection.Forward)
                {
                    _position += distance * _facingDirection;
                }
                else if (_hopDirection == HopDirection.Left)
                {
                    _facingDirection.Rotate(-_hopFrameRotation);
                    _position += distance * _facingDirection;
                }
                else if (_hopDirection == HopDirection.Right)
                {
                    _facingDirection.Rotate(_hopFrameRotation);
                    _position += distance * _facingDirection;
                }
                else if (_hopDirection == HopDirection.Backward)
                {
                    if (_hopFrame == _hopFrameLength / 2)
                    {
                        _facingDirection.Rotate(MathHelper.Pi);
                    }
                }
            }

            _hopFrame++;
            if (_hopFrame >= _hopFrameLengthWithDelay)
            {
                _hopDirection = HopDirection.None;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var rotation = (float) Math.Atan2(_facingDirection.X, -_facingDirection.Y);
        spriteBatch.Draw(
            _idleTexture,
            _position,
            null,
            Color.White,
            rotation,
            _spriteCenter,
            1,
            SpriteEffects.None,
            0
        );
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref _position);
        reader.Read(ref _radius);
        reader.Read(ref _hopDirectionByte);
        reader.Read(ref _hopFrame);
        reader.Read(ref _facingDirection);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in _position);
        writer.Write(in _radius);
        writer.Write(in _hopDirectionByte);
        writer.Write(in _hopFrame);
        writer.Write(in _facingDirection);
    }
}
