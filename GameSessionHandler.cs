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
    INetcodeSession<PlayerInputs> _session;
    GameState _gameState;
    NetcodePlayer _localPlayer;
    TimeSpan _sleepTime;
    PlayerInputs _localInput;

    public GameSessionHandler(INetcodeSession<PlayerInputs> session)
    {
        _session = session;
        _gameState = new GameState();
        _session.TryGetLocalPlayer(out _localPlayer);

        _gameState.FrameNumber = 0;
        _gameState.Players = [new Player(), new Player()];

        _localInput = new PlayerInputs();
    }

    public void Update(GameTime gameTime)
    {
        if (_sleepTime > TimeSpan.Zero)
        {
            _sleepTime -= gameTime.ElapsedGameTime;
            return;
        }

        _session.BeginFrame();

        _localInput.DigitalInputs = DigitalInputs.None;

        if (Keyboard.GetState().IsKeyDown(Keys.Up))
            _localInput.DigitalInputs |= DigitalInputs.Up;
        if (Keyboard.GetState().IsKeyDown(Keys.Down))
            _localInput.DigitalInputs |= DigitalInputs.Down;
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
            _localInput.DigitalInputs |= DigitalInputs.Left;
        if (Keyboard.GetState().IsKeyDown(Keys.Right))
            _localInput.DigitalInputs |= DigitalInputs.Right;
        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            _localInput.DigitalInputs |= DigitalInputs.Click;
        _localInput.CursorPosition.X = Mouse.GetState().X;
        _localInput.CursorPosition.Y = Mouse.GetState().Y;

        if (_session.AddLocalInput(_localPlayer, _localInput) is not ResultCode.Ok)
            return;
        if (_session.SynchronizeInputs() is not ResultCode.Ok)
            return;

        UpdateGameState(_session.CurrentSynchronizedInputs);

        _session.AdvanceFrame();
    }

    public void UpdateGameState(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        _gameState.FrameNumber++;
        for (var i = 0; i < inputs.Length && i < _gameState.Players.Length; i++)
        {
            _gameState.Players[i].Update(inputs[i]);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var player in _gameState.Players)
        {
            player.Draw(spriteBatch);
        }
    }

    public void AdvanceFrame()
    {
        _session.SynchronizeInputs();
        UpdateGameState(_session.CurrentSynchronizedInputs);
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
        reader.Read(ref _gameState.FrameNumber);
        reader.Read(_gameState.Players);
    }

    public void SaveState(in Frame frame, ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in _gameState.FrameNumber);
        writer.Write(_gameState.Players);
    }

    public void TimeSync(FrameSpan framesAhead)
    {
        _sleepTime = framesAhead.Duration();
    }
}

public struct GameState
{
    public int FrameNumber;
    public Player[] Players;
}
