using System.Text.Json;

namespace LooseIndexFile.Models;

public class Autoincrement
{
    public required int LastKey { get; set; }
    public required int LastBlockId { get; set; }
    public required int BlocksCount { get; set; }

    public static Autoincrement ReadFromFile(string path)
    {
        return JsonSerializer.Deserialize<Autoincrement>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Sensitive data has been corrupted");
    }
    
    public void SaveToFile(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(this));
    }

}