using Labyrinth.Enums;

namespace Labyrinth.Model;

internal class Cell : ICloneable
{
    public Cell(CellType type, (int Y, int X) coordinate)
    {
        Type = type;
        Coordinate = coordinate;
    }

    public (int X, int Y) Coordinate { get; private set; }

    public CellType Type { get; set; }

    public static double DistanceBetween(Cell source, Cell destination)
    {
        double squareDistance = Math.Pow(destination.Coordinate.X - source.Coordinate.X, 2)
                                + Math.Pow(destination.Coordinate.Y - source.Coordinate.Y, 2);

        return Math.Sqrt(squareDistance);
    }
    
    public object Clone()
    {
        return new Cell(Type, Coordinate);
    }
}