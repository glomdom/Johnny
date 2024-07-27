using Johnny.Tests.Data;
using Xunit.Abstractions;

namespace Johnny.Tests;

public class ReaderTests(ITestOutputHelper outputHelper) {
    private readonly ITestOutputHelper _outputHelper = outputHelper;

    [Fact]
    public void Test_SimpleStructRead() {
        int[] values = [32, 42];
        var bytes = values.SelectMany(BitConverter.GetBytes).ToArray();

        using MemoryStream fileContents = new(bytes);
        using BinaryReader reader = new(fileContents);

        SimpleStruct result = SimpleStruct.ReadStruct(reader);
        SimpleStruct expected = new() {
            X = 32,
            Y = 42,
        };

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Test_NestedStructRead() {
        byte[] bytes;

        using MemoryStream writerStream = new();
        using (BinaryWriter writer = new(writerStream)) {
            writer.Write(0);
            writer.Write(0);
            writer.Write(0);
            writer.Write(1);
            writer.Write(1);
            writer.Write(1);
            writer.Write(32);

            bytes = writerStream.ToArray();
        }

        using MemoryStream readerStream = new(bytes);
        using BinaryReader reader = new(readerStream);

        var result = NestedStruct.ReadStruct(reader);
        var expected = new NestedStruct {
            Vector = new Vector3 {
                X = 0,
                Y = 0,
                Z = 0,
            },
            
            AnotherVector = new Vector3 {
                X = 0,
                Y = 0,
                Z = 0,
            },
            
            RandomInteger = 32,
        };
    }

    [Fact]
    public void Test_PrimitivesRead() {
        byte[] bytes;

        using MemoryStream writerStream = new();
        using (BinaryWriter writer = new(writerStream)) {
            writer.Write((byte)1);
            writer.Write((sbyte)-2);
            writer.Write((short)3);
            writer.Write((ushort)4);
            writer.Write(5);
            writer.Write((uint)6);
            writer.Write((long)7);
            writer.Write((ulong)8);
            writer.Write(9.1f);
            writer.Write(10.2);
            writer.Write(true);
            writer.Write('a');
            writer.Write("hello");

            bytes = writerStream.ToArray();
        }

        using MemoryStream readerStream = new(bytes);
        using BinaryReader reader = new(readerStream);

        var result = AllPrimitivesStruct.ReadStruct(reader);
        var expected = new AllPrimitivesStruct {
            Byte = 1,
            SByte = -2,
            Short = 3,
            UShort = 4,
            Int = 5,
            UInt = 6,
            Long = 7,
            ULong = 8,
            Float = 9.1f,
            Double = 10.2,
            Bool = true,
            Char = 'a',
            String = "hello",
        };

        Assert.Equal(expected, result);
    }
}