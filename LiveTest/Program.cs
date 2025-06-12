using PowerStrings;
using System.Diagnostics;

var str = PowerString.From("Hello, World!");
str.Replace('o', '0');
Console.WriteLine(str);

var stopwatch = new Stopwatch();
stopwatch.Start();
var testString = "Hallo Welt!";
for (int i = 0; i < 1_000_000; i++)
{
    if (testString.Contains("Welt"))
    {
        testString = testString.Replace("Welt!", "World");
    }
    else
    {
        testString = testString.Replace("World", "Welt!");
    }
}
stopwatch.Stop();

var stopwatch2 = new Stopwatch();
stopwatch2.Start();
var testPowerString = PowerString.From("Hallo Welt!");
for (int i = 0; i < 1_000_000; i++)
{
    if (testPowerString.Contains("Welt"))
    {
        testPowerString.Replace("Welt!", "World");
    }
    else
    {
        testPowerString.Replace("World", "Welt!");
    }
}
stopwatch2.Stop();

Console.WriteLine($"String: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"PowerString: {stopwatch2.ElapsedMilliseconds} ms");
Console.WriteLine($"PowerString is {(stopwatch2.ElapsedMilliseconds < stopwatch.ElapsedMilliseconds ? "faster" : "slower")} than String.");

foreach (var item in PowerString.From("Hello, World!"))
{
    Console.Write(item);
}

Console.ReadKey();