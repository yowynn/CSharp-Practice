using System;
using System.IO;
using System.Text;

internal class MD5
{
    private const int BLOCK_SIZE = 64;
    private const int MD_SIZE = 16;
    private const int BLOCK_SIZE_PADDING = BLOCK_SIZE - sizeof(ulong);
    private class Context
    {
        public uint[] Block;
        public uint[] MD;
        public Stream Reader;
        public ulong Length => (ulong)Reader.Length;
        public string Output
        {
            get
            {
                StringBuilder output = new(MD_SIZE * 2);
                foreach (byte b in MDBuffer)
                {
                    output.AppendFormat("{0:x2}", b);
                }
                return output.ToString();
            }
        }

        private unsafe Span<byte> BlockBuffer
        {
            get{
                fixed (uint* ptr = Block)
                {
                    return new Span<byte>(ptr, BLOCK_SIZE);
                }
            }
        }

        private unsafe Span<byte> MDBuffer
        {
            get
            {
                fixed (uint* ptr = MD)
                {
                    return new Span<byte>(ptr, MD_SIZE);
                }
            }
        }

        private Span<byte> LengthBuffer => new(BitConverter.GetBytes(Length << 3));

        private Context(Stream stream)
        {
            Block = new uint[BLOCK_SIZE >> 2];
            MD = new uint[MD_SIZE >> 2];
            Reader = stream;
            Reader.Position = 0;
        }

        private static int FillSpan(Span<byte> span, Span<byte> data, int offset, int length)
        {
            length = Math.Min(Math.Min(length, span.Length - offset), data.Length);
            if (length > span.Length - offset)
                length = span.Length - offset;
            if (length < 0 || offset < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            for (var i = 0; i < length; ++i)
            {
                span[offset + i] = data[i];
            }
            return length;
        }

        public int FillBlock()
        {
            return Reader.Read(BlockBuffer);
        }

        public int FillBlock(Span<byte> data, int offset = 0, int length = BLOCK_SIZE)
        {
            return FillSpan(BlockBuffer, data, offset, length);
        }


        public int FillMD(Span<byte> data, int offset = 0, int length = MD_SIZE)
        {
            return FillSpan(MDBuffer, data, offset, length);
        }

        public void AppandLength()
        {
            FillBlock(LengthBuffer, BLOCK_SIZE_PADDING, sizeof(ulong));
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
            MemoryStream stream = new(data);
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
        return (index / 16) switch
        {
            0 => (x, y, z) => x & y | ~x & z,
            1 => (x, y, z) => x & z | y & ~z,
            2 => (x, y, z) => x ^ y ^ z,
            3 => (x, y, z) => y ^ (x | ~z),
            _ => throw new ArgumentOutOfRangeException(),
        };
    };

    private static readonly Func<int, int> K = (index) =>
    {
        return (index / 16) switch
        {
            0 => index % 16,
            1 => (index * 5 + 1) % 16,
            2 => (index * 3 + 5) % 16,
            3 => (index * 7) % 16,
            _ => throw new ArgumentOutOfRangeException(),
        };
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
        var Block = context.Block;
        var MD = context.MD;
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
        int filled = context.FillBlock();
        while (filled == BLOCK_SIZE)
        {
            ProcessBlock(context);
            filled = context.FillBlock();
        }
        if (filled < BLOCK_SIZE_PADDING)
        {
            context.FillBlock(new Span<byte>(PAD, 0, BLOCK_SIZE_PADDING - filled), filled, BLOCK_SIZE_PADDING - filled);
            context.AppandLength();
            ProcessBlock(context);
        }
        else
        {
            context.FillBlock(new Span<byte>(PAD, 0, BLOCK_SIZE - filled), filled, BLOCK_SIZE - filled);
            ProcessBlock(context);
            context.FillBlock(new Span<byte>(PAD, BLOCK_SIZE - filled, BLOCK_SIZE_PADDING), 0, BLOCK_SIZE_PADDING);
            context.AppandLength();
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
