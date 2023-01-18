using ColoringABC.Hive;

namespace ColoringABC;

public class ABC
{
    private readonly Graph _initialGraph;
    private readonly int _verticesCount;
    private readonly int _scoutsCount;
    private readonly int _onlookersCount;
    private readonly List<ScoutBee> _scouts;
    private readonly List<OnlookerBee> _onlookers;
    private readonly Queue<int> _availableColors;
    private readonly List<int> _usedColors;

    public ABC(Graph graph, int scoutsCount, int onlookersCount)
    {
        _initialGraph = graph;
        _verticesCount = graph.Vertices.Count;
        _scoutsCount = scoutsCount;
        _onlookersCount = onlookersCount;
        _scouts = Enumerable.Range(0, scoutsCount).Select(_ => new ScoutBee((Graph)graph.Clone())).ToList();
        _onlookers = Enumerable.Range(0, onlookersCount).Select(_ => new OnlookerBee()).ToList();
        _availableColors = new Queue<int>(Enumerable.Range(0, _verticesCount));
        _usedColors = new List<int>(_verticesCount);
    }

    public Graph Solve(bool printIterations = false)
    {
        SelectFirstVertices();
        var iteration = 0;
        while (_scouts.Any(s => s.AlreadySelected.Count != _verticesCount))
        {
            var scoutsVertices = _scouts.ToDictionary(s => s, s => s.SelectVertex());
            var verticesValues = scoutsVertices.Select(pair => new
            {
                Vertex = pair.Value,
                Value = pair.Key.GetVertexValue(pair.Value, scoutsVertices.Values, _onlookersCount)
            });

            foreach (var verticesValue in verticesValues)
            {
                var onlookerIndex = 0;
                foreach (var neighbor in verticesValue.Vertex.Neighbors)
                {
                    if (onlookerIndex >= verticesValue.Value - 1)
                    {
                        break;
                    }

                    _onlookers[onlookerIndex++].SetVertexColor(neighbor, _usedColors, _availableColors);
                }

                _onlookers[++onlookerIndex].SetVertexColor(verticesValue.Vertex, _usedColors, _availableColors);
            }

            if (printIterations && iteration++ % 20 == 0)
            {
                PrintIteration(iteration);
            }
        }

        return _scouts.Select(s => s.Graph).MinBy(g => g.ChromaticNumber)!;
    }

    private void SelectFirstVertices()
    {
        var bestVertices = ScoutBee.SelectBestVerticesFromGraph(_initialGraph, _scoutsCount).ToList();
        for (int i = 0; i < _scoutsCount; i++)
        {
            _scouts[i].SelectedVertexId = bestVertices[i].Id;
            _scouts[i].AlreadySelected.Add(bestVertices[i]);
        }
    }

    private void PrintIteration(int iteration)
    {
        Console.WriteLine($"==============Iteration: {iteration}==============");
        for (int i = 0; i < _scouts.Count; i++)
        {
            Console.WriteLine($"=================Scout #{i}=================");
            Console.WriteLine(_scouts[i].Graph.IsValid
                ? $"ChromaticNumber: {_scouts[i].Graph.ChromaticNumber}"
                : "Solution is incorrect");
        }

        Console.WriteLine();
    }
}