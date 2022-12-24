using LooseIndexFile;
using LooseIndexFile.Models;

namespace ConsoleInterface;

internal static class Program
{
    private static readonly Random Random = new();

    static void Main(string[] args)
    {
        string dataFolder = @"D:\Lab3Data";
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        var looseIndexFile = new LooseIndexFile<int>(new LooseIndexFileConfiguration(dataFolder));

        FillFile(looseIndexFile, 1000);
        Console.WriteLine("Added");
    }


    static void FillFile(LooseIndexFile<int> looseIndexFile, int itemsCount)
    {
        var values = Enumerable.Range(0, itemsCount).OrderBy(_ => Random.Next(0, itemsCount));
        foreach (var value in values)
        {
            looseIndexFile.Add(value);
        }
    }
}