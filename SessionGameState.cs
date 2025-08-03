using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Entity;
using Symbiosis.Input;
using System;

namespace Symbiosis;

public struct SessionGameState : IBinarySerializable
{
    public int FrameNumber = 0;
    public PlayerInputs[] PreviousInputs = new PlayerInputs[2];
    public Spider Spider;
    public Frog Frog;

    public SessionGameState(bool isLocal, int firstLocalPlayerIndex)
    {
        if (isLocal)
        {
            Spider = new Spider(true);
            Frog = new Frog(true);
        }
        else
        {
            // Default player 1 to spider and 2 to frog for now
            Spider = new Spider(firstLocalPlayerIndex == 0);
            Frog = new Frog(firstLocalPlayerIndex == 1);
        }
    }

    private SessionGameState(SessionGameState toCopy)
    {
        FrameNumber = toCopy.FrameNumber;
        Spider = toCopy.Spider;
        Frog = toCopy.Frog;
        PreviousInputs[0] = toCopy.PreviousInputs[0];
        PreviousInputs[1] = toCopy.PreviousInputs[1];
    }

    public bool IsLocalCursorPlayer() => Spider.IsLocalPlayer;

    public SessionGameState GetCopy() => new SessionGameState(this);

    public void Update(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        FrameNumber++;
        Spider.Update(inputs[0].Input);
        Frog.Update(inputs[1].Input);
        PreviousInputs[0] = inputs[0];
        PreviousInputs[1] = inputs[1];
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Frog.Draw(spriteBatch);
        Spider.Draw(spriteBatch);
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref FrameNumber);
        reader.Read(ref Spider);
        reader.Read(ref Frog);
        reader.Read(ref PreviousInputs[0]);
        reader.Read(ref PreviousInputs[1]);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in FrameNumber);
        writer.Write(in Spider);
        writer.Write(in Frog);
        writer.Write(in PreviousInputs[0]);
        writer.Write(in PreviousInputs[1]);
    }
}
