using System.Collections.Generic;
using System.Text;

namespace mazer;

public static class WallExtensions
{
    private static readonly IReadOnlyDictionary<Wall, Wall> opposites = new Dictionary<Wall, Wall>
    {
        { Wall.Up, Wall.Down },
        { Wall.Down, Wall.Up },
        { Wall.Left, Wall.Right },
        { Wall.Right, Wall.Left },
        { Wall.None, Wall.All },
        { Wall.All, Wall.None },
    };

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
        return opposites[wall];
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
}
