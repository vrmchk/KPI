using System.Text;
using Labyrinth.Enums;
using Labyrinth.Model;

namespace Labyrinth.Common;
internal static class Extensions
{
    public static Cell ToCell(this CompressedCell source)
    {
        return new Cell(source.Type, (source.Coordinate.Item2, source.Coordinate.Item1));
    }
    
    public static void PrintState(this State source)
    {
        var sb = new StringBuilder();
        sb.AppendLine(new string('-', Console.BufferWidth));
        sb.AppendLine(source.Maze.ToString());
        sb.AppendLine(new string('-', Console.BufferWidth));
        Console.WriteLine(sb.ToString());
        Thread.Sleep(1);
    }
    
    public static List<T> ToList<T>(this T[,] source)
    {
        var list = new List<T>();
        for (int rowIndex = 0; rowIndex < source.GetLength(0); rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < source.GetLength(1); columnIndex++)
            {
                list.Add(source[rowIndex, columnIndex]);
            }
        }

        return list;
    }

    public static List<State> GetTotalStates(this State state, List<State>? result = null)
    {
        result ??= new List<State>();
        result.Add(state);
        var neighbors = state.GetNeighbors();
        foreach (var child in neighbors)
        {
            child.GetTotalStates(result);
        }
        return result;
    }
    
    public static int GetTotalImpasses(this State state)
    {
        return state.Maze.Cells.ToList().Count(cell => cell.Coordinate.X != 0 && cell.Coordinate.Y != 0 && cell.Coordinate.X != state.Maze.Cells.GetLength(0) - 1 && cell.Coordinate.Y != state.Maze.Cells.GetLength(1) && state.Maze.Neighbors(cell.Coordinate).Count(n => n == null || n.Type == CellType.Wall) == 3);
    }
}
