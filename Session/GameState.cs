using System.Text.Json.Serialization;
using Backdash;
using Backdash.Serialization;
using Symbiosis.Entity;
using Symbiosis.Input;
using Symbiosis.UI;

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
    public int RoundFrame = 0;
    public int LastEggEnemySpawn = 0;
    public int LastFrogEnemySpawn = 0;
    public bool Paused = false;
    public int EndedOnFrame = 0;
    public ButtonState ResumeButtonState = new ButtonState();
    public ButtonState RetryButtonState = new ButtonState();
    public ButtonState CreditsButtonState = new ButtonState();
    public ButtonState BackButtonState = new ButtonState();
    public bool ShowCredits = false;
    public PeerEvent ConnectionEvent = new PeerEvent();

    [JsonIgnore] public bool RoundLost => EndedOnFrame > 0 && EggCount <= 0; 
    [JsonIgnore] public bool RoundWon => EndedOnFrame > 0 && EggCount > 0; 

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
        sbyte connectionEventByte = 0;

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
        reader.Read(ref RoundFrame);
        reader.Read(ref LastEggEnemySpawn);
        reader.Read(ref LastFrogEnemySpawn);
        reader.Read(ref Paused);
        reader.Read(ref EndedOnFrame);
        reader.Read(ref ResumeButtonState);
        reader.Read(ref RetryButtonState);
        reader.Read(ref CreditsButtonState);
        reader.Read(ref BackButtonState);
        reader.Read(ref ShowCredits);
        reader.Read(ref connectionEventByte);

        ConnectionEvent = (PeerEvent)connectionEventByte;
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        sbyte connectionEventByte = (sbyte)ConnectionEvent;
        
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
        writer.Write(in RoundFrame);
        writer.Write(in LastEggEnemySpawn);
        writer.Write(in LastFrogEnemySpawn);
        writer.Write(in Paused);
        writer.Write(in EndedOnFrame);
        writer.Write(in ResumeButtonState);
        writer.Write(in RetryButtonState);
        writer.Write(in CreditsButtonState);
        writer.Write(in BackButtonState);
        writer.Write(in ShowCredits);
        writer.Write(in connectionEventByte);
    }
}
