using System.Diagnostics;
using Labyrinth.Common;
using Labyrinth.Model;
using Labyrinth.PathSolver;

namespace Labyrinth;

internal static class Program
{
    private static void Main(string[] args)
    {
        string path = Directory.GetCurrentDirectory().Replace(@"Labyrinth\bin", @"LabyrinthMaker\bin") + "-windows\\" +
                      "result.json";
        const bool printSteps = true;

        var maze = Maze.LoadFromFile(path);

        Console.WriteLine(maze + "\n");

        var state = new State(maze, null);
        Console.Write("Choose BFS or A* solving method: ");
        IPathSolver solver = Console.ReadLine()!.ToLower() == "bfs" ? new BfsPathSolver() : new AStarPathSolver();


        var sw = Stopwatch.StartNew();
        var res = solver.Solve(state, printSteps);
        sw.Stop();
        Console.WriteLine($"Algorithm took {sw.ElapsedMilliseconds} ms");

        if (res.State == null)
        {
            Console.WriteLine("There is no way");
        }
        else
        {
            IEnumerable<State> solutionPath = res.State.GetPath();
            Console.WriteLine(solutionPath.First(st => st.Distance == 1).Maze);

            Console.WriteLine($"Iterations: {res.Iterations}");
            Console.WriteLine("Dead ends: " + state.GetTotalImpasses());
            Console.WriteLine("Total states: " + state.GetTotalStates().Count);
            Console.WriteLine("Stored states: " + res.StatesCount);
            Console.WriteLine("Path: ");
            Console.WriteLine(string.Join(", ",
                solutionPath.Reverse().Select(part => part.Maze.Selected.Coordinate.ToString())));
        }

        Console.ReadLine();
    }
}