namespace Lab4.Hive;

class OnlookerBee : Bee
{
    private readonly int _upperBound;
    private readonly int _lowerBound;

    public OnlookerBee(int lowerBound, int upperBound)
    {
        _lowerBound = lowerBound;
        _upperBound = upperBound;
    }

    public override int[] GenerateSolution(int[][] graph, int[]? currentSolution = null)
    {
        if (currentSolution == null)
            throw new ArgumentNullException(nameof(currentSolution));

        int[] newSolution = (int[])currentSolution.Clone();
        int randomIndex = Rand.Next(graph.Length);
        int newValue = currentSolution[randomIndex] + Rand.Next(-3, 3);
        
        if (newValue > _upperBound)
            newValue = _upperBound;
        else if (newValue < _lowerBound)
            newValue = _lowerBound;
        
        newSolution[randomIndex] = newValue;
        double newSolutionFitness = CalculateFitness(newSolution, graph);
        return ShouldAccept(newSolutionFitness, CalculateFitness(currentSolution, graph)) ? newSolution : currentSolution;
    }

    private bool ShouldAccept(double newSolutionFitness, double currentSolutionFitness)
    {
        double delta = newSolutionFitness - currentSolutionFitness;
        double probability = Math.Exp(-delta);
        return Rand.NextDouble() < probability;
    }
}
