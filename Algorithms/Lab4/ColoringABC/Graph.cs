namespace ColoringABC;

public class Graph : ICloneable
{
    public List<Vertex> Vertices { get; init; } = new();
    public int ChromaticNumber => Vertices.Select(v => v.ColorValue).Distinct().Count();
    public bool IsValid => Vertices.All(v => v.IsValid);
    
    public Vertex AddVertex(Vertex vertex)
    {
        if (Vertices.Any(v => v.Id == vertex.Id))
            throw new InvalidOperationException(nameof(vertex));

        Vertices.Add(vertex);
        return Vertices.Find(v => v.Equals(vertex))!;
    }
    
    public void AddEdge(Vertex firstVertex, Vertex secondVertex)
    {
        var first = Vertices.First(vertex => vertex.Id == firstVertex.Id);
        var second = Vertices.First(vertex => vertex.Id == secondVertex.Id);

        if (first == null || second == null)
            throw new ArgumentOutOfRangeException(nameof(firstVertex), "One of passed parameters out of bound");

        first.LinkTo(second);
    }

    public object Clone()
    {
        var adjacencyArray = new int[Vertices.Count][];
        var colors = Vertices.Select(vertex => vertex.ColorValue).ToArray();
        foreach (var vertex in Vertices)
        {
            adjacencyArray[vertex.Id] = new int[vertex.Degree];
            int i = 0;
            foreach (var linked in vertex.Neighbors)
            {
                adjacencyArray[vertex.Id][i++] = linked.Id;
            }
        }

        var graph = FromJaggedArray(adjacencyArray);

        graph.Vertices.ForEach(vertex => vertex.ColorValue = colors[graph.Vertices.IndexOf(vertex)]);

        return graph;
    }

    public override string ToString()
    {
        return string.Join("\n", Vertices);
    }

    public static Graph GenerateGraph(int numVertices, int maxEdges)
    {
        var random = new Random();
        var graph = new Graph();

        for (int i = 0; i < numVertices; i++)
        {
            graph.AddVertex(new Vertex(i));
        }

        var orderedRandomly = graph.Vertices.OrderBy(_ => random.Next(numVertices)).ToList();
        for (int vertexIndex = 0; vertexIndex < orderedRandomly.Count; vertexIndex++)
        {
            var first = orderedRandomly[vertexIndex];

            var nextIndex = vertexIndex != orderedRandomly.Count - 1 ? vertexIndex + 1 : 0;
            var second = orderedRandomly[nextIndex];

            graph.AddEdge(first, second);
        }

        while (graph.Vertices.All(vertex => vertex.Degree != maxEdges))
        {
            orderedRandomly = graph.Vertices.OrderBy(_ => random.Next(numVertices)).ToList();
            var first = orderedRandomly.First();
            var second = orderedRandomly.Last();

            graph.AddEdge(first, second);
        }
        return graph;
    }

    public static Graph FromJaggedArray(int[][] array)
    {
        var graph = new Graph();

        for (int i = 0; i < array.Length; i++)
        {
            graph.AddVertex(new Vertex(i));
        }

        for (int i = 0; i < array.Length; i++)
        {
            for (int j = 0; j < array[i].Length; j++)
            {
                graph.Vertices[i].LinkTo(graph.Vertices[array[i][j]]);
            }
        }

        return graph;
    }
}