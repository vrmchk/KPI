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

    public int[] Solve()
    {
        for (int i = 0; i < MaxIterations; i++)
        {
            List<Bee> bees = new List<Bee>();

            // initialize employed bees
            for (int j = 0; j < _numBees; j++)
            {
                bees.Add(new EmployedBee(_lowerBound, _upperBound));
            }

            //initialize onlookers
            for (int j = 0; j < _numOnlookers; j++)
            {
                bees.Add(new OnlookerBee(_lowerBound, _upperBound));
            }

            //initialize scouts
            for (int j = 0; j < _numScouts; j++)
            {
                bees.Add(new ScoutBee());
            }

            // Send employed bees to generate new solutions
            List<int[]> newSolutions = new List<int[]>();
            foreach (var bee in bees.Where(b => b is EmployedBee))
            {
                newSolutions.Add(bee.GenerateSolution(_graph, _bestSolution));
            }
            // Send onlooker bees to generate new solutions
            foreach (var bee in bees.Where(b => b is OnlookerBee))
            {
                newSolutions.Add(bee.GenerateSolution(_graph, _bestSolution));
            }
            // Send scout bees to generate new solutions
            foreach (var bee in bees.Where(b => b is ScoutBee))
            {
                newSolutions.Add(bee.GenerateSolution(_graph));
            }
            // select the best solution
            foreach (var solution in newSolutions)
            {
                double fitness = Fitness(solution);
                if (fitness < _bestFitness)
                {
                    _bestFitness = fitness;
                    _bestSolution = solution;
                }
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
                if (solution[i] == solution[_graph[i][j]])
                    violations++;
            }
        }
        return (double)violations / _totalEdges;
    }
}
