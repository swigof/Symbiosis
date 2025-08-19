using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Input;
using System;
using System.Text.Json.Serialization;
using Symbiosis.Graphics;
using static Symbiosis.Manager.CollisionManager;

namespace Symbiosis.Entity;

public enum HopDirection : byte
{
    None,
    Left,
    Right,
    Forward,
    Backward
}

public struct Frog : IBinarySerializable
{
    // Game State
    public Vector2 Position = new Vector2(Spider.Home.X, Spider.Home.Y - 50);
    public HopDirection HopDirection = HopDirection.None;
    public byte HopFrame = 0;
    public byte HopCooldown = 0;
    public Vector2 FacingDirection = new Vector2(0, -1);
    public float Rotation = 0f; 
    public bool Tonguing = false;
    public byte TongueFrame = 0;
    public bool Respawning = false;
    public byte RespawnFrame = 0;
    public Vector2 DeadDirection = Vector2.One;

    int _tongueSegmentCount = 0;
    Sprite _idle = Game1.Atlas.CreateSprite("frog-idle");
    Sprite _hop = Game1.Atlas.CreateSprite("frog-move-3");
    AnimatedSprite _flipAnimation = Game1.Atlas.CreateAnimatedSprite("frog-flip-animation");
    Sprite _tongueSegmentTexture = Game1.Atlas.CreateSprite("tongue");
    [JsonIgnore] public bool IsLocalPlayer = false;
    [JsonIgnore] public Circle BoundingCircle { get => new Circle { Center = Position, Radius = _radius }; }
    [JsonIgnore] public Circle TongueBoundingCircle =>
        new Circle
        {
            Center = Position + FacingDirection * (_tongueSegmentSpacing * ((_tongueExtendFrameLength + 1) / 2f) + 5), 
            Radius = _tongueSegmentRadius + _tongueSegmentSpacing * (_tongueExtendFrameLength / 2)
        };

    const int _hopFrameLength = 10;
    const int _hopDelay = 15;
    const float _hopFrameRotation = MathHelper.Pi / (_hopFrameLength * 8);
    const int _tongueExtendFrameLength = 7; // also the length of tongue at full extension
    const int _tongueSegmentRadius = 4;
    const int _tongueSegmentSpacing = 4;
    const int _radius = 6;
    const int _respawnFrameLength = 180;

    public Frog(bool isLocalPlayer)
    {
        IsLocalPlayer = isLocalPlayer;
        _idle.CenterOrigin();
        _hop.CenterOrigin();
        _flipAnimation.CenterOrigin();
        _tongueSegmentTexture.CenterOrigin();
    }

    public void Update(PlayerInputs inputs)
    {
        if (Respawning)
        {
            Position += DeadDirection * 5;
            Rotation += 0.5f;
            RespawnFrame++;
            if (RespawnFrame == _respawnFrameLength)
            {
                Reset();
                Respawning = false;
                RespawnFrame = 0;
            }
            return;
        }
        
        if (HopDirection == HopDirection.None && !Tonguing)
        {
            if (inputs.DigitalInputs.HasFlag(DigitalInputs.Action))
            {
                Tonguing = true;
                TongueFrame = 0;
            }
            else if (HopCooldown <= 0)
            {
                if (inputs.DigitalInputs.HasFlag(DigitalInputs.Down))
                {
                    HopDirection = HopDirection.Backward;
                    HopFrame = 0;
                }
                else if (inputs.DigitalInputs.HasFlag(DigitalInputs.Up))
                {
                    HopDirection = HopDirection.Forward;
                    HopFrame = 0;
                }
                else if (inputs.DigitalInputs.HasFlag(DigitalInputs.Left))
                {
                    HopDirection = HopDirection.Left;
                    HopFrame = 0;
                }
                else if (inputs.DigitalInputs.HasFlag(DigitalInputs.Right))
                {
                    HopDirection = HopDirection.Right;
                    HopFrame = 0;
                }
            }
        }

        if (Tonguing)
        {
            if (TongueFrame < _tongueExtendFrameLength)
            {
                _tongueSegmentCount = TongueFrame + 1;
            }
            else
            {
                _tongueSegmentCount = _tongueExtendFrameLength - ((TongueFrame - _tongueExtendFrameLength) / 2);
            }

            TongueFrame++;
            if (TongueFrame > _tongueExtendFrameLength + _tongueExtendFrameLength * 2)
            {
                Tonguing = false;
            }
        }

        if (HopDirection != HopDirection.None)
        {
            if (HopFrame < _hopFrameLength)
            {
                var distance = MathHelper.CatmullRom(0, 1, 5, 0, HopFrame / (float)_hopFrameLength);
                if (HopDirection == HopDirection.Forward)
                {
                    Position += distance * FacingDirection;
                }
                else if (HopDirection == HopDirection.Left)
                {
                    FacingDirection.Rotate(-_hopFrameRotation);
                    Rotation = (float) Math.Atan2(FacingDirection.X, -FacingDirection.Y);
                    Position += distance * FacingDirection;
                }
                else if (HopDirection == HopDirection.Right)
                {
                    FacingDirection.Rotate(_hopFrameRotation);
                    Rotation = (float) Math.Atan2(FacingDirection.X, -FacingDirection.Y);
                    Position += distance * FacingDirection;
                }
                else if (HopDirection == HopDirection.Backward)
                {
                    if (HopFrame == _hopFrameLength / 2)
                    {
                        FacingDirection.Rotate(MathHelper.Pi);
                        Rotation = (float) Math.Atan2(FacingDirection.X, -FacingDirection.Y);
                    }
                }
            }
            HopFrame++;
            if (HopFrame >= _hopFrameLength)
            {
                _flipAnimation.Reset();
                HopDirection = HopDirection.None;
                HopCooldown = _hopDelay;
            }
        }
        else if (HopCooldown > 0)
        {
            HopCooldown--;
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        foreach (var circle in GetTongueBoundingCircles())
        {
            _tongueSegmentTexture.Rotation = Rotation;
            _tongueSegmentTexture.Draw(spriteBatch, circle.Center);
        }

        if (HopDirection is HopDirection.Forward or HopDirection.Left or HopDirection.Right)
        {
            _hop.Rotation = Rotation;
            _hop.Draw(spriteBatch, Position);
        }
        else if (HopDirection is HopDirection.Backward)
        {
            _flipAnimation.Rotation = Rotation;
            _flipAnimation.Update(gameTime);
            _flipAnimation.Draw(spriteBatch, Position);
        }
        else
        {
            _idle.Rotation = Rotation;
            _idle.Draw(spriteBatch, Position);
        }
    }

    public void Reset()
    {
        Position = new Vector2(Spider.Home.X, Spider.Home.Y - 16);
        HopDirection = HopDirection.Forward;
        HopFrame = 0;
        HopCooldown = 0;
        FacingDirection = new Vector2(0, -1);
        Rotation = 0;
        Tonguing = false;
        TongueFrame = 0;
        _tongueSegmentCount = 0;
    }

    public Circle[] GetTongueBoundingCircles()
    {
        Circle[] circles = new Circle[_tongueSegmentCount];
        for (var i = 0; i < _tongueSegmentCount; i++)
        {
            circles[i] = new Circle { 
                Center = Position + FacingDirection * (_tongueSegmentSpacing * i + 5),
                Radius = _tongueSegmentRadius
            };
        }
        return circles;
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        byte hopDirectionByte = 0;

        reader.Read(ref Position);
        reader.Read(ref hopDirectionByte);
        reader.Read(ref HopFrame);
        reader.Read(ref HopCooldown);
        reader.Read(ref Tonguing);
        reader.Read(ref TongueFrame);
        reader.Read(ref FacingDirection);
        reader.Read(ref Rotation);
        reader.Read(ref Respawning);
        reader.Read(ref RespawnFrame);
        reader.Read(ref DeadDirection);

        HopDirection = (HopDirection)hopDirectionByte;
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        byte hopDirectionByte = (byte)HopDirection;

        writer.Write(in Position);
        writer.Write(in hopDirectionByte);
        writer.Write(in HopFrame);
        writer.Write(in HopCooldown);
        writer.Write(in Tonguing);
        writer.Write(in TongueFrame);
        writer.Write(in FacingDirection);
        writer.Write(in Rotation);
        writer.Write(in Respawning);
        writer.Write(in RespawnFrame);
        writer.Write(in DeadDirection);
    }
}
