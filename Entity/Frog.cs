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

public struct Frog(bool isLocalPlayer) : IBinarySerializable
{
    // Game State
    Vector2 _position = new Vector2(200, 200);
    HopDirection _hopDirection = HopDirection.None;
    byte _hopFrame = 0;
    byte _hopCooldown = 0;
    Vector2 _facingDirection = new Vector2(0, -1);
    bool _tonguing = false;
    byte _tongueFrame = 0;

    public bool IsLocalPlayer = isLocalPlayer;
    public int _tongueSegmentCount = 0;
    public Circle BoundingCircle { get => new Circle { Center = _position, Radius = _radius }; }

    const int _hopFrameLength = 10;
    const int _hopDelay = 15;
    const float _hopFrameRotation = MathHelper.Pi / (_hopFrameLength * 8);
    const int _tongueExtendFrameLength = 7;
    const int _tongueSegmentRadius = 6;
    const int _tongueSegmentSpacing = 6;
    const int _radius = 12;
    static readonly Vector2 _spriteCenter = new Vector2(16, 16);
    static readonly Vector2 _tongueSpriteCenter = new Vector2(4, 4);
    static readonly Texture2D _idleTexture = Game1.GameContent.Load<Texture2D>("frog");
    static readonly Texture2D _tongueSegmentTexture = Game1.GameContent.Load<Texture2D>("8pxcircle");

    public void Update(PlayerInputs inputs)
    {
        if (_hopDirection == HopDirection.None && !_tonguing)
        {
            if (inputs.DigitalInputs.HasFlag(DigitalInputs.Action))
            {
                _tonguing = true;
                _tongueFrame = 0;
            }
            else if (_hopCooldown <= 0)
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
        }

        if (_tonguing)
        {
            if (_tongueFrame < _tongueExtendFrameLength)
            {
                _tongueSegmentCount = _tongueFrame + 1;
            }
            else
            {
                _tongueSegmentCount = _tongueExtendFrameLength - ((_tongueFrame - _tongueExtendFrameLength) / 2);
            }

            _tongueFrame++;
            if (_tongueFrame > _tongueExtendFrameLength + _tongueExtendFrameLength * 2)
            {
                _tonguing = false;
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
            if (_hopFrame >= _hopFrameLength)
            {
                _hopDirection = HopDirection.None;
                _hopCooldown = _hopDelay;
            }
        }
        else if (_hopCooldown > 0)
        {
            _hopCooldown--;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var rotation = (float) Math.Atan2(_facingDirection.X, -_facingDirection.Y);
        foreach (var circle in GetTongueBoundingCircles())
        {
            spriteBatch.Draw(
                _tongueSegmentTexture,
                circle.Center,
                null,
                Color.Pink,
                rotation,
                _tongueSpriteCenter,
                1.5f,
                SpriteEffects.None,
                0
            );
        }
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

    public Circle[] GetTongueBoundingCircles()
    {
        Circle[] circles = new Circle[_tongueSegmentCount];
        for (var i = 0; i < _tongueSegmentCount; i++)
        {
            circles[i] = new Circle { 
                Center = _position + _facingDirection * (_tongueSegmentSpacing * i + 10),
                Radius = _tongueSegmentRadius 
            };
        }
        return circles;
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        byte hopDirectionByte = 0;

        reader.Read(ref _position);
        reader.Read(ref hopDirectionByte);
        reader.Read(ref _hopFrame);
        reader.Read(ref _hopCooldown);
        reader.Read(ref _tonguing);
        reader.Read(ref _tongueFrame);
        reader.Read(ref _facingDirection);

        _hopDirection = (HopDirection)hopDirectionByte;
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        byte hopDirectionByte = (byte)_hopDirection;

        writer.Write(in _position);
        writer.Write(in hopDirectionByte);
        writer.Write(in _hopFrame);
        writer.Write(in _hopCooldown);
        writer.Write(in _tonguing);
        writer.Write(in _tongueFrame);
        writer.Write(in _facingDirection);
    }
}
