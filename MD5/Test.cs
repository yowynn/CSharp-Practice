using System;
using System.IO;


internal class Test
{
    public class RandomWriter
    {
        private const int BUFFER_SIZE = 1024 * 1024;
        private readonly Random random;
        private readonly byte[] buffer;
        private int pointer;
        public RandomWriter()
        {
            random = new Random();
            buffer = new byte[BUFFER_SIZE];
            pointer = BUFFER_SIZE;
        }

        public void WriteBytes(Stream stream, ulong size)
        {
            while (size > 0)
            {
                if (pointer == BUFFER_SIZE)
                {
                    random.NextBytes(buffer);
                    pointer = 0;
                }
                var rest = (int)Math.Min((ulong)(BUFFER_SIZE - pointer), size);
                stream.Write(new Span<byte>(buffer, pointer, rest));
                stream.Flush();
                pointer += rest;
                size -= (ulong)rest;
            }
        }
    }
    public static void GenSmallFiles(int fileCount = 200, string path = "test/data")
    {
        RandomWriter writer = new();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        for (int i = 0; i < fileCount; i++)
        {
            var s = File.Create($"{path}{i}");
            writer.WriteBytes(s, (ulong)i);
            s.Dispose();
        }
    }

    public static void GenLargeFile(ulong fileSize = 1UL << 30, string path = "test/bigfile")
    {
        RandomWriter writer = new();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var s = File.Create(path);
        writer.WriteBytes(s, fileSize);
        s.Dispose();
    }
}
