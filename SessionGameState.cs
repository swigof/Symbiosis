using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Entity;
using Symbiosis.Input;
using System;
using System.Threading;

namespace Symbiosis;

public struct SessionGameState : IBinarySerializable
{
    int _frameNumber = 0;
    PlayerInputs[] _previousInputs = new PlayerInputs[2];
    Spider _spider;
    Frog _frog;

    Mutex _stateMutex = new Mutex();

    public SessionGameState(bool isLocal, int firstLocalPlayerIndex)
    {
        if (isLocal)
        {
            _spider = new Spider(true);
            _frog = new Frog(true);
        }
        else
        {
            // Default player 1 to spider and 2 to frog for now
            _spider = new Spider(firstLocalPlayerIndex == 0);
            _frog = new Frog(firstLocalPlayerIndex == 1);
        }
    }

    private SessionGameState(SessionGameState toCopy)
    {
        _frameNumber = toCopy._frameNumber;
        _spider = toCopy._spider;
        _frog = toCopy._frog;
        _previousInputs[0] = toCopy._previousInputs[0];
        _previousInputs[1] = toCopy._previousInputs[1];
    }

    // Not thread protected. Not guaranteed to be state alligned. 
    public bool IsLocalCursorPlayer() => _spider.IsLocalPlayer;

    public SessionGameState GetCopy() {
        _stateMutex.WaitOne();
        try
        {
            return new SessionGameState(this);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void Update(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        _stateMutex.WaitOne();
        try
        {
            _frameNumber++;
            _spider.Update(inputs[0].Input);
            _frog.Update(inputs[1].Input);
            _previousInputs[0] = inputs[0];
            _previousInputs[1] = inputs[1];
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
            _frog.Draw(spriteBatch);
            _spider.Draw(spriteBatch);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        _stateMutex.WaitOne();
        try
        {
            reader.Read(ref _frameNumber);
            reader.Read(ref _spider);
            reader.Read(ref _frog);
            reader.Read(ref _previousInputs[0]);
            reader.Read(ref _previousInputs[1]);
        }
        finally
        { 
            _stateMutex.ReleaseMutex(); 
        }
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        _stateMutex.WaitOne();
        try
        {
            writer.Write(in _frameNumber);
            writer.Write(in _spider);
            writer.Write(in _frog);
            writer.Write(in _previousInputs[0]);
            writer.Write(in _previousInputs[1]);
        }
        finally
        {
            _stateMutex.ReleaseMutex();
        }
    }
}
