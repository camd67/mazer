using System.Collections.Generic;
using Godot;
using Godot.Collections;

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
    [Export]
    public Vector2I patternSize;

    [Export]
    private int patternsPerRow;

    [Export]
    private int upperHorizConnectionSize = 2;

    [Export]
    private int bottomHorizConnectionSize = 1;

    [Export]
    private int leftVertConnectionSize = 1;

    [Export]
    private int rightVertConnectionSize = 1;

    // These should really be some kinda custom resource
    [ExportGroup("Atlases"), Export]
    private Array<Vector2I> wallAtlases;

    [Export]
    private Vector2I wallAtlas;

    [Export]
    private Vector2I floorAtlas;

    [Export]
    private Array<Vector2I> floorAtlases;

    [Export]
    private Vector2I shadowAtlas;

    [Export]
    private Array<Vector2I> shadowAtlases;

    private System.Collections.Generic.Dictionary<Wall, List<Vector2I>> atlasCellsByWall;
    private readonly Vector2I debugAtlas = new Vector2I(7, 9);

    public void Init()
    {
        atlasCellsByWall = new System.Collections.Generic.Dictionary<Wall, List<Vector2I>>();
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
                if (atlas == wallAtlas && RngUtil.OneIn(5))
                {
                    atlas = RngUtil.Pick(wallAtlases);
                }
                else if (atlas == floorAtlas && RngUtil.OneIn(2))
                {
                    atlas = RngUtil.Pick(floorAtlases);
                }
                else if (atlas == shadowAtlas && RngUtil.OneIn(2))
                {
                    atlas = RngUtil.Pick(shadowAtlases);
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

    public void DrawFloor(Rect2I rect, TileMap otherTileMap)
    {
        var end = rect.End;

        // Through each tile in our overall rect
        for (var tileX = rect.Position.X; tileX < end.X; tileX++)
        {
            for (var tileY = rect.Position.Y; tileY < end.Y; tileY++)
            {
                var removeTopLeft = true;
                var removeTopRight = true;
                var removeBottomRight = true;
                var removeBottomLeft = true;

                if (tileX == rect.Position.X)
                {
                    // We can't draw on the left side
                    removeBottomLeft = false;
                    removeTopLeft = false;
                }
                else if (tileX == end.X - 1)
                {
                    // We can't draw on the right side
                    removeBottomRight = false;
                    removeTopRight = false;
                }

                if (tileY == rect.Position.Y)
                {
                    // We can't remove the top
                    removeTopLeft = false;
                    removeTopRight = false;
                }
                else if (tileY == end.Y - 1)
                {
                    // We can't remove the bottom
                    removeBottomLeft = false;
                    removeBottomRight = false;
                }

                // Through each cell in each tile
                FillTileWithFloors(
                    tileX, tileY, otherTileMap, removeTopLeft, removeTopRight, removeBottomRight, removeBottomLeft
                );
            }
        }
    }

    private void FillTileWithFloors(
        int tileX,
        int tileY,
        TileMap otherTileMap,
        bool removeTopLeft,
        bool removeTopRight,
        bool removeBottomRight,
        bool removeBottomLeft)
    {
        for (var x = 0; x < patternSize.X; x++)
        {
            for (var y = 0; y < patternSize.Y; y++)
            {
                var topLeft = removeTopLeft && x < leftVertConnectionSize
                                             && y < upperHorizConnectionSize;
                var topRight = removeTopRight && x > patternSize.X - rightVertConnectionSize - 1
                                               && y < upperHorizConnectionSize;
                var bottomRight = removeBottomRight && x > patternSize.X - rightVertConnectionSize - 1
                                                     && y > patternSize.Y - bottomHorizConnectionSize - 1;
                var bottomLeft = removeBottomLeft && x < leftVertConnectionSize
                                                   && y > patternSize.Y - bottomHorizConnectionSize - 1;

                // We're not in a corner we want to remove, skip
                if (!topLeft && !topRight && !bottomRight && !bottomLeft)
                {
                    continue;
                }

                var atlas = floorAtlas;

                if (RngUtil.OneIn(2))
                {
                    atlas = RngUtil.Pick(floorAtlases);
                }

                otherTileMap.SetCell(
                    0,
                    new Vector2I(
                        tileX * patternSize.X + x,
                        tileY * patternSize.Y + y
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
                atlasList.Add(GetCellAtlasCoords(0, editorLocation));
            }
        }

        return atlasList;
    }
}
