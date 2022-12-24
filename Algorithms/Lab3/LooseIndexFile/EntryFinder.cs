using System.Text.Json;
using LooseIndexFile.Models;

namespace LooseIndexFile;

public class EntryFinder<T>
{
    private readonly LooseIndexFileConfiguration _configuration;

    public EntryFinder(LooseIndexFileConfiguration configuration)
    {
        _configuration = configuration;
    }

    public FileEntry<T>? Find(int key)
    {
        var indexFileEntry = FindIndexFileEntry(key);
        return indexFileEntry is null ? null : Find(indexFileEntry);
    }
    
    public IndexFileEntry? FindIndexFileEntry(int key)
    {
        int min = 0;
        var filesCount = File.ReadLines(_configuration.IndexFileLocation).Count();
        int max = filesCount;
        while (min <= max)
        {
            int mid = (min + max) / 2;
            string line = GetLine(_configuration.IndexFileLocation, mid);
            var entry = JsonSerializer.Deserialize<IndexFileEntry>(line)!;
            if (key == entry.Key)
                return entry;

            if (key < entry.Key)
                max = mid - 1;

            else
                min = mid + 1;
        }

        return null;
    }

    private FileEntry<T> Find(IndexFileEntry indexFileEntry)
    {
        var lineIndex = FindLineIndex(indexFileEntry);
        string line = GetLine(_configuration.MainFileLocation, lineIndex);
        return JsonSerializer.Deserialize<FileEntry<T>>(line)!;
    }

    private int FindLineIndex(IndexFileEntry indexFileEntry)
    {
        int endOfBlockInFile = indexFileEntry.BlockId / _configuration.BlockSize * _configuration.ExtendedBlockSize - 1;
        return endOfBlockInFile - (indexFileEntry.BlockId - indexFileEntry.Key);
    }
    
    private string GetLine(string fileLocation, int index)
    {
        var linesCount = File.ReadLines(fileLocation).Count();
        if (index >= linesCount)
            throw new IndexOutOfRangeException(nameof(index));
        
        using var sr = new StreamReader(fileLocation);
        for (int i = 0; i < index; i++)
        {
            sr.ReadLine();
        }
        return sr.ReadLine()!;
    }
}