// See https://aka.ms/new-console-template for more information
using System;
using System.IO;

string Calc(string filePath = "")
{
    var path = args.Length == 1 ? args[0] : filePath;
    var file = new FileInfo(path);
    return MD5.CheckSum(file);
}


//Test.GenLargeFile(1UL << 33, $"test/bigfile");
Console.WriteLine(Calc(@"test/bigfile"));
//Test.GenLargeFile(1UL << 20, $"test/bigfile2");
//Console.WriteLine(Calc(@"test/bigfile2"));
//Console.WriteLine(Calc(@"C:\Users\Wynn\Desktop\md5_a.dat"));

Console.WriteLine("Hello, World!");
Console.Read();