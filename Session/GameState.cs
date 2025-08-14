using Backdash.Serialization;
using Symbiosis.Entity;
using Symbiosis.Input;

namespace Symbiosis.Session;

public struct GameState : IBinarySerializable
{
    public int FrameNumber = 0;
    public PlayerInputs[] PreviousInputs = new PlayerInputs[2];
    public Spider Spider = new Spider();
    public Frog Frog = new Frog();
    public EggEnemyCluster[] Clusters = new EggEnemyCluster[5];
    public FrogEnemy[] FrogEnemies = new FrogEnemy[5];
    public int NextEggEnemyIndex = 0; 
    public int NextFrogEnemyIndex = 0;
    public int EggCount = 100;

    public GameState()
    {
        for (var i = 0; i < PreviousInputs.Length; i++)
            PreviousInputs[i] = new PlayerInputs();
        for (var i = 0; i < Clusters.Length; i++)
            Clusters[i] = new EggEnemyCluster();
        for (var i = 0; i < FrogEnemies.Length; i++)
            FrogEnemies[i] = new FrogEnemy();
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref FrameNumber);
        reader.Read(ref Spider);
        reader.Read(ref Frog);
        reader.Read(ref PreviousInputs[0]);
        reader.Read(ref PreviousInputs[1]);
        reader.Read(ref Clusters);
        reader.Read(ref FrogEnemies);
        reader.Read(ref NextEggEnemyIndex);
        reader.Read(ref NextFrogEnemyIndex);
        reader.Read(ref EggCount);
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in FrameNumber);
        writer.Write(in Spider);
        writer.Write(in Frog);
        writer.Write(in PreviousInputs[0]);
        writer.Write(in PreviousInputs[1]);
        writer.Write(in Clusters);
        writer.Write(in FrogEnemies);
        writer.Write(in NextEggEnemyIndex);
        writer.Write(in NextFrogEnemyIndex);
        writer.Write(in EggCount);
    }
}
