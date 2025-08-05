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

    public GameState() { }

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
