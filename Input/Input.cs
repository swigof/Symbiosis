using Backdash.Serialization;
using System;
using System.Runtime.InteropServices;

namespace Symbiosis.Input;

[Flags]
public enum DigitalInputs : ushort
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
    Action = 1 << 4,
    Click = 1 << 5
}

public record struct CursorPosition
{
    public int X;
    public int Y;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct PlayerInputs : IBinarySerializable
{
    public DigitalInputs DigitalInputs;
    public CursorPosition CursorPosition;

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        byte digitalInputsByte = 0;

        reader.Read(ref digitalInputsByte);
        reader.Read(ref CursorPosition.X);
        reader.Read(ref CursorPosition.Y);

        DigitalInputs = (DigitalInputs)digitalInputsByte;
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        byte digitalInputsByte = (byte)DigitalInputs;

        writer.Write(in digitalInputsByte);
        writer.Write(in CursorPosition.X);
        writer.Write(in CursorPosition.Y);
    }
}
