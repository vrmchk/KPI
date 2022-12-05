using Labyrinth.Enums;

namespace Labyrinth.Model;

internal class State : IEquatable<State>
{
    private List<State> _neighbors = null!;

    public Maze Maze { get; set; }
    public State? Parent { get; set; }
    public int Distance => (int)Math.Ceiling(Cell.DistanceBetween(Maze.Selected, Maze.Destination));
    public int Generation { get; set; }
    public int Evaluation { get; set; }

    public State(Maze maze, State? parent)
    {
        Maze = maze ?? throw new ArgumentNullException(nameof(maze));
        Parent = parent;
        Generation = 1 + parent?.Generation ?? 0;
        Evaluation = Distance + Generation;
    }

    public List<State> GetDepth()
    {
        var path = new List<State>();

        foreach (var node in GetNeighbors())
        {
            var tmp = node.GetDepth();
            if (tmp.Count > path.Count)
                path = tmp;
        }

        path.Insert(0, this);
        return path;
    }

    public List<State> GetNeighbors()
    {
        if (_neighbors != null)
            return _neighbors;
        _neighbors = new List<State>(3);
        foreach (var direction in Enum.GetValues<Direction>())
        {
            if (TryMove(direction, out var currentChild) && currentChild!.Equals(Parent) == false)
            {
                _neighbors.Add(currentChild);
            }
        }

        return _neighbors;
    }

    public List<State> GetPath()
    {
        var result = new List<State>();
        var curr = this;
        do
        {
            result.Add(curr);
            curr = curr.Parent;
        } while (curr != null);

        return result;
    }

    private bool TryMove(Direction dir, out State? newState)
    {
        (int horizontal, int vertical) = (0, 0);
        switch (dir)
        {
            case Direction.Up:
                vertical--;
                break;
            case Direction.Down:
                vertical++;
                break;
            case Direction.Left:
                horizontal--;
                break;
            case Direction.Right:
                horizontal++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
        }

        var selected = Maze.Selected;
        var below = Maze[selected.Coordinate.X + vertical, selected.Coordinate.Y + horizontal];

        newState = null;

        if (below == null)
            return false;
        if (below.Type is not CellType.Empty)
            return false;

        var moved = (Maze)Maze.Clone();
        moved.MoveSelection(dir);

        newState = new State(moved, this);
        return true;
    }

    public bool Equals(State? other)
    {
        return other is not null
               && Maze.Equals(other.Maze)
               && Parent == other.Parent;
    }

    public override string ToString()
    {
        return $"Generation: {Generation}, Distance: {Distance}, F: {Evaluation}";
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as State);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Maze, Parent, Distance, Generation, Evaluation);
    }
}