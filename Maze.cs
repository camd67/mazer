using Godot;

namespace mazer;

public partial class Maze : TileMap
{
    [Signal]
    public delegate void MazeGeneratedEventHandler(Vector2I startingLocation, Vector2I exitLocation);

    // [Export] - Un-exported since this breaks when != -1
    private int deadEndsToTrim = -1;

    [Export]
    public Vector2I mapSize;

    [Export]
    private Vector2I maxRoomSize;

    [Export]
    private Vector2I minRoomSize;

    [Export]
    private int outerWallPaddingThickness;

    [Export]
    private int roomGenerationAttempts;

    [Export]
    public float minPlayerSpawnDistance;

    private PatternManager patternManager;
    private AStar2D pathfinder;

    public override void _Ready()
    {
        patternManager = GD.Load<PackedScene>("res://pattern_management.tscn").Instantiate<PatternManager>();
        patternManager.Init();
    }

    public void GenerateMaze()
    {
        Clear();
        var mazeGenerator = new MazeGenerator(
            mapSize,
            deadEndsToTrim,
            roomGenerationAttempts,
            maxRoomSize,
            minRoomSize
        );
        var maze = mazeGenerator.GenerateMaze();

        var width = maze.GetLength(0);
        var height = maze.GetLength(1);

        DrawMaze(maze);
        DrawOuterPaddingWalls(width, height);
        CleanupRooms(mazeGenerator);

        var exitLocation = mazeGenerator.GenerateRandomRoomLocation();

        var startingLocation = mazeGenerator.GenerateRandomRoomLocation();

        var generationAttempts = 10_000;
        while (LocationToGlobal(exitLocation).DistanceTo(LocationToGlobal(startingLocation)) <
               minPlayerSpawnDistance
               && generationAttempts > 0)
        {
            startingLocation = mazeGenerator.GenerateRandomRoomLocation();
            generationAttempts--;
        }

        if (generationAttempts <= 0)
        {
            GD.PushWarning("Player location generation failed due to too many attempts. Using last tried location. " +
                           "This may result in players too close to the exit.");
        }

        InitPathfinder(maze);

        EmitSignal(SignalName.MazeGenerated, startingLocation, exitLocation);
    }

    private void InitPathfinder(Wall[,] walls)
    {
        pathfinder = new AStar2D();
        // Can't reserve since we don't actually know the number of open spaces and we'd be reserving too much
        // pathfinder.ReserveSpace(mapSize.X * mapSize.Y);
        for (var x = 0; x < mapSize.X; x++)
        {
            for (var y = 0; y < mapSize.Y; y++)
            {
                if (walls[x, y].HasOpening())
                {
                    pathfinder.AddPoint(PointToPathId(new Vector2I(x,y)), new Vector2(x,y));
                }
            }
        }
        for (var x = 0; x < mapSize.X; x++)
        {
            for (var y = 0; y < mapSize.Y; y++)
            {
                var wall = walls[x, y];
                var thisPoint = new Vector2I(x, y);
                var thisId = PointToPathId(thisPoint);

                if (!wall.Has(Wall.Up))
                {
                    var otherPoint = thisPoint + Vector2I.Up;
                    var otherId = PointToPathId(otherPoint);
                    pathfinder.ConnectPoints(thisId, otherId);
                }
                if (!wall.Has(Wall.Right))
                {
                    var otherPoint = thisPoint + Vector2I.Right;
                    var otherId = PointToPathId(otherPoint);
                    pathfinder.ConnectPoints(thisId, otherId);
                }
                if (!wall.Has(Wall.Down))
                {
                    var otherPoint = thisPoint + Vector2I.Down;
                    var otherId = PointToPathId(otherPoint);
                    pathfinder.ConnectPoints(thisId, otherId);
                }
                if (!wall.Has(Wall.Left))
                {
                    var otherPoint = thisPoint + Vector2I.Left;
                    var otherId = PointToPathId(otherPoint);
                    pathfinder.ConnectPoints(thisId, otherId);
                }
            }
        }
    }

    private void CleanupRooms(MazeGenerator mazeGenerator)
    {
        foreach (var room in mazeGenerator.rooms)
        {
            patternManager.DrawFloor(room, this);
        }
    }

    private void DrawOuterPaddingWalls(int width, int height)
    {
        // Draw the top and bottom padding (excluding the corner bits)
        for (var x = 0; x < width; x++)
        {
            for (var y = -outerWallPaddingThickness; y < 0; y++)
            {
                patternManager.DrawCell(Wall.All, new Vector2I(x, y), this);
            }

            for (var y = height; y < height + outerWallPaddingThickness; y++)
            {
                patternManager.DrawCell(Wall.All, new Vector2I(x, y), this);
            }
        }

        // Draw the left and right padding (including the extra corner bits)
        for (var y = -outerWallPaddingThickness; y < height + outerWallPaddingThickness; y++)
        {
            for (var x = -outerWallPaddingThickness; x < 0; x++)
            {
                patternManager.DrawCell(Wall.All, new Vector2I(x, y), this);
            }

            for (var x = width; x < width + outerWallPaddingThickness; x++)
            {
                patternManager.DrawCell(Wall.All, new Vector2I(x, y), this);
            }
        }
    }

    private void DrawMaze(Wall[,] maze)
    {
        var width = maze.GetLength(0);
        var height = maze.GetLength(1);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var wall = maze[x, y];
                var pos = new Vector2I(x, y);
                patternManager.DrawCell(wall, pos, this);
            }
        }
    }

    /// <summary>
    ///     Safely gets the given coords from the maze.
    ///     If the coords are not in bounds then Wall.All is returned.
    /// </summary>
    private Wall SafeGetWall(int x, int y, Wall[,] maze)
    {
        return maze.IsInBounds(x, y) ? maze[x, y] : Wall.All;
    }

    /// <summary>
    /// Draws a directed path on the maze on the second layer
    /// </summary>
    public void DrawDirectedPath(Vector2[] points)
    {
        var lastDirection = Vector2I.Zero;
        for (var i = 0; i < points.Length; i++)
        {
            // Convert from our vector2 into vector2i for easier manipulation
            var point = VectorUtil.ToI(points[i]);

            // Figure out the direction we're moving in (if we're not on the last point)
            // that last point will just continue the last direction
            var pathSides = lastDirection == Vector2I.Zero ? Wall.None : WallExtensions.DirectionToWall[lastDirection].Invert();
            if (i < points.Length - 1)
            {
                lastDirection = VectorUtil.ToI(points[i + 1]) - point;
            }
            else
            {
                lastDirection = Vector2I.Zero;
            }
            pathSides = pathSides.With(lastDirection == Vector2I.Zero ? Wall.None : WallExtensions.DirectionToWall[lastDirection]);

            patternManager.DrawPath(point, pathSides, this);
        }
    }

    public Vector2 LocationToGlobal(Vector2I location)
    {
        var topLeftOffsetVector = TileSet.TileSize * patternManager.patternSize;
        var halfOffsetVector = TileSet.TileSize * (patternManager.patternSize / 2);
        var global = location * topLeftOffsetVector + halfOffsetVector;
        // The center of the cell is always a safe play to place something
        return global;
    }

    public Vector2I LocationToIndex(Vector2 location)
    {
        var blockSize = TileSet.TileSize * patternManager.patternSize;
        return VectorUtil.ToI(location / blockSize);
    }

    private long PointToPathId(Vector2I pos)
    {
        return pos.X + pos.Y * mapSize.X;
    }

    public Vector2[] ComputePath(Vector2I from, Vector2I to)
    {
        return pathfinder.GetPointPath(PointToPathId(from), PointToPathId(to));
    }
}
