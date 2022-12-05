using System.Text;
using Labyrinth.Common;
using Labyrinth.Enums;
using Newtonsoft.Json;

namespace Labyrinth.Model;

internal class Maze : ICloneable
{
    private (int x, int y) _selectedCoord;
    private (int x, int y) _sourceCoord;
    private (int x, int y) _destinationCoord;

    public Cell[,] Cells { get; init; }

    public Cell? this[int x, int y]
    {
        get => Enumerable.Range(0, Cells.GetLength(0)).Contains(x)
               && Enumerable.Range(0, Cells.GetLength(1)).Contains(y)
            ? Cells[x, y]
            : null;

        set => Cells[x, y] = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Cell Selected
    {
        get => this[_selectedCoord.x, _selectedCoord.y] ??
               throw new ArgumentOutOfRangeException(nameof(_selectedCoord));
        set => this[_selectedCoord.x, _selectedCoord.y] = value;
    }

    public Cell Source
    {
        get => this[_sourceCoord.x, _sourceCoord.y] ?? throw new ArgumentOutOfRangeException(nameof(_sourceCoord));
        set => this[_sourceCoord.x, _sourceCoord.y] = value;
    }

    public Cell Destination
    {
        get => this[_destinationCoord.x, _destinationCoord.y] ??
               throw new ArgumentOutOfRangeException(nameof(_destinationCoord));
        set => this[_destinationCoord.x, _destinationCoord.y] = value;
    }

    public Maze(int height, int width)
    {
        Cells = new Cell[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Cells[i, j] = new Cell(CellType.Empty, (i, j));
            }
        }
    }

    public Maze(Cell[,] cells)
    {
        Cells = cells ?? throw new ArgumentNullException(nameof(cells));
        var list = cells.ToList();

        if (list.Count(cell => cell.Type == CellType.Source) != 1)
            throw new ArgumentException("Maze matrix have to include single entrance", nameof(cells));

        if (list.Count(cell => cell.Type == CellType.Destination) != 1)
            throw new ArgumentException("Maze matrix have to include single exit", nameof(cells));

        if (list.Count(cell => cell.Type == CellType.Selected) > 1)
            throw new ArgumentException("Maze matrix have to include single selected cell", nameof(cells));

        foreach (Cell cell in list)
        {
            if (cell.Type == CellType.Source)
            {
                _sourceCoord = cell.Coordinate;
                if (list.Any(c => c.Type == CellType.Selected) == false)
                    _selectedCoord = cell.Coordinate;
            }

            if (cell.Type == CellType.Destination)
                _destinationCoord = cell.Coordinate;

            if (cell.Type == CellType.Selected)
                _selectedCoord = cell.Coordinate;
        }
    }

    public void MoveSelection(Direction direction)
    {
        int horizontalOffset = 0, verticalOffset = 0;
        switch (direction)
        {
            case Direction.Up:
                verticalOffset = -1;
                break;
            case Direction.Down:
                verticalOffset = 1;
                break;
            case Direction.Left:
                horizontalOffset = -1;
                break;
            case Direction.Right:
                horizontalOffset = 1;
                break;
        }

        (int x, int y) currCoord = (_selectedCoord.x + verticalOffset, _selectedCoord.y + horizontalOffset);
        int width = Cells.GetLength(0);
        int height = Cells.GetLength(1);

        if (currCoord.x < 0 || currCoord.x >= width)
            return;
        if (currCoord.y < 0 || currCoord.y >= height)
            return;

        if (Cells[currCoord.x, currCoord.y].Type is not CellType.Empty)
            return;

        if (Selected.Type != CellType.Source)
        {
            Selected.Type = CellType.Visited;
        }

        _selectedCoord = currCoord;
        Selected.Type = CellType.Selected;
    }

    public static Maze LoadFromFile(string path)
    {
        var deserialized = JsonConvert.DeserializeObject<CompressedCell[,]>(File.ReadAllText(path))!;

        int rows = deserialized.GetLength(0);
        int columns = deserialized.GetLength(1);
        var maze = new Maze(rows, columns);
        for (int rowIndex = 0; rowIndex < rows; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                var cell = deserialized[rowIndex, columnIndex].ToCell();
                if (cell.Type is CellType.Source)
                {
                    maze._sourceCoord = cell.Coordinate;
                    maze._selectedCoord = cell.Coordinate;
                }
                else if (cell.Type is CellType.Destination)
                {
                    maze._destinationCoord = cell.Coordinate;
                }

                maze[rowIndex, columnIndex] = cell;
            }
        }

        return maze;
    }

    public List<Cell?> Neighbors((int x, int y) coordinate)
    {
        var top = this[coordinate.x + 1, coordinate.y];
        var right = this[coordinate.x, coordinate.y + 1];
        var bottom = this[coordinate.x - 1, coordinate.y];
        var left = this[coordinate.x, coordinate.y + 1];
        return new List<Cell?> { top, left, right, bottom };
    }

    public object Clone()
    {
        int rows = Cells.GetLength(0);
        int cols = Cells.GetLength(1);
        var cells = new Cell[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                cells[i, j] = (Cell)Cells[i, j].Clone();
        }

        return new Maze(cells);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int row = 0; row < Cells.GetLength(0); row++)
        {
            sb.Append('\n');
            for (int column = 0; column < Cells.GetLength(1); column++)
            {
                string value = Cells[row, column].Type switch
                {
                    CellType.Empty => " ",
                    CellType.Source => "s",
                    CellType.Destination => "d",
                    CellType.Visited => "*",
                    CellType.Selected => "$",
                    CellType.Wall => "%",
                    _ => ""
                };
                sb.Append(value + " ");
            }
        }

        return sb.ToString();
    }
}