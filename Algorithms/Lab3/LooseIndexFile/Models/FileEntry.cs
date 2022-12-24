namespace LooseIndexFile.Models;

public class FileEntry<T>
{
    public required int Key { get; set; }
    public required T Value { get; set; }
}