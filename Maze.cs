using Godot;

namespace mazer;

public partial class Maze : TileMap
{
    [Signal]
    public delegate void MazeGeneratedEventHandler(Vector2I startingLocation, Vector2I exitLocation);

    [Export]
    private int outerWallPaddingThickness;

    [Export]
    private Vector2I mapSize;

    [Export]
    private int deadEndsToTrim;

    [Export]
    private int roomGenerationAttempts;

    [Export]
    private Vector2I maxRoomSize;

    [Export]
    private Vector2I minRoomSize;

    private PatternManager patternManager;

    public override void _Ready()
    {
        patternManager = GD.Load<PackedScene>("res://pattern_management.tscn").Instantiate<PatternManager>();
        patternManager.Init();
    }

    public void GenerateMaze()
    {
        var mazeGenerator = new MazeGenerator(
            bounds: mapSize,
            deadEndsToTrim: deadEndsToTrim,
            roomGenerationAttempts: roomGenerationAttempts,
            maxRoomSize: maxRoomSize,
            minRoomSize: minRoomSize
        );
        var maze = mazeGenerator.GenerateMaze();

        var width = maze.GetLength(0);
        var height = maze.GetLength(1);

        DrawMaze(maze);
        DrawOuterPaddingWalls(width, height);

        // Choose a random starting location, which needs to be on the perimeter of the maze
        var startingLocation = mazeGenerator.GenerateRandomRoomLocation();

        var exitLocation = mazeGenerator.GenerateRandomRoomLocation();

        EmitSignal(SignalName.MazeGenerated, startingLocation, exitLocation);
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
                patternManager.DrawCell(maze[x, y], new Vector2I(x, y), this);

                if (x >= 1 && y >= 1)
                {
                    // return;
                }
            }
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
}
