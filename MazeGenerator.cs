using System.Collections.Generic;
using Godot;

namespace mazer;

public class MazeGenerator
{
    public static Vector2I GenerateRandomEdgeCell(Vector2I bounds)
    {
        var edge = GD.Randi() % 4;
        var location = new Vector2I();

        switch (edge)
        {
            // Top or bottom
            case 0 or 2:
                location.X = GD.RandRange(0, bounds.X - 1);
                break;
            // Right or left
            case 1 or 3:
                location.Y = GD.RandRange(0, bounds.Y - 1);
                break;
        }

        return location;
    }

    private static readonly IReadOnlyDictionary<Vector2I, Wall> DirectionToWallCache = new Dictionary<Vector2I, Wall>
    {
        { Vector2I.Up, Wall.Up },
        { Vector2I.Right, Wall.Right },
        { Vector2I.Down, Wall.Down },
        { Vector2I.Left, Wall.Left },
    };

    private readonly Vector2I bounds;
    private readonly List<Vector2I> knownDeadEnds = new();

    public MazeGenerator(Vector2I bounds)
    {
        this.bounds = bounds;
    }

    private bool IsInBounds(Vector2I pos)
    {
        return pos.X >= 0 && pos.X < bounds.X && pos.Y >= 0 && pos.Y < bounds.Y;
    }

    public Wall[,] GenerateMaze()
    {
        var maze = new Wall[bounds.X, bounds.Y];

        for (var x = 0; x < bounds.X; x++)
        {
            for (var y = 0; y < bounds.Y; y++)
            {
                maze[x, y] = Wall.All;
            }
        }

        var visitedSpaces = new HashSet<Vector2I>();

        // Choose a starting point somewhere on the edge of our maze
        var startingLocation = GenerateRandomEdgeCell(bounds);

        var cellsToProcess = new Stack<Vector2I>();

        visitedSpaces.Add(startingLocation);
        cellsToProcess.Push(startingLocation);

        while (cellsToProcess.Count > 0)
        {
            var currentCell = cellsToProcess.Pop();
            // Check to see if any neighbors need to be processed
            var candidateDirections = new List<Vector2I>(4);

            var up = currentCell + Vector2I.Up;
            if (IsInBounds(up) && !visitedSpaces.Contains(up))
            {
                candidateDirections.Add(Vector2I.Up);
            }

            var right = currentCell + Vector2I.Right;
            if (IsInBounds(right) && !visitedSpaces.Contains(right))
            {
                candidateDirections.Add(Vector2I.Right);
            }

            var down = currentCell + Vector2I.Down;
            if (IsInBounds(down) && !visitedSpaces.Contains(down))
            {
                candidateDirections.Add(Vector2I.Down);
            }

            var left = currentCell + Vector2I.Left;
            if (IsInBounds(left) && !visitedSpaces.Contains(left))
            {
                candidateDirections.Add(Vector2I.Left);
            }

            if (candidateDirections.Count > 0)
            {
                // We have unvisited neighbors!

                // Process our cell again
                cellsToProcess.Push(currentCell);
                // Choose a direction to move in
                var chosenDirection = candidateDirections[GD.RandRange(0, candidateDirections.Count - 1)];
                var nextCell = currentCell + chosenDirection;
                // Remove the wall in this current cell, and the next cell
                var chosenWall = DirectionToWallCache[chosenDirection];
                // Clear the wall in our current direction and the next cell in the opposite direction
                maze[currentCell.X, currentCell.Y] = maze[currentCell.X, currentCell.Y].Without(chosenWall);
                maze[nextCell.X, nextCell.Y] = maze[nextCell.X, nextCell.Y].Without(chosenWall.Invert());

                if (maze[currentCell.X, currentCell.Y].IsDeadEnd())
                {
                    knownDeadEnds.Add(currentCell);
                }

                visitedSpaces.Add(nextCell);
                cellsToProcess.Push(nextCell);
            }
        }

        return maze;
    }

    public Vector2I GenerateExitLocation()
    {
        // Choose a dead end at random
        return knownDeadEnds[GD.RandRange(0, knownDeadEnds.Count - 1)];
    }
}
