// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");
if (args.Length == 1)
{
    var path = args[0];
    var file = new FileInfo(path);
    Console.WriteLine(MD5.CheckSum(file));
}
else
{
    var path = @"C:\Users\Wynn\Desktop\md5_a.dat";
    //var path = @"C:\Users\Wynn\Desktop\111.txt";
    var file = new FileInfo(path);
    Console.WriteLine(MD5.CheckSum(file));
}
Console.Read();