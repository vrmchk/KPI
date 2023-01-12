namespace Lab4.Hive;

abstract class Bee
{
    protected readonly Random Rand;

    public Bee()
    {
        Rand = new Random();
    }
    public virtual int[] GenerateSolution(int[][] graph, int[]? currentSolution = null)
    {
        int[] bee = new int[graph.Length];
        for (int i = 0; i < graph.Length; i++)
        {
            bee[i] = Rand.Next(graph.Length) + 1;
        }
        return bee;
    }
    public virtual double CalculateFitness(int[] solution, int[][] graph)
    {
        int violations = 0;
        for (int i = 0; i < graph.Length; i++)
        {
            for (int j = 0; j < graph[i].Length; j++)
            {
                if (solution[i] == solution[graph[i][j]])
                    violations++;
            }
        }
        int totalEdges = graph.Select(x => x.Length).Sum();
        return (double)violations / totalEdges;
    }
}
