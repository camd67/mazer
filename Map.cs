using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using Godot;

namespace mazer;

public partial class Map : TileMap
{
    [Export]
    private Vector2I mapSize;

    [Export]
    private Vector2I patternSize;

    private IReadOnlyDictionary<Wall, TileMapPattern> wallToAtlas;

    public override void _Ready()
    {
        InitWallToAtlas();
        var mazeGenerator = new MazeGenerator(mapSize);
        var maze = mazeGenerator.GenerateMaze();

        var width = maze.GetLength(0);
        var height = maze.GetLength(1);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                SetPattern(
                    0,
                    new Vector2I(x, y) * patternSize,
                    wallToAtlas[maze[x,y]]
                );
            }
        }
        GD.Print(maze.AsString());
    }

    private void InitWallToAtlas()
    {
        wallToAtlas = new Dictionary<Wall, TileMapPattern>
        {
            { Wall.None, TileSet.GetPattern(0) },
            { Wall.Up, TileSet.GetPattern(1) },
            { Wall.Right, TileSet.GetPattern(2) },
            { Wall.Up | Wall.Right, TileSet.GetPattern(3) },
            { Wall.Down, TileSet.GetPattern(4) },
            { Wall.Up | Wall.Down, TileSet.GetPattern(5) },
            { Wall.Right | Wall.Down, TileSet.GetPattern(6) },
            { Wall.Up | Wall.Down | Wall.Right, TileSet.GetPattern(7) },
            { Wall.Left, TileSet.GetPattern(8) },
            { Wall.Left | Wall.Up, TileSet.GetPattern(9) },
            { Wall.Left | Wall.Right, TileSet.GetPattern(10) },
            { Wall.Left | Wall.Right | Wall.Up, TileSet.GetPattern(11) },
            { Wall.Left | Wall.Down, TileSet.GetPattern(12) },
            { Wall.Up | Wall.Left | Wall.Down, TileSet.GetPattern(13) },
            { Wall.Left | Wall.Down | Wall.Right, TileSet.GetPattern(14) },
            { Wall.All, TileSet.GetPattern(15) },
        };
    }
}
