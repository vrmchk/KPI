using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LabyrinthMaker;

internal class MazeGenerator
{
    private readonly Random _rnd = new();
    private bool[,] _cells;
    private Action<bool[,]> _renderHandler;

    public MazeGenerator(Action<bool[,]> renderHandler)
    {
        _renderHandler = renderHandler;
    }

    public bool[,] GenerateMaze(int size)
    {
        if (size % 2 == 0)
            throw new ArgumentException("Size has to be odd number for proper bounds generation");
        _cells = new bool[size, size];
        CompressedCell posRnd = new CompressedCell {
            Coordinate = (1, 1)
        };
        SetCell(posRnd, true);

        HashSet<CompressedCell> candidateCells = new HashSet<CompressedCell>();
        candidateCells.UnionWith(GetCandidateCellsFor(posRnd, false));
        while (candidateCells.Count > 0)
        {
            CompressedCell thisCell = candidateCells.ElementAt(_rnd.Next(0, candidateCells.Count));

            IList<CompressedCell> pathCandidates = GetCandidateCellsFor(thisCell, true);

            if (pathCandidates.Count > 0)
            {
                ConnectCell(pathCandidates[_rnd.Next(0, pathCandidates.Count)], thisCell);
            }

            candidateCells.UnionWith(GetCandidateCellsFor(thisCell, false));

            candidateCells.Remove(thisCell);

        }
        _renderHandler?.Invoke(_cells);

        return _cells;
    }

    private void SetCell(CompressedCell posRnd, bool isPath)
    {
        _cells[posRnd.Coordinate.X, posRnd.Coordinate.Y] = isPath;
    }

    private void ConnectCell(CompressedCell cellA, CompressedCell cellB)
    {
        int x = (cellA.Coordinate.X + cellB.Coordinate.X) / 2;
        int y = (cellA.Coordinate.Y + cellB.Coordinate.Y) / 2;
        _cells[cellB.Coordinate.X, cellB.Coordinate.Y] = true;
        _cells[x, y] = true;
    }

    private bool CellHasValidPosition(CompressedCell position)
    {
        return
            position.Coordinate.X >= 0 &&
            position.Coordinate.Y >= 0 &&
            position.Coordinate.X < _cells.GetLength(0) &&
            position.Coordinate.Y < _cells.GetLength(1);
    }

    private IList<CompressedCell> GetCandidateCellsFor(CompressedCell position, bool getPathCells)
    {
        List<CompressedCell> candidatePathCells = new List<CompressedCell>();
        List<CompressedCell> candidateWallCells = new List<CompressedCell>();

        CompressedCell northCandidate = new CompressedCell { Coordinate = (position.Coordinate.X, position.Coordinate.Y - 2) };
        CompressedCell eastCandidate = new CompressedCell { Coordinate = (position.Coordinate.X + 2, position.Coordinate.Y) };
        CompressedCell southCandidate = new CompressedCell { Coordinate = (position.Coordinate.X, position.Coordinate.Y + 2) };
        CompressedCell westCandidate = new CompressedCell { Coordinate = (position.Coordinate.X - 2, position.Coordinate.Y) };

        if (CellHasValidPosition(northCandidate))
        {
            if (_cells[northCandidate.Coordinate.X, northCandidate.Coordinate.Y])
            {
                candidatePathCells.Add(northCandidate);
            }
            else
            {
                candidateWallCells.Add(northCandidate);
            }
        }
        if (CellHasValidPosition(eastCandidate))
        {
            if (_cells[eastCandidate.Coordinate.X, eastCandidate.Coordinate.Y])
            {
                candidatePathCells.Add(eastCandidate);
            }
            else
            {
                candidateWallCells.Add(eastCandidate);
            }
        }
        if (CellHasValidPosition(southCandidate))
        {
            if (_cells[southCandidate.Coordinate.X, southCandidate.Coordinate.Y])
            {
                candidatePathCells.Add(southCandidate);
            }
            else
            {
                candidateWallCells.Add(southCandidate);
            }
        }
        if (CellHasValidPosition(westCandidate))
        {
            if (_cells[westCandidate.Coordinate.X, westCandidate.Coordinate.Y])
            {
                candidatePathCells.Add(westCandidate);
            }
            else
            {
                candidateWallCells.Add(westCandidate);
            }
        }

        if (getPathCells) { return candidatePathCells; }
        else { return candidateWallCells; }
    }
}
