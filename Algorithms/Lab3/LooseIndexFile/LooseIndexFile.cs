using System.Text.Json;
using LooseIndexFile.Models;

namespace LooseIndexFile;

public class LooseIndexFile<T>
{
    private readonly LooseIndexFileConfiguration _configuration;
    private readonly EntryFinder<T> _entryFinder;

    public LooseIndexFile(LooseIndexFileConfiguration configuration)
    {
        _configuration = configuration;
        _entryFinder = new EntryFinder<T>(configuration);
    }

    #region CRUD

    public FileEntry<T>? Get(int key)
    {
        try
        {
            return _entryFinder.Find(key);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public FileEntry<T> Add(T value)
    {
        int key = GetKey();

        // if (GetBlocksCount() >= _configuration.BlockSize && BlockIsFilled(ComputeBlockId(key)))
        //     RebuildFiles();

        var indexFileEntry = new IndexFileEntry { Key = key, BlockId = GetBlockId(key) };
        var fileEntry = new FileEntry<T> { Key = key, Value = value };
        AddToIndexFile(_configuration.IndexFileLocation, indexFileEntry);
        AddToMainFile(_configuration.MainFileLocation, fileEntry);
        return fileEntry;
    }
 
    public FileEntry<T> Update(int key, T value)
    {
        return Update(new FileEntry<T> { Key = key, Value = value });
    }

    public FileEntry<T> Update(FileEntry<T> entry)
    {
        try
        {
            var existingEntry = _entryFinder.Find(entry.Key);
            if (entry is null)
                throw new InvalidOperationException("Entry with this key doesn't exist");
            ReplaceLine(JsonSerializer.Serialize(existingEntry), JsonSerializer.Serialize(entry));
            return entry;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Entry with this key doesn't exist", ex);
        }
    }

    public void Delete(int key)
    {
        try
        {
            var indexFileEntry = _entryFinder.FindIndexFileEntry(key);
            var fileEntry = _entryFinder.Find(key);
            if (indexFileEntry is null || fileEntry is null)
                throw new InvalidOperationException("Entry with this key doesn't exist");

            RemoveIndexFileEntry(indexFileEntry);
            ReplaceLine(JsonSerializer.Serialize(fileEntry), string.Empty);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Entry with this key doesn't exist", ex);
        }
    }

    #endregion

    #region KeyAndBlockIdOperations

    private int GetKey()
    {
        if (!File.Exists(_configuration.AutoincrementFileLocation))
        {
            new Autoincrement
                {
                    LastKey = 0,
                    LastBlockId = 0,
                    BlockSize = _configuration.BlockSize,
                    BlocksCount = 0
                }
                .SaveToFile(_configuration.AutoincrementFileLocation);
        }

        var autoincrement = Autoincrement.ReadFromFile(_configuration.AutoincrementFileLocation);

        int key = ++autoincrement.LastKey;
        autoincrement.SaveToFile(_configuration.AutoincrementFileLocation);
        return key;
    }

    private int GetBlockId(int key)
    {
        var autoincrement = Autoincrement.ReadFromFile(_configuration.AutoincrementFileLocation);
        var blockId = ComputeBlockId(key);
        if (blockId > autoincrement.LastBlockId)
        {
            WriteStartOfNewBlock();
            autoincrement.LastBlockId = blockId;
            autoincrement.BlocksCount++;
            autoincrement.SaveToFile(_configuration.AutoincrementFileLocation);
        }

        return blockId;
    }

    private int ComputeBlockId(int key)
    {
        return (int)Math.Ceiling(key / (decimal)_configuration.BlockSize) * _configuration.BlockSize;
    }

    private bool BlockIsFilled(int blockId)
    {
        return blockId > Autoincrement.ReadFromFile(_configuration.AutoincrementFileLocation).LastBlockId;
    }

    private int GetBlocksCount()
    {
        try
        {
            var autoincrement = Autoincrement.ReadFromFile(_configuration.AutoincrementFileLocation);
            return autoincrement.BlocksCount;
        }
        catch (FileNotFoundException ex)
        {
            return 0;
        }
    }

    #endregion

    #region FileRebuild

    private void RebuildFiles()
    {
        var blocks = EnumerateRebuiltBlocks();
        string indexTempFile = Path.GetTempFileName();
        string mainTempFile = Path.GetTempFileName();
        foreach (var block in blocks)
        {
            var indexFileEntries = block.IndexFileEntries;
            foreach (var indexFileEntry in indexFileEntries.Where(e => e is not null))
            {
                AddToIndexFile(indexTempFile, indexFileEntry!);
            }

            AddBlock(mainTempFile, block);
        }

        File.Move(indexTempFile, _configuration.IndexFileLocation, overwrite: true);
        File.Move(mainTempFile, _configuration.MainFileLocation, overwrite: true);
        _configuration.BlockSize *= _configuration.ResizeGrowth;
    }

    private IEnumerable<FileBlock<T>> EnumerateRebuiltBlocks()
    {
        using var indexReader = new StreamReader(_configuration.IndexFileLocation);
        using var mainReader = new StreamReader(_configuration.MainFileLocation);
        var indexFileEntries = new List<IndexFileEntry?>();
        var fileEntries = new List<FileEntry<T>?>();
        var resultBlocks = new List<FileBlock<T>>();
        int idCounter = 0;
        while (!mainReader.EndOfStream || !indexReader.EndOfStream)
        {
            string lineFromMain = mainReader.ReadLine()!;
            if (lineFromMain == _configuration.BlockTerminator)
            {
                if (indexFileEntries.Count > 0 && fileEntries.Count > 0)
                {
                    AddEntries(resultBlocks, indexFileEntries, fileEntries);
                }

                if (resultBlocks.Count == _configuration.ResizeGrowth)
                {
                    var block = CreateNewBlock(resultBlocks, ref idCounter);
                    yield return block;
                }
            }
            else if (lineFromMain == string.Empty)
            {
                indexFileEntries.Add(null);
                fileEntries.Add(null);
            }
            else
            {
                indexFileEntries.Add(JsonSerializer.Deserialize<IndexFileEntry>(indexReader.ReadLine()!)!);
                fileEntries.Add(JsonSerializer.Deserialize<FileEntry<T>>(lineFromMain)!);
            }
        }

        if (indexFileEntries.Count > 0 && fileEntries.Count > 0)
        {
            AddEntries(resultBlocks, indexFileEntries, fileEntries);
        }

        if (resultBlocks.Count != 0)
        {
            var block = CreateNewBlock(resultBlocks, ref idCounter);
            yield return block;
        }
    }

    private void AddEntries(List<FileBlock<T>> fileBlocks, List<IndexFileEntry?> indexFileEntries,
        List<FileEntry<T>?> fileEntries)
    {
        fileBlocks.Add(new FileBlock<T>
        {
            IndexFileEntries = indexFileEntries.ToArray(),
            FileEntries = fileEntries.ToArray()
        });
        indexFileEntries.Clear();
        fileEntries.Clear();
    }

    private FileBlock<T> CreateNewBlock(List<FileBlock<T>> fileBlocks, ref int idCounter)
    {
        int newBlockSize = _configuration.BlockSize * _configuration.ResizeGrowth;
        int newBlockId = ++idCounter * newBlockSize;
        var block = new FileBlock<T>
        {
            IndexFileEntries = fileBlocks.SelectMany(e => e.IndexFileEntries)
                .Select(e =>
                    e is null ? null : new IndexFileEntry { Key = e.Key, BlockId = newBlockId })
                .ToArray(),
            FileEntries = fileBlocks.SelectMany(e => e.FileEntries).ToArray()
        };
        fileBlocks.Clear();
        return block;
    }

    #endregion

    #region IOOperations

    private void AddToIndexFile(string fileLocation, IndexFileEntry indexFileEntry)
    {
        File.AppendAllLines(fileLocation, new[] { JsonSerializer.Serialize(indexFileEntry) });
    }

    private void AddToMainFile(string fileLocation, FileEntry<T> fileEntry)
    {
        File.AppendAllLines(fileLocation, new[] { JsonSerializer.Serialize(fileEntry) });
    }

    private void AddBlock(string fileLocation, FileBlock<T> fileBlock)
    {
        File.AppendAllLines(fileLocation, new[] { _configuration.BlockTerminator }
            .Union(fileBlock.FileEntries.Select(e => e is null ? string.Empty : JsonSerializer.Serialize(e))));
    }

    private void WriteStartOfNewBlock()
    {
        File.AppendAllLines(_configuration.MainFileLocation, new[] { _configuration.BlockTerminator });
    }

    private void RemoveIndexFileEntry(IndexFileEntry indexFileEntry)
    {
        string entryJson = JsonSerializer.Serialize(indexFileEntry);
        string tempFile = Path.GetTempFileName();
        var linesToKeep = File.ReadLines(_configuration.IndexFileLocation).Where(l => l != entryJson);
        File.WriteAllLines(tempFile, linesToKeep);
        File.Delete(_configuration.IndexFileLocation);
        File.Move(tempFile, _configuration.IndexFileLocation);
    }

    private void ReplaceLine(string existingLine, string newLine)
    {
        string tempFile = Path.GetTempFileName();
        foreach (var line in File.ReadLines(_configuration.MainFileLocation))
        {
            File.AppendAllLines(tempFile, line == existingLine ? new[] { newLine } : new[] { line });
        }

        File.Delete(_configuration.MainFileLocation);
        File.Move(tempFile, _configuration.MainFileLocation);
    }

    #endregion
}