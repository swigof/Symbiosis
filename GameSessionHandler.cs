using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Symbiosis.Input;
using System;

namespace Symbiosis;

public class GameSessionHandler : INetcodeSessionHandler
{
    readonly static Color QuarterTransparent = new Color(255, 255, 255, 255 / 4);

    public SessionGameState GameState;

    INetcodeSession<PlayerInputs> _session;
    TimeSpan _sleepTime = new TimeSpan();
    PlayerInputs _localInput = new PlayerInputs();
    Vector2 _remoteCursorPosition = Vector2.Zero;
    Texture2D _cursorTexture = Game1.GameContent.Load<Texture2D>("cursor_none");

    public GameSessionHandler(INetcodeSession<PlayerInputs> session)
    {
        _session = session;
        NetcodePlayer localPlayer;
        _session.TryGetLocalPlayer(out localPlayer);
        GameState = new SessionGameState(_session.IsLocal(), localPlayer.Index);
    }

    public void Update(GameTime gameTime, bool isActive)
    {
        if (_sleepTime > TimeSpan.Zero)
        {
            _sleepTime -= gameTime.ElapsedGameTime;
            return;
        }

        _session.BeginFrame();

        _localInput.DigitalInputs = DigitalInputs.None;

        if (isActive)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                _localInput.DigitalInputs |= DigitalInputs.Up;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                _localInput.DigitalInputs |= DigitalInputs.Down;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                _localInput.DigitalInputs |= DigitalInputs.Left;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                _localInput.DigitalInputs |= DigitalInputs.Right;
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                _localInput.DigitalInputs |= DigitalInputs.Action;
            var mouseState = Mouse.GetState();
            if (Game1.Graphics.GraphicsDevice.Viewport.Bounds.Contains(mouseState.Position))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                    _localInput.DigitalInputs |= DigitalInputs.Click;
                _localInput.CursorPosition.X = mouseState.X;
                _localInput.CursorPosition.Y = mouseState.Y;
            }
        }

        foreach (var player in _session.GetPlayers())
        {
            if (player.IsLocal())
            {
                if (_session.AddLocalInput(player, _localInput) is not ResultCode.Ok)
                    return;
            }
        }

        if (_session.SynchronizeInputs() is not ResultCode.Ok)
            return;

        var inputs = _session.CurrentSynchronizedInputs;
        if (!GameState.IsLocalCursorPlayer())
        {
            _remoteCursorPosition.X = inputs[0].Input.CursorPosition.X;
            _remoteCursorPosition.Y = inputs[0].Input.CursorPosition.Y;
        }
        GameState.Update(inputs);

        _session.AdvanceFrame();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!GameState.IsLocalCursorPlayer())
            spriteBatch.Draw(_cursorTexture, _remoteCursorPosition, null, QuarterTransparent);
        GameState.Draw(spriteBatch);
    }

    public void AdvanceFrame()
    {
        _session.SynchronizeInputs();
        GameState.Update(_session.CurrentSynchronizedInputs);
        _session.AdvanceFrame(); 
    }

    public void OnPeerEvent(NetcodePlayer player, PeerEventInfo evt)
    {
        //throw new NotImplementedException();
    }

    public void OnSessionClose()
    {
        //throw new NotImplementedException();
    }

    public void OnSessionStart()
    {
        //throw new NotImplementedException();
    }

    public void LoadState(in Frame frame, ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref GameState);
    }

    public void SaveState(in Frame frame, ref readonly BinaryBufferWriter writer)
    {
        writer.Write(ref GameState);
    }

    public void TimeSync(FrameSpan framesAhead)
    {
        _sleepTime = framesAhead.Duration();
    }
}
