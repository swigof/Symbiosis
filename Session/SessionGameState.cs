using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Entity;
using Symbiosis.Input;
using System;
using System.Threading;
using Symbiosis.Manager;

namespace Symbiosis.Session;

// Provides thread protected game state access
public struct SessionGameState
{
    GameState _gameState = new GameState();
    Mutex _stateMutex = new Mutex();

    public SessionGameState(bool isLocal, int firstLocalPlayerIndex)
    {
        if (isLocal)
        {
            _gameState.Spider = new Spider(true);
            _gameState.Frog = new Frog(true);
        }
        else
        {
            // Default player 1 to spider and 2 to frog for now
            _gameState.Spider = new Spider(firstLocalPlayerIndex == 0);
            _gameState.Frog = new Frog(firstLocalPlayerIndex == 1);
        }
    }

    // Not thread protected. Not guaranteed to be state alligned. 
    public bool IsLocalCursorPlayer() => _gameState.Spider.IsLocalPlayer;

    public void Update(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        _stateMutex.WaitOne();
        try
        {
            _gameState.FrameNumber++;
            _gameState.Spider.Update(inputs[0].Input);
            _gameState.Frog.Update(inputs[1].Input);
            _gameState.PreviousInputs[0] = inputs[0];
            _gameState.PreviousInputs[1] = inputs[1];
            for (var i = 0; i < _gameState.Clusters.Length; i++)
            {
                if(_gameState.Clusters[i].Active)
                    _gameState.Clusters[i].Update();
            }
            for (var i = 0; i < _gameState.FrogEnemies.Length; i++)
            {
                if(_gameState.FrogEnemies[i].Active)
                    _gameState.FrogEnemies[i].Update(_gameState.Frog.Position);
            }
            SpawnManager.Update(_gameState);
            CollisionManager.Update(_gameState);
        }
        finally 
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _stateMutex.WaitOne();
        try
        {
            for (var i = 0; i < _gameState.Clusters.Length; i++)
            {
                if(_gameState.Clusters[i].Active)
                    _gameState.Clusters[i].Draw(spriteBatch);
            }
            _gameState.Frog.Draw(spriteBatch);
            for (var i = 0; i < _gameState.FrogEnemies.Length; i++)
            {
                if(_gameState.FrogEnemies[i].Active)
                    _gameState.FrogEnemies[i].Draw(spriteBatch);
            }
            _gameState.Spider.Draw(spriteBatch);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void LoadState(ref readonly BinaryBufferReader reader)
    {
        _stateMutex.WaitOne();
        try
        {
            reader.Read(ref _gameState);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void SaveState(ref readonly BinaryBufferWriter writer)
    {
        _stateMutex.WaitOne();
        try
        {
            writer.Write(in _gameState);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }
}
