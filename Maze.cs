using System.Collections.Generic;
using Godot;

namespace mazer;

public partial class Maze : TileMap
{
    [Signal]
    public delegate void MazeGeneratedEventHandler(Vector2I startingLocation, Vector2I exitLocation);

    private IReadOnlyDictionary<Wall, TileMapPattern> entranceAtlas;

    [Export]
    private int outerWallPaddingThickness;

    [Export]
    private Vector2I mapSize;

    [Export]
    private int deadEndsToTrim;

    [Export]
    private Vector2I patternSize;

    [Export]
    private int roomGenerationAttempts;

    [Export]
    private Vector2I maxRoomSize;

    [Export]
    private Vector2I minRoomSize;

    private IReadOnlyDictionary<Wall, TileMapPattern> wallToAtlas;

    public override void _Ready()
    {
        InitAtlasMaps();
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
        // GD.Print(maze.AsString());

        // Choose a random starting location, which needs to be on the perimeter of the maze
        var startingLocation = MazeGenerator.GenerateRandomEdgeCell(mapSize);
        AddEntrance(startingLocation, maze);

        var exitLocation = mazeGenerator.GenerateExitLocation();

        EmitSignal(SignalName.MazeGenerated, startingLocation, exitLocation);
    }

    private void DrawOuterPaddingWalls(int width, int height)
    {
        // Draw the top and bottom padding (excluding the corner bits)
        for (var x = 0; x < width; x++)
        {
            for (var y = -outerWallPaddingThickness; y < 0; y++)
            {
                SetPattern(
                    0,
                    new Vector2I(x, y) * patternSize,
                    wallToAtlas[Wall.All]
                );
            }
            for (var y = height; y < height + outerWallPaddingThickness; y++)
            {
                SetPattern(
                    0,
                    new Vector2I(x, y) * patternSize,
                    wallToAtlas[Wall.All]
                );
            }
        }

        // Draw the left and right padding (including the extra corner bits)
        for (var y = -outerWallPaddingThickness; y < height + outerWallPaddingThickness; y++)
        {
            for (var x = -outerWallPaddingThickness; x < 0; x++)
            {
                SetPattern(
                    0,
                    new Vector2I(x, y) * patternSize,
                    wallToAtlas[Wall.All]
                );
            }
            for (var x = width; x < width + outerWallPaddingThickness; x++)
            {
                SetPattern(
                    0,
                    new Vector2I(x, y) * patternSize,
                    wallToAtlas[Wall.All]
                );
            }
        }
    }

    private void AddEntrance(Vector2I startingLocation, Wall[,] maze)
    {
        var wallAtEntrance = maze[startingLocation.X, startingLocation.Y];
        var mapEdge = Wall.None;

        if (startingLocation.X == 0)
        {
            mapEdge = Wall.Left;
        }
        else if (startingLocation.X == mapSize.X - 1)
        {
            mapEdge = Wall.Right;
        }
        else if (startingLocation.Y == 0)
        {
            mapEdge = Wall.Up;
        }
        else if (startingLocation.Y == mapSize.Y - 1)
        {
            mapEdge = Wall.Down;
        }

        // Selectively replace the walls of the maze with a door pattern
        // We want to be careful about replacing walls so they flow correctly
        // TODO: Place patterns correctly
    }

    private void DrawMaze(Wall[,] maze)
    {
        var width = maze.GetLength(0);
        var height = maze.GetLength(1);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                SetPattern(
                    0,
                    new Vector2I(x, y) * patternSize,
                    wallToAtlas[maze[x, y]]
                );
            }
        }
    }

    private void InitAtlasMaps()
    {
        var patternIndex = 0;
        wallToAtlas = new Dictionary<Wall, TileMapPattern>
        {
            { Wall.None, TileSet.GetPattern(patternIndex++) },
            { Wall.Up, TileSet.GetPattern(patternIndex++) },
            { Wall.Right, TileSet.GetPattern(patternIndex++) },
            { Wall.Up | Wall.Right, TileSet.GetPattern(patternIndex++) },
            { Wall.Down, TileSet.GetPattern(patternIndex++) },
            { Wall.Up | Wall.Down, TileSet.GetPattern(patternIndex++) },
            { Wall.Right | Wall.Down, TileSet.GetPattern(patternIndex++) },
            { Wall.Up | Wall.Down | Wall.Right, TileSet.GetPattern(patternIndex++) },
            { Wall.Left, TileSet.GetPattern(patternIndex++) },
            { Wall.Left | Wall.Up, TileSet.GetPattern(patternIndex++) },
            { Wall.Left | Wall.Right, TileSet.GetPattern(patternIndex++) },
            { Wall.Left | Wall.Right | Wall.Up, TileSet.GetPattern(patternIndex++) },
            { Wall.Left | Wall.Down, TileSet.GetPattern(patternIndex++) },
            { Wall.Up | Wall.Left | Wall.Down, TileSet.GetPattern(patternIndex++) },
            { Wall.Left | Wall.Down | Wall.Right, TileSet.GetPattern(patternIndex++) },
            { Wall.All, TileSet.GetPattern(patternIndex++) },
        };

        entranceAtlas = new Dictionary<Wall, TileMapPattern>
        {
            { Wall.Up, TileSet.GetPattern(patternIndex++) },
            { Wall.Left, TileSet.GetPattern(patternIndex++) },
            { Wall.Right, TileSet.GetPattern(patternIndex++) },
            { Wall.Down, TileSet.GetPattern(patternIndex++) },
            { Wall.Down | Wall.Right | Wall.Left, TileSet.GetPattern(patternIndex++) },
            { Wall.Down | Wall.Left, TileSet.GetPattern(patternIndex++) },
            { Wall.Down | Wall.Right, TileSet.GetPattern(patternIndex++) },
            { Wall.Down | Wall.Right | Wall.Up, TileSet.GetPattern(patternIndex++) },
        };
    }

    public Vector2 LocationToGlobal(Vector2I location)
    {
        var topLeftOffsetVector = TileSet.TileSize * patternSize;
        var halfOffsetVector = TileSet.TileSize * (patternSize / 2);
        var global = location * topLeftOffsetVector + halfOffsetVector;
        // The center of the cell is always a safe play to place something
        return global;
    }
}
