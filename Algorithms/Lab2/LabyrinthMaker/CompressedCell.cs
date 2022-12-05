using Labyrinth.Enums;

namespace LabyrinthMaker;

internal struct CompressedCell
{
    public CompressedCell((int, int) coordinate, CellType type)
    {
        Coordinate = coordinate;
        Type = type;
    }

    public (int X, int Y) Coordinate { get; set; }

    public CellType Type { get; set; }
}
