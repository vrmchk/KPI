namespace LooseIndexFile.Models;

public class LooseIndexFileConfiguration
{
    private int _blockSize;
    private const int MinBlockSize = 10;

    public LooseIndexFileConfiguration(string? dataFolder = null, int blockSize = MinBlockSize,
        string? blockTerminator = null)
    {
        IndexFileLocation = dataFolder is null ? "index.txt" : @$"{dataFolder}\index.txt";
        MainFileLocation = dataFolder is null ? "data.txt" : @$"{dataFolder}\data.txt";
        AutoincrementFileLocation = dataFolder is null ? "autoincrement.json" : @$"{dataFolder}\autoincrement.json";
        BlockTerminator = blockTerminator ?? "NEW BLOCK";
        BlockSize = blockSize;
    }

    public LooseIndexFileConfiguration(string indexFileLocation, string mainFileLocation,
        string autoincrementFileLocation, int blockSize, string blockTerminator)
    {
        IndexFileLocation = indexFileLocation;
        MainFileLocation = mainFileLocation;
        AutoincrementFileLocation = autoincrementFileLocation;
        BlockTerminator = blockTerminator;
        BlockSize = blockSize;
    }

    public string IndexFileLocation { get; init; }
    public string MainFileLocation { get; init; }
    public string AutoincrementFileLocation { get; init; }
    public string BlockTerminator { get; init; }

    public int BlockSize
    {
        get => _blockSize;
        set
        {
            if (value < MinBlockSize)
            {
                throw new InvalidOperationException($"Minimal block size value is {MinBlockSize}");
            }

            if (_blockSize == default)
            {
                try
                {
                    var autoincrement = Autoincrement.ReadFromFile(AutoincrementFileLocation);
                    _blockSize = autoincrement.BlockSize;
                }
                catch (Exception ex)
                {
                    _blockSize = value;
                }
            }
            else
            {
                _blockSize = value;
                var autoincrement = Autoincrement.ReadFromFile(AutoincrementFileLocation);
                autoincrement.BlockSize = value;
                autoincrement.SaveToFile(AutoincrementFileLocation);
            }
        }
    }

    public int ResizeGrowth => 2;
    public int BlockExtensionSize => 1;
    public int ExtendedBlockSize => BlockSize + BlockExtensionSize;
}