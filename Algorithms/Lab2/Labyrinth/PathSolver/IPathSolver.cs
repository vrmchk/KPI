using Labyrinth.Model;

namespace Labyrinth.PathSolver;
internal interface IPathSolver
{
    SearchResult Solve(State state, bool printSteps = false);
}
