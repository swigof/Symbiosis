 // MIT License Copyright (c) 2022 Lucas Teles
 // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 // documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
 // rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 // permit persons to whom the Software is furnished to do so, subject to the following conditions:
 //
 // The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
 // Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
 // LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 // THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 // THE SOFTWARE.

using Backdash.Serialization;
using Microsoft.Xna.Framework;
using System;

namespace Symbiosis;

public static class Extensions
{
    public static Vector2 RoundTo(this Vector2 vector, int digits = 2) =>
        new(MathF.Round(vector.X, digits), MathF.Round(vector.Y, digits));

    public static void Write(this BinaryBufferWriter writer, in Rectangle rect)
    {
        writer.Write(rect.X);
        writer.Write(rect.Y);
        writer.Write(rect.Width);
        writer.Write(rect.Height);
    }

    public static void Write(this BinaryBufferWriter writer, in Vector2 rect)
    {
        writer.Write(rect.X);
        writer.Write(rect.Y);
    }

    public static void Read(this BinaryBufferReader reader, ref Vector2 vector)
    {
        reader.Read(ref vector.X);
        reader.Read(ref vector.Y);
    }

    public static void Read(this BinaryBufferReader reader, ref Rectangle rect)
    {
        rect.X = reader.ReadInt32();
        rect.Y = reader.ReadInt32();
        rect.Width = reader.ReadInt32();
        rect.Height = reader.ReadInt32();
    }
}
