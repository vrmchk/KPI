namespace LooseIndexFile.Models;

public class LooseIndexFileConfiguration
{
    private const int MinBlockSize = 10;
    
    public LooseIndexFileConfiguration(string? dataFolder = null, int blockSize = MinBlockSize, string? blockTerminator = null)
    {
        IndexFileLocation = dataFolder is null ? "index.txt" : @$"{dataFolder}\index.txt";
        MainFileLocation = dataFolder is null ? "data.txt" : @$"{dataFolder}\data.txt";
        AutoincrementFileLocation = dataFolder is null ? "autoincrement.json" : @$"{dataFolder}\autoincrement.json";
        BlockTerminator = blockTerminator ?? "NEW BLOCK";
        BlockSize = blockSize >= MinBlockSize ? blockSize : MinBlockSize;
    }

    public LooseIndexFileConfiguration(string indexFileLocation, string mainFileLocation, string autoincrementFileLocation, int blockSize, string blockTerminator)
    {
        IndexFileLocation = indexFileLocation;
        MainFileLocation = mainFileLocation;
        AutoincrementFileLocation = autoincrementFileLocation;
        BlockSize = blockSize >= MinBlockSize ? blockSize : MinBlockSize;
        BlockTerminator = blockTerminator;
    }

    public string IndexFileLocation { get; init; }
    public string MainFileLocation { get; init; }
    public string AutoincrementFileLocation { get; init; }
    public string BlockTerminator { get; init; }
    public int BlockSize { get; set; }
    public int ResizeGrowth => 2;
    public int BlockExtensionSize => 1;
    public int ExtendedBlockSize => BlockSize + BlockExtensionSize;
}