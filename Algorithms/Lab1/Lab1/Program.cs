using System.Diagnostics;
using Lab1.Generators;
using Lab1.Sorters;

namespace Lab1;

internal class Program
{
    private static string _path = string.Empty;
    private static IFileGenerator _generator = null!;
    private static IFileSorter _sorter = null!;
    private const int linesCount = 100_000_000;

    public static void Main(string[] args)
    {
        Console.Write(new string('-', Console.BufferWidth));
        Console.WriteLine("Normal");
        Normal();
        Console.Write(new string('-', Console.BufferWidth));
        Console.WriteLine("Modified");
        Modified();
    }

    static void Normal()
    {
        _path = @"D:\KPI\Algorithms\Lab1\Lab1\TextFile.txt";

        _generator = new TextFileGenerator();
        _generator.GenerateBySize(_path, 10);
        Console.WriteLine("Generated");
        _sorter = new TextFileSorter();
        var sw = Stopwatch.StartNew();
        _sorter.Sort(_path, out string sortedFileName);
        sw.Stop();
        Console.WriteLine($"Sorted, file: {sortedFileName}, seconds: {sw.Elapsed.TotalSeconds}");
    }

    static void Modified()
    {
        _path = @"D:\KPI\Algorithms\Lab1\Lab1\BinaryFile.dat";

        _generator = new BinaryFileGenerator();
        _generator.GenerateByLinesCount(_path, linesCount);
        Console.WriteLine("Generated");
        _sorter = new BinaryFileSorter();
        var sw = Stopwatch.StartNew();
        ((BinaryFileSorter) _sorter).SortParts(_path, "sorted.dat", linesCount, linesCount / 8);
        _sorter.Sort("sorted.dat", out string sortedFileName);
        sw.Stop();
        Console.WriteLine($"Sorted, file: {sortedFileName}, seconds: {sw.Elapsed.TotalSeconds}");
        FileWorker.ShowContent(sortedFileName, 20);
    }
}