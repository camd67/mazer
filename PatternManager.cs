using System.Collections.Generic;
using Godot;

namespace mazer;

/// <summary>
///     Helps manage "patterns" for our tilemap.
///     Originally this was using actual tilemap patterns but that sorta
///     didn't work since calling AddPattern caused an editor crash.
///     That'll probably get fixed but until then we'll do manual parsing
///     and creation of "patterns".
/// </summary>
public partial class PatternManager : TileMap
{
    private Dictionary<Wall, List<Vector2I>> atlasCellsByWall;

    [Export]
    public Vector2I patternSize;

    [Export]
    private int patternsPerRow;

    [Export]
    private Godot.Collections.Array<Vector2I> wallAtlases;

    public void Init()
    {
        atlasCellsByWall = new Dictionary<Wall, List<Vector2I>>();
        ParsePatterns();
    }

    /// <summary>
    ///     Draws the given wall at the given location in the provided tilemap.
    /// </summary>
    public void DrawCell(Wall wall, Vector2I pos, TileMap otherTileMap)
    {
        var atlasList = atlasCellsByWall[wall];

        for (var x = 0; x < patternSize.X; x++)
        {
            for (var y = 0; y < patternSize.Y; y++)
            {
                var index = x * patternSize.Y + y;
                var atlas = atlasList[index];
                // Little flair to make walls a bit more interesting.
                // If we ever draw a blank wall, replace it with a random
                // assortment of other walls
                if (atlas == Vector2I.Zero && RngUtil.OneIn(5))
                {
                    atlas = RngUtil.Pick(wallAtlases);
                }
                otherTileMap.SetCell(
                    0,
                    new Vector2I(
                        pos.X * patternSize.X + x,
                        pos.Y * patternSize.Y + y
                    ),
                    1,
                    atlas
                );
            }
        }
    }

    private void ParsePatterns()
    {
        for (var w = 0; w <= (int)Wall.All; w++)
        {
            // Compute our x,y based on the number of patterns per row
            var x = w % patternsPerRow;
            var y = w / patternsPerRow;
            // GD.Print("Creating atlas map for " + (Wall)w + " at index " + x + "," + y);
            atlasCellsByWall[(Wall)w] = CreatePatternAt(x, y);
        }
    }

    private List<Vector2I> CreatePatternAt(int xIndex, int yIndex)
    {
        var atlasList = new List<Vector2I>(patternSize.X * patternSize.Y);
        // Make sure to add a little offset nudge based on the index
        // Each row/column has 1 padding row
        var patternSizeWithOffset = patternSize + new Vector2I(1, 1);

        for (var x = 0; x < patternSize.X; x++)
        {
            for (var y = 0; y < patternSize.Y; y++)
            {
                var editorLocation = new Vector2I(
                    xIndex * patternSizeWithOffset.X + x,
                    yIndex * patternSizeWithOffset.Y + y
                );
                // GD.Print("Getting pattern data starting at " + editorLocation);
                atlasList.Add(GetCellAtlasCoords(0, editorLocation));
            }
        }

        return atlasList;
    }
}
