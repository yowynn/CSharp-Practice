using EnvDTE;
using System.Drawing;
using WynneCsharp.Algorithm.BitmapCompression;
using System.IO;
#pragma warning disable CA1416 // 验证平台兼容性

namespace WynneCsharp
{
    [TestClass]
    public class UnitTest1
    {
        public string Root => Path.GetFullPath(Environment.CurrentDirectory + "\\..\\..\\..\\..\\");

        [TestMethod]
        public void TestMethod1(string imageName = "test.png")
        {
            string filePath = Root + "TestAssets\\Input\\" + imageName;
            string filePath1 = Root + "TestAssets\\Input\\" + imageName + "_o1.png";
            string filePath2 = Root + "TestAssets\\Input\\" + imageName + "_o2.png";
            Bitmap image = (Bitmap)Image.FromFile(filePath);
            var input = new byte[image.Width * image.Height];
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    var texel = image.GetPixel(i, j);
                    var brightness = (byte)(texel.GetBrightness() * byte.MaxValue);
                    image.SetPixel(i, j, Color.FromArgb(brightness, brightness, brightness));
                    input[image.Width * j + i] = brightness;
                }
            image.Save(filePath1);
            var compressed = BTC.Compress(input, image.Width, image.Height, 4);
            var uncompressed = BTC.Uncompress(compressed, image.Width, image.Height, 4);
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    var texel = uncompressed[image.Width * j + i];
                    image.SetPixel(i, j, Color.FromArgb(texel, texel, texel));
                }
            image.Save(filePath2);
        }
    }
}


#pragma warning restore CA1416 // 验证平台兼容性
