using System.Collections.Generic;
using System.Text;
using Godot;

namespace mazer;

public static class WallExtensions
{
    private static readonly IReadOnlyDictionary<Wall, Wall> Opposites = new Dictionary<Wall, Wall>
    {
        // Basic inversions (only these contain all / none
        { Wall.Up, Wall.Down },
        { Wall.Down, Wall.Up },
        { Wall.Left, Wall.Right },
        { Wall.Right, Wall.Left },
        { Wall.None, Wall.All },
        { Wall.All, Wall.None },

        // Pair inversions
        { Wall.Up | Wall.Down, Wall.Left | Wall.Right },
        { Wall.Up | Wall.Left, Wall.Down | Wall.Right },
        { Wall.Up | Wall.Right, Wall.Left | Wall.Down },
        { Wall.Down | Wall.Right, Wall.Left | Wall.Up },
        { Wall.Down | Wall.Left, Wall.Right | Wall.Up },
        { Wall.Left | Wall.Right, Wall.Up | Wall.Down },

        // Triplet inversions (dead ends)
        { Wall.Up | Wall.Left | Wall.Down, Wall.Right },
        { Wall.Up | Wall.Right | Wall.Down, Wall.Left },
        { Wall.Up | Wall.Left | Wall.Right, Wall.Down },
        { Wall.Right | Wall.Left | Wall.Down, Wall.Up },
    };

    public static readonly IReadOnlyList<Wall> CardinalDirs = new[]
    {
        Wall.Up,
        Wall.Right,
        Wall.Down,
        Wall.Left,
    };

    private static readonly IReadOnlyDictionary<Wall, string> WallAscii = new Dictionary<Wall, string>
    {
        { Wall.Right | Wall.Left | Wall.Down, "╹" },
        { Wall.Up | Wall.Left | Wall.Right, "╻" },
        { Wall.Up | Wall.Right | Wall.Down, "╸" },
        { Wall.Up | Wall.Left | Wall.Down, "╺" },
        { Wall.None, "╬" },
        { Wall.All, "█" },
        { Wall.Left | Wall.Right, "║" },
        { Wall.Down | Wall.Right, "╝" },
        { Wall.Down | Wall.Left, "╚" },
        { Wall.Up | Wall.Left, "╔" },
        { Wall.Up | Wall.Right, "╗" },
        { Wall.Up | Wall.Down, "═" },
        { Wall.Right, "╣" },
        { Wall.Left, "╠" },
        { Wall.Down, "╩" },
        { Wall.Up, "╦" },
    };

    public static readonly IReadOnlyDictionary<Vector2I, Wall> DirectionToWall = new Dictionary<Vector2I, Wall>
    {
        { Vector2I.Up, Wall.Up },
        { Vector2I.Right, Wall.Right },
        { Vector2I.Down, Wall.Down },
        { Vector2I.Left, Wall.Left },
    };

    public static readonly IReadOnlyDictionary<Wall, Vector2I> WallToDirection = new Dictionary<Wall, Vector2I>
    {
        { Wall.Up, Vector2I.Up },
        { Wall.Right, Vector2I.Right },
        { Wall.Down, Vector2I.Down },
        { Wall.Left, Vector2I.Left },
    };

    public static bool HasOpening(this Wall wall)
    {
        return !wall.Has(Wall.Up)
               || !wall.Has(Wall.Down)
               || !wall.Has(Wall.Right)
               || !wall.Has(Wall.Left);
    }

    public static bool Has(this Wall wall, Wall mask)
    {
        return (wall & mask) == mask;
    }

    public static Wall With(this Wall wall, Wall mask)
    {
        return wall | mask;
    }

    public static Wall Without(this Wall wall, Wall mask)
    {
        return wall & ~mask;
    }

    public static Wall Invert(this Wall wall)
    {
        return Opposites[wall];
    }

    public static bool IsDeadEnd(this Wall wall)
    {
        // Probably a better way, but just check all the combinations of dead ends
        return
            wall is (Wall.All & ~Wall.Up)
                or (Wall.All & ~Wall.Down)
                or (Wall.All & ~Wall.Left)
                or (Wall.All & ~Wall.Right)
            ;
    }

    public static string AsString(this Wall[,] maze)
    {
        var output = new StringBuilder();

        for (var y = 0; y < maze.GetLength(1); y++)
        {
            for (var x = 0; x < maze.GetLength(0); x++)
            {
                var wall = maze[x, y];
                output.Append('(');
                output.Append(wall.HasFlag(Wall.Up) ? 'U' : ' ');
                output.Append(wall.HasFlag(Wall.Right) ? 'R' : ' ');
                output.Append(wall.HasFlag(Wall.Down) ? 'D' : ' ');
                output.Append(wall.HasFlag(Wall.Left) ? 'L' : ' ');
                output.Append(')');
                output.Append(", ");
            }

            output.Append('\n');
        }

        return output.ToString();
    }

    public static string AsAscii(this Wall[,] maze)
    {
        var output = new StringBuilder();

        for (var y = 0; y < maze.GetLength(1); y++)
        {
            for (var x = 0; x < maze.GetLength(0); x++)
            {
                var wall = maze[x, y];

                output.Append(WallAscii[wall]);
            }

            output.Append('\n');
        }

        return output.ToString();
    }

    public static bool IsInBounds(this Wall[,] maze, int x, int y)
    {
        return x > 0
               && x < maze.GetLength(0)
               && y > 0
               && y < maze.GetLength(1);
    }
}
