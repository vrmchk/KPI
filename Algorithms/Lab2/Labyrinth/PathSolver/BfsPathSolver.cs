using Labyrinth.Common;
using Labyrinth.Model;

namespace Labyrinth.PathSolver;
internal class BfsPathSolver : IPathSolver
{
    private int _iteration = 0;
    
    public SearchResult Solve(State state, bool printSteps = false)
    {
        int states = state.GetDepth().Count;
        return Bfs(state, states, printSteps);
    }

    private SearchResult Bfs(State state, int depth, bool printSteps)
    {
        var visited = new HashSet<State>();
        var queue = new Queue<State>();
        queue.Enqueue(state);
        while (queue.Count != 0)
        {
            var current = queue.Dequeue();
            
            if (visited.Contains(current))
                continue;

            visited.Add(current);
            
            if (current.Generation >= depth) 
                return new SearchResult(null, int.MaxValue, int.MaxValue);

            _iteration++;
            if (printSteps)
                current.PrintState();

            if (current.Distance == 1)
                return new SearchResult(current, _iteration, queue.Count + current.Generation);

            foreach (var neighbor in current.GetNeighbors())
            {
                if (!visited.Contains(neighbor))
                    queue.Enqueue(neighbor);
            }
        }
        
        return new SearchResult(null, int.MaxValue ,int.MaxValue);
    }
}
