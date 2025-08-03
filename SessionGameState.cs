using Backdash;
using Backdash.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Symbiosis.Entity;
using Symbiosis.Input;
using System;
using System.Collections.Generic;

namespace Symbiosis;

public class SessionGameState : IBinarySerializable
{
    public int FrameNumber = 0;
    public PlayerInputs[] PreviousInputs = new PlayerInputs[2];
    public List<EggEnemyCluster> EggEnemyClusters = new List<EggEnemyCluster>();
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

    public bool IsLocalCursorPlayer() => Spider.IsLocalPlayer;

    public void Update(ReadOnlySpan<SynchronizedInput<PlayerInputs>> inputs)
    {
        FrameNumber++;
        Spider.Update(inputs[0].Input);
        Frog.Update(inputs[1].Input);
        PreviousInputs[0] = inputs[0];
        PreviousInputs[1] = inputs[1];
        foreach (var cluster in EggEnemyClusters)
            cluster.Update();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Frog.Draw(spriteBatch);
        Spider.Draw(spriteBatch);
        foreach (var cluster in EggEnemyClusters)
            cluster.Draw(spriteBatch);
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref FrameNumber);
        reader.Read(Spider);
        reader.Read(Frog);
        reader.Read(PreviousInputs);

        int clusterCount = 0;
        reader.Read(ref clusterCount);
        int i = 0;
        while (i < EggEnemyClusters.Count && i < clusterCount)
        {
            reader.Read(EggEnemyClusters[i]);
            i++;
        }
        if (i < EggEnemyClusters.Count)
        {
            EggEnemyClusters.RemoveRange(i, EggEnemyClusters.Count - i);
        }
        else if (i < clusterCount)
        {
            while (i < clusterCount)
            {
                EggEnemyCluster cluster = new EggEnemyCluster();
                reader.Read(cluster);
                EggEnemyClusters.Add(cluster);
            }
        }
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in FrameNumber);
        writer.Write(Spider);
        writer.Write(Frog);
        writer.Write(PreviousInputs);

        writer.Write(EggEnemyClusters.Count);
        foreach (var cluster in EggEnemyClusters)
        {
            writer.Write(cluster);
        }
    }
}
