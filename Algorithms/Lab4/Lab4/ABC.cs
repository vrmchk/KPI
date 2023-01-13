using Lab4.Hive;

namespace Lab4;

class ABC
{
    private readonly int[][] _graph;

    private int[] _bestSolution;
    private double _bestFitness;
    private readonly int _numBees;
    private readonly int _numOnlookers;
    private readonly int _numScouts;
    private readonly int _lowerBound;
    private readonly int _upperBound;
    private const int MaxIterations = 1_000;
    private readonly int _totalEdges;
    
    public ABC(int[][] graph, int numBees, int numOnlookers, int numScouts, int lowerBound, int upperBound)
    {
        _graph = graph;
        _totalEdges = _graph.Select(x => x.Length).Sum();
        _numBees = numBees;
        _numOnlookers = numOnlookers;
        _numScouts = numScouts;
        _lowerBound = lowerBound;
        _upperBound = upperBound;
        _bestSolution = InitializePopulation();
        _bestFitness = Fitness(_bestSolution);
    }

    public int[] Solve(bool printIterations = false)
    {
        for (int i = 0; i < MaxIterations; i++)
        {
            List<ScoutBee> scoutBees = new List<ScoutBee>();
            List<Bee> otherBees = new List<Bee>();

            for (int j = 0; j < _numBees; j++)
                otherBees.Add(new EmployedBee(_lowerBound, _upperBound));

            for (int j = 0; j < _numOnlookers; j++)
                otherBees.Add(new OnlookerBee(_lowerBound, _upperBound));

            for (int j = 0; j < _numScouts; j++)
                scoutBees.Add(new ScoutBee());

            HashSet<int[]> newSolutions = new HashSet<int[]>();
            foreach (Bee bee in otherBees)
            {
                if (bee is ScoutBee)
                    continue;
                newSolutions.Add(bee.GenerateSolution(_graph, _bestSolution));
            }
            foreach (ScoutBee bee in scoutBees)
            {
                newSolutions.Add(bee.GenerateSolution(_graph));
            }

            Solution bestSolution = newSolutions.Select(s => new Solution(_graph, s)).MaxBy(solution => solution.Fitness)!;
            
            _bestSolution = bestSolution.ColorSet;
            _bestFitness = bestSolution.Fitness;
            
            if (printIterations && i % 20 == 0)
            {
                Console.WriteLine($"Iteration: {i}, Fitness: {Math.Round(_bestFitness, 5)}, UsedColors: {_bestSolution.Distinct().Count()}");
            }
        }
        return _bestSolution;
    }
    private int[] InitializePopulation()
    {
        var initialSolution = new ScoutBee().GenerateSolution(_graph);
        return initialSolution;
    }
    private double Fitness(int[] solution)
    {
        int violations = 0;
        for (int i = 0; i < _graph.Length; i++)
        {
            for (int j = 0; j < _graph[i].Length; j++)
            {
                if (solution[i] == solution[_graph[i][j]] && i != j)
                    violations++;
            }
        }
        int chromaticNumber = ChromaticNumber(_graph);
        int numDistinctColors = solution.Distinct().Count();
        int totalColors = chromaticNumber * _graph.Length;
        return 1 - (double)(violations + numDistinctColors) / totalColors;
    }
    private static int ChromaticNumber(int[][] graph)
    {
        int[] colors = new int[graph.Length];
        Array.Fill(colors, -1);
        colors[0] = 0;

        bool[] availableColors = new bool[graph.Length];
        for (int i = 0; i < graph.Length; i++)
        {
            Array.Fill(availableColors, true);
            for (int j = 0; j < graph[i].Length; j++)
            {
                int neighbor = graph[i][j];
                if (colors[neighbor] != -1)
                    availableColors[colors[neighbor]] = false;
            }

            int color = 0;
            for (int j = 0; j < availableColors.Length; j++)
            {
                if (availableColors[j])
                {
                    color = j;
                    break;
                }
            }
            colors[i] = color;
        }

        return colors.Max() + 1;
    }
}