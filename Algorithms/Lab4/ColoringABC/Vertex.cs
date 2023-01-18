namespace ColoringABC;

public class Vertex : IEquatable<Vertex>
{
    public int Id { get; init; }
    public int ColorValue { get; set; }
    public HashSet<Vertex> Neighbors { get; init; }
    
    public int Degree => Neighbors.Count;
    public bool IsValid => Neighbors.All(v => v.ColorValue != ColorValue);

    public Vertex(int id, int colorValue = -1)
    {
        Id = id;
        ColorValue = colorValue;
        Neighbors = new HashSet<Vertex>();
    }

    public void LinkTo(Vertex neighbor)
    {
        Neighbors.Add(neighbor);
        neighbor.Neighbors.Add(this);
    }

    public override string ToString()
    {
        return $"{nameof(Id)}: {Id}, {nameof(ColorValue)}: {ColorValue}, {nameof(Degree)}: {Degree}, {nameof(Neighbors)}: {string.Join(" ", Neighbors.Select(n => n.Id))}";
    }

    public bool Equals(Vertex? other)
    {
        return other?.Id == Id;
    }
    public override bool Equals(object? obj)
    {
        return Equals(obj as Vertex);
    }
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}