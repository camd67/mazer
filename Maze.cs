using Godot;

namespace mazer;

public partial class Maze : TileMap
{
    [Signal]
    public delegate void MazeGeneratedEventHandler(Vector2I startingLocation, Vector2I exitLocation);

    // [Export] - Un-exported since this breaks when != -1
    private int deadEndsToTrim = -1;

    [Export]
    private Vector2I mapSize;

    [Export]
    private Vector2I maxRoomSize;

    [Export]
    private Vector2I minRoomSize;

    [Export]
    private int outerWallPaddingThickness;

    [Export]
    private int roomGenerationAttempts;

    private PatternManager patternManager;

    public override void _Ready()
    {
        patternManager = GD.Load<PackedScene>("res://pattern_management.tscn").Instantiate<PatternManager>();
        patternManager.Init();
    }

    public void GenerateMaze()
    {
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

        // Choose a random starting location, which needs to be on the perimeter of the maze
        var startingLocation = mazeGenerator.GenerateRandomRoomLocation();

        var exitLocation = mazeGenerator.GenerateRandomRoomLocation();

        EmitSignal(SignalName.MazeGenerated, startingLocation, exitLocation);
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

    public Vector2 LocationToGlobal(Vector2I location)
    {
        var topLeftOffsetVector = TileSet.TileSize * patternManager.patternSize;
        var halfOffsetVector = TileSet.TileSize * (patternManager.patternSize / 2);
        var global = location * topLeftOffsetVector + halfOffsetVector;
        // The center of the cell is always a safe play to place something
        return global;
    }
}
