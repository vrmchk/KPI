using Labyrinth.Enums;

namespace Labyrinth.Utils;
internal struct CompressedCell
{
    public CompressedCell((int, int) coordinate, CellType type)
    {
        Coordinate = coordinate;
        Type = type;
    }

    public (int, int) Coordinate { get; set; }

    public CellType Type { get; set; }
}
