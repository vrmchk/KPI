namespace ColoringABC.Hive;

public class ScoutBee
{
    public ScoutBee(Graph graph)
    {
        Graph = graph;
        AlreadySelected = new HashSet<Vertex>(graph.Vertices.Count);
    }

    public Graph Graph { get; }
    public int SelectedVertexId { get; set; } = -1;
    public HashSet<Vertex> AlreadySelected { get; }

    public Vertex SelectVertex()
    {
        var vertex = Graph.Vertices
            .Where(v => !AlreadySelected.Contains(v))
            .MaxBy(v => v.Degree)!;

        SelectedVertexId = vertex.Id;
        AlreadySelected.Add(vertex);
        return vertex;
    }

    public double GetVertexValue(Vertex vertex, IEnumerable<Vertex> selectedVertices, int onlookersCount)
    {
        var degreeSum = selectedVertices.Sum(v => v.Degree);
        return onlookersCount * ((double)vertex.Degree / degreeSum);
    }

    public static IEnumerable<Vertex> SelectBestVerticesFromGraph(Graph graph, int count)
    {
        return graph.Vertices
            .OrderByDescending(v => v.Degree)
            .Take(count);
    }
}