namespace LooseIndexFile.Models;

public class FileBlock<T>
{
    public required IndexFileEntry?[] IndexFileEntries { get; init; }
    public required FileEntry<T>?[] FileEntries { get; init; }
}