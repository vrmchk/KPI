namespace LooseIndexFile.Models;

public class IndexFileEntry
{
    public required int Key { get; init; }
    public required int BlockId { get; set; }
}