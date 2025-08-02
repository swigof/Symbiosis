using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Symbiosis.Entity;
using Symbiosis.Input;
using System;
using System.Collections.Generic;

namespace Symbiosis;

public class GameSessionHandler : INetcodeSessionHandler
{
    static Color QuarterTransparent = new Color(255, 255, 255, 255/4);

    INetcodeSession<PlayerInputs> _session;
    GameState _gameState;
    TimeSpan _sleepTime;
    PlayerInputs _localInput;
    Vector2 _remoteCursorPosition;
    Texture2D _cursorTexture;

    public GameSessionHandler(INetcodeSession<PlayerInputs> session)
    {
        _session = session;
        _gameState = new GameState();
        _remoteCursorPosition = Vector2.Zero;
        _cursorTexture = Game1.GameContent.Load<Texture2D>("cursor_none");

        _gameState.FrameNumber = 0;
        _gameState.PreviousInputs = new PlayerInputs[2];
        if (_session.IsLocal())
        {
            _gameState.Spider = new Spider(true);
            _gameState.Frog = new Frog(true);
        }
        else
        {
            // Default player 1 to spider and 2 to frog for now
            NetcodePlayer localPlayer;
            _session.TryGetLocalPlayer(out localPlayer);
            _gameState.Spider = new Spider(localPlayer != null && localPlayer.Index == 0);
            _gameState.Frog = new Frog(localPlayer != null && localPlayer.Index == 1);
        }
        _gameState.EggEnemyClusters = new List<EggEnemyCluster>();

        _localInput = new PlayerInputs();
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

        UpdateGameState(_session.CurrentSynchronizedInputs);

        _session.AdvanceFrame();
    }

    public void UpdateGameState(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        _gameState.FrameNumber++;
        _gameState.Spider.Update(inputs[0].Input);
        _gameState.Frog.Update(inputs[1].Input);
        _gameState.PreviousInputs[0] = inputs[0];
        _gameState.PreviousInputs[1] = inputs[1];
        if (!IsLocalCursorPlayer())
        {
            _remoteCursorPosition.X = inputs[0].Input.CursorPosition.X;
            _remoteCursorPosition.Y = inputs[0].Input.CursorPosition.Y;
        }
        foreach (var cluster in _gameState.EggEnemyClusters)
        {
            cluster.Update();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _gameState.Frog.Draw(spriteBatch);
        _gameState.Spider.Draw(spriteBatch);
        if (!IsLocalCursorPlayer())
            spriteBatch.Draw(_cursorTexture, _remoteCursorPosition, null, QuarterTransparent);
        foreach (var cluster in _gameState.EggEnemyClusters)
        {
            cluster.Draw(spriteBatch);
        }
    }

    public bool IsLocalCursorPlayer() => _gameState.Spider.IsLocalPlayer;

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
        reader.Read(_gameState.Spider);
        reader.Read(_gameState.Frog);
        reader.Read(_gameState.PreviousInputs);

        int clusterCount = 0;
        reader.Read(ref clusterCount);
        int i = 0;
        while (i < _gameState.EggEnemyClusters.Count && i < clusterCount)
        {
            reader.Read(_gameState.EggEnemyClusters[i]);
            i++;
        }
        if (i < _gameState.EggEnemyClusters.Count)
        {
            _gameState.EggEnemyClusters.RemoveRange(i, _gameState.EggEnemyClusters.Count - i);
        }
        else if (i < clusterCount)
        {
            while (i < clusterCount)
            {
                EggEnemyCluster cluster = new EggEnemyCluster();
                reader.Read(cluster);
                _gameState.EggEnemyClusters.Add(cluster);
            }
        }
    }

    public void SaveState(in Frame frame, ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in _gameState.FrameNumber);
        writer.Write(_gameState.Spider);
        writer.Write(_gameState.Frog);
        writer.Write(_gameState.PreviousInputs);

        writer.Write(_gameState.EggEnemyClusters.Count);
        foreach (var cluster in _gameState.EggEnemyClusters)
        {
            writer.Write(cluster);
        }
    }

    public void TimeSync(FrameSpan framesAhead)
    {
        _sleepTime = framesAhead.Duration();
    }
}

public struct GameState
{
    public int FrameNumber;
    public Spider Spider;
    public Frog Frog;
    public PlayerInputs[] PreviousInputs;
    public List<EggEnemyCluster> EggEnemyClusters;
}
