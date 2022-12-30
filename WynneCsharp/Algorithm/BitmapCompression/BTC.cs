using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WynneCsharp.Algorithm.BitmapCompression
{
    public class BTC
    {
        /// <summary>
        /// Compresses a grayscale image using the Block Truncating Coding (BTC) algorithm.
        /// </summary>
        /// <param name="image">The grayscale image data to be compressed.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="blockSize">The size of the blocks in pixels (e.g. 8, 16, 32, etc.).</param>
        /// <returns>A byte array containing the BTC-compressed representation of the image.</returns>
        public static byte[] Compress(byte[] image, int width, int height, int blockSize)
        {
            // Create a buffer to hold the compressed data
            byte[] compressedData = new byte[(width * height) / (blockSize * blockSize) * (2 + blockSize * blockSize / 8)];

            // Iterate over the blocks in the image
            for (int blockX = 0; blockX < width / blockSize; blockX++)
            {
                for (int blockY = 0; blockY < height / blockSize; blockY++)
                {
                    // Calculate the mean of the block
                    double mean = 0;
                    for (int x = 0; x < blockSize; x++)
                    {
                        for (int y = 0; y < blockSize; y++)
                        {
                            int pixelIndex = (blockY * blockSize + y) * width + (blockX * blockSize + x);
                            mean += image[pixelIndex];
                        }
                    }
                    mean /= blockSize * blockSize;

                    // Create a bitmap for the block, where each bit corresponds to one texel
                    byte[] bitmap = new byte[blockSize * blockSize / 8];
                    int count = 0;
                    for (int x = 0; x < blockSize; x++)
                    {
                        for (int y = 0; y < blockSize; y++)
                        {
                            int pixelIndex = (blockY * blockSize + y) * width + (blockX * blockSize + x);
                            if (image[pixelIndex] >= mean)
                            {
                                int bitmapIndex = y * blockSize / 8 + x / 8;
                                int bitOffset = x % 8;
                                bitmap[bitmapIndex] |= (byte)(1 << bitOffset);
                                count++;
                            }
                        }
                    }

                    // Calculate the variance of the block
                    double variance = 0;
                    for (int x = 0; x < blockSize; x++)
                    {
                        for (int y = 0; y < blockSize; y++)
                        {
                            int pixelIndex = (blockY * blockSize + y) * width + (blockX * blockSize + x);
                            variance += (image[pixelIndex] - mean) * (image[pixelIndex] - mean);
                        }
                    }
                    variance /= blockSize * blockSize;

                    // Choose the values "a" and "b" such that the mean and variance of the reconstructed block are as close as possible to the original mean and variance
                    int a = (int)Math.Round(mean - Math.Sqrt(variance * count / (blockSize * blockSize - count)));
                    int b = (int)Math.Round(mean + Math.Sqrt(variance / count * (blockSize * blockSize - count)));
                    if (((byte)b) == 255)
                    {
                        Console.WriteLine($"????????? {a} {b}");
                    }

                    // Encode the values of "a", "b", and the bitmap as the compressed representation of the block
                    int dataIndex = (blockY * width / blockSize + blockX) * (2 + blockSize * blockSize / 8);
                    compressedData[dataIndex] = (byte)a;
                    compressedData[dataIndex + 1] = (byte)b;
                    Array.Copy(bitmap, 0, compressedData, dataIndex + 2, blockSize * blockSize / 8);
                }
            }

            return compressedData;
        }

        /// <summary>
        /// Uncompresses a BTC-compressed grayscale image.
        /// </summary>
        /// <param name="compressedData">The BTC-compressed representation of the image.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="blockSize">The size of the blocks in pixels (e.g. 8, 16, 32, etc.).</param>
        /// <returns>A byte array containing the uncompressed image data.</returns>
        public static byte[] Uncompress(byte[] compressedData, int width, int height, int blockSize)
        {
            // Create a buffer to hold the uncompressed data
            byte[] image = new byte[width * height];

            // Iterate over the blocks in the compressed data
            for (int blockX = 0; blockX < width / blockSize; blockX++)
            {
                for (int blockY = 0; blockY < height / blockSize; blockY++)
                {
                    // Decode the values of "a", "b", and the bitmap from the compressed representation of the block
                    int dataIndex = (blockY * width / blockSize + blockX) * (2 + blockSize * blockSize / 8);
                    int a = compressedData[dataIndex];
                    int b = compressedData[dataIndex + 1];
                    byte[] bitmap = new byte[blockSize * blockSize / 8];
                    Array.Copy(compressedData, dataIndex + 2, bitmap, 0, blockSize * blockSize / 8);

                    // Reconstruct the texel values of the block based on the bitmap and the values of "a" and "b"
                    for (int x = 0; x < blockSize; x++)
                    {
                        for (int y = 0; y < blockSize; y++)
                        {
                            int pixelIndex = (blockY * blockSize + y) * width + (blockX * blockSize + x);
                            int bitmapIndex = y * blockSize / 8 + x / 8;
                            int bitOffset = x % 8;
                            if ((bitmap[bitmapIndex] & (1 << bitOffset)) != 0)
                            {
                                image[pixelIndex] = (byte)b;
                            }
                            else
                            {
                                image[pixelIndex] = (byte)a;
                            }
                        }
                    }
                }
            }

            return image;
        }
    }
}
