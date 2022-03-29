using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class MD5
{
    private const int BLOCK_BUFFER_SIZE = 64;
    private const int MD_BUFFER_SIZE = 16;
    private class Context
    {
        public uint[] BlockBuffer;
        public uint[] MDBuffer;
        public Stream Reader;
        public ulong Length => (ulong)Reader.Length * 8;
        public string Output
        {
            get
            {
                StringBuilder output = new StringBuilder(32);
                for (var i = 0; i < 4; ++i)
                {
                    var bytes = BitConverter.GetBytes(MDBuffer[i]);
                    foreach (byte b in bytes)
                    {
                        output.AppendFormat("{0:x2}", b);
                    }
                }
                return output.ToString();
            }
        }
        private unsafe Span<byte> _Block {
            get{
                fixed (uint* ptr = BlockBuffer)
                {
                    return new Span<byte>(ptr, BLOCK_BUFFER_SIZE);
                }
            }
        }

        private unsafe Span<byte> _MD
        {
            get
            {
                fixed (uint* ptr = MDBuffer)
                {
                    return new Span<byte>(ptr, MD_BUFFER_SIZE);
                }
            }
        }

        public Span<byte> LengthSpan => new Span<byte>(BitConverter.GetBytes(Length));

        private Context(Stream stream)
        {
            BlockBuffer = new uint[BLOCK_BUFFER_SIZE / 4];
            MDBuffer = new uint[MD_BUFFER_SIZE / 4];
            Reader = stream;
            Reader.Position = 0;
        }

        private static int FillSpan(Span<byte> span, Span<byte> data, int offset, int length)
        {
            length = Math.Min(Math.Min(length, span.Length - offset), data.Length);
            if (length > span.Length - offset)
                length = span.Length - offset;
            if (length < 0 || offset < 0)
                throw new ArgumentException();
            for (var i = 0; i < length; ++i)
            {
                span[offset + i] = data[i];
            }
            return length;
        }

        public int FillBlock()
        {
            return Reader.Read(_Block);
        }

        public int FillBlock(Span<byte> data, int offset = 0, int length = 16)
        {
            return FillSpan(_Block, data, offset, length);
        }


        public int FillMD(Span<byte> data, int offset = 0, int length = 16)
        {
            return FillSpan(_MD, data, offset, length);
        }

        public static Context FromStream(Stream stream)
        {
            return new Context(stream);
        }

        public static Context FromFile(FileInfo fileInfo)
        {
            FileStream stream = fileInfo.OpenRead();
            return new Context(stream);
        }

        public static Context FromByteArray(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            return new Context(stream);
        }

        public static Context FromString(string message, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(message);
            return FromByteArray(data);
        }

        public static Context FromString(string message)
        {
            return FromString(message, Encoding.UTF8);
        }
    }

    private static readonly Func<int, uint> T = (index) =>
        (UInt32) (Math.Abs(Math.Sin(index + 1)) * 0x1_0000_0000L);

    private static readonly Func<int, Func<uint, uint, uint, uint>> F = (index) =>
    {
        switch(index / 16)
        {
            case 0: return (x, y, z) => x & y | ~x & z;
            case 1: return (x, y, z) => x & z | y & ~z;
            case 2: return (x, y, z) => x ^ y ^ z;
            case 3: return (x, y, z) => y ^ (x | ~z);
            default: throw new ArgumentOutOfRangeException();
        }
    };

    private static readonly Func<int, int> K = (index) =>
    {
        switch (index / 16)
        {
            case 0: return index % 16;
            case 1: return (index * 5 + 1) % 16;
            case 2: return (index * 3 + 5) % 16;
            case 3: return (index * 7) % 16;
            default: throw new ArgumentOutOfRangeException();
        }
    };

    private static readonly int[] S =
    {
        7,  12, 17, 22, 7,  12, 17, 22, 7,  12, 17, 22, 7,  12, 17, 22,
        5,  9,  14, 20, 5,  9,  14, 20, 5,  9,  14, 20, 5,  9,  14, 20,
        4,  11, 16, 23, 4,  11, 16, 23, 4,  11, 16, 23, 4,  11, 16, 23,
        6,  10, 15, 21, 6,  10, 15, 21, 6,  10, 15, 21, 6,  10, 15, 21,
    };

    private static readonly byte[] MD =
    {
        0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
        0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
    };

    private static readonly byte[] PAD =
    {
        0b_1000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
        0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000,
    };

    private static uint Rotate(uint data, int leftShift)
    {
         return data << leftShift | data >> (32 - leftShift);
    }

    private static void ProcessBlock(Context context)
    {
        var Block = context.BlockBuffer;
        var MD = context.MDBuffer;
        UInt32 A = MD[0];
        UInt32 B = MD[1];
        UInt32 C = MD[2];
        UInt32 D = MD[3];
        for (var i = 0; i < 64; ++i)
        {
            A = B + Rotate(A + F(i)(B, C, D) + Block[K(i)] + T(i), S[i]);
            (A, B, C, D) = (D, A, B, C);
        }
        MD[0] += A;
        MD[1] += B;
        MD[2] += C;
        MD[3] += D;
    }

    private static string CheckSum(Context context)
    {
        context.FillMD(MD);
        int fillSize = context.FillBlock();
        while (fillSize == BLOCK_BUFFER_SIZE)
        {
            ProcessBlock(context);
            fillSize = context.FillBlock();
        }
        if (fillSize < 56)
        {
            context.FillBlock(new Span<byte>(PAD, 0, 56 - fillSize), fillSize, 56 - fillSize);
            context.FillBlock(context.LengthSpan, 56, 8);
            ProcessBlock(context);
        }
        else
        {
            context.FillBlock(new Span<byte>(PAD, 0, 64 - fillSize), fillSize, 64 - fillSize);
            ProcessBlock(context);
            context.FillBlock(new Span<byte>(PAD, 64 - fillSize, 56), 0, 56);
            context.FillBlock(context.LengthSpan, 56, 8);
            ProcessBlock(context);
        }
        return context.Output;
    }

    public static string CheckSum(FileInfo fileInfo)
    {
        var context = Context.FromFile(fileInfo);
        return CheckSum(context);
    }
}
