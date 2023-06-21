using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace mazer;

/// <summary>
///     Generates a maze.
///     This class stores state, meaning you should probably create a new instance if you
///     want to re-generate or otherwise create a new maze.
/// </summary>
public class MazeGenerator
{
    /// <summary>
    ///     The bounds of the maze.
    ///     We should not generate any tiles beyond these edges.
    /// </summary>
    private readonly Vector2I bounds;

    /// <summary>
    ///     The number of dead ends to trim off the maze.
    ///     If this number is too high eventually the whole maze will
    ///     be consumed since technically everything is a "dead end"
    ///     eventually.
    ///
    ///     Pass in -1 to consume all possible dead ends
    /// </summary>
    private readonly int deadEndsToTrim;

    /// <summary>
    ///     The max size of rooms we'll generate.
    /// </summary>
    private readonly Vector2I maxRoomSize;

    /// <summary>
    ///     The minimum size of rooms we'll generate.
    /// </summary>
    private readonly Vector2I minRoomSize;

    private readonly Dictionary<Vector2I, int> regionsByPos = new();

    /// <summary>
    ///     The number of attempts to generate rooms.
    ///     This doesn't mean that if we have 200 attempts we'll get
    ///     200 rooms. Instead we'll try 200 times and if we get any
    ///     overlaps that'll skip that "attempt".
    /// </summary>
    private readonly int roomGenerationAttempts;

    private int currentRegion;

    private Wall[,] maze;
    private HashSet<Vector2I> visitedSpaces;

    public readonly List<Rect2I> rooms = new();

    public MazeGenerator(
        Vector2I bounds,
        int deadEndsToTrim,
        int roomGenerationAttempts,
        Vector2I maxRoomSize,
        Vector2I minRoomSize)
    {
        this.bounds = bounds;
        this.deadEndsToTrim = deadEndsToTrim == -1 ? int.MaxValue : deadEndsToTrim;
        this.maxRoomSize = maxRoomSize;
        this.minRoomSize = minRoomSize;
        this.roomGenerationAttempts = roomGenerationAttempts;
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0
               && x < bounds.X
               && y >= 0
               && y < bounds.Y;
    }

    private bool IsInBounds(Vector2I pos)
    {
        return pos.X >= 0
               && pos.X < bounds.X
               && pos.Y >= 0
               && pos.Y < bounds.Y;
    }

    /// <summary>
    ///     Generates a maze.
    ///     To control generation change the constructor parameters.
    ///     Access the generated maze via the maze field.
    /// </summary>
    public Wall[,] GenerateMaze()
    {
        maze = InitMaze();

        visitedSpaces = new HashSet<Vector2I>();

        // Inspired by http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
        // 1. Carve rooms based on the roomGenerationAttempts and room size params
        CarveRooms();

        // 2. Iterate through every open area and fill in with corridors
        for (var x = 0; x < bounds.X; x++)
        {
            for (var y = 0; y < bounds.Y; y++)
            {
                if (maze[x, y] == Wall.All)
                {
                    // We found a place where we have full walls, process it
                    var startingLocation = new Vector2I(x, y);
                    var cellsToProcess = new Stack<Vector2I>();

                    visitedSpaces.Add(startingLocation);
                    cellsToProcess.Push(startingLocation);
                    regionsByPos[startingLocation] = currentRegion;

                    CarveCorridors(cellsToProcess);
                    currentRegion++;
                }
            }
        }

        // 3. Join together caves and corridors until everything is in a single continuous loop
        JoinRegions();

        // 4. Trim off dead ends
        TrimDeadEnds();

        return maze;
    }

    private void JoinRegions()
    {
        // Collect a dict of all joints to the regions they join together
        var wallsForJoint = new Dictionary<Vector2I, List<Wall>>();
        var jointsByRegion = new Dictionary<int, List<Vector2I>>();
        var regionsByJoint = new Dictionary<Vector2I, HashSet<int>>();

        for (var x = 0; x < bounds.X; x++)
        {
            for (var y = 0; y < bounds.Y; y++)
            {
                var cell = maze[x, y];
                var location = new Vector2I(x, y);
                var joinedWalls = new List<Wall>();
                var thisRegion = regionsByPos[location];
                var otherRegions = new HashSet<int>();

                foreach (var dir in WallExtensions.CardinalDirs)
                {
                    if (cell.Has(dir))
                    {
                        var otherCell = location + WallExtensions.WallToDirection[dir];

                        if (IsInBounds(otherCell) && regionsByPos[otherCell] != thisRegion)
                        {
                            joinedWalls.Add(dir);
                            otherRegions.Add(regionsByPos[otherCell]);
                        }
                    }
                }

                // We're in a cell that has only one region.
                // That means it's not a joint!
                if (joinedWalls.Count == 0)
                {
                    continue;
                }

                wallsForJoint[location] = joinedWalls;

                if (!jointsByRegion.ContainsKey(regionsByPos[location]))
                {
                    jointsByRegion[regionsByPos[location]] = new List<Vector2I>();
                }

                jointsByRegion[regionsByPos[location]].Add(location);
                regionsByJoint[location] = otherRegions;
            }
        }

        while (jointsByRegion.Count > 0)
        {
            var workingRegion = RngUtil.Pick(jointsByRegion.Keys.ToList());
            var joint = RngUtil.Pick(jointsByRegion[workingRegion]);

            // Knock down a wall to another region that has a valid joint
            var wallsForThisJoint = wallsForJoint[joint].ToList();
            var wall = RngUtil.Pick(wallsForThisJoint);
            var otherSide = WallExtensions.WallToDirection[wall] + joint;
            var otherRegion = regionsByPos[otherSide];
            wallsForThisJoint.Remove(wall);

            // Keep looping through our walls until we encounter a wall that opens into a region we haven't processed yet
            while (!jointsByRegion.ContainsKey(otherRegion))
            {
                if (wallsForJoint.Count == 0)
                {
                    throw new Exception("Map gen resulted in broken down wall with no valid region on other side");
                }

                wall = RngUtil.Pick(wallsForThisJoint);
                wallsForThisJoint.Remove(wall);
                otherSide = WallExtensions.WallToDirection[wall] + joint;
                otherRegion = regionsByPos[otherSide];
            }

            maze[joint.X, joint.Y] = maze[joint.X, joint.Y].Without(wall);
            maze[otherSide.X, otherSide.Y] = maze[otherSide.X, otherSide.Y].Without(wall.Invert());

            // Cleanup any remaining joints in this region
            RemoveJointsFromRegion(workingRegion, otherRegion, jointsByRegion, regionsByJoint);
            RemoveJointsFromRegion(otherRegion, workingRegion, jointsByRegion, regionsByJoint);
        }
    }

    private void RemoveJointsFromRegion(
        int regionToClean,
        int regionToCleanFrom,
        Dictionary<int, List<Vector2I>> jointsByRegion,
        Dictionary<Vector2I, HashSet<int>> regionsByJoint)
    {
        var remainingJointsForThisRegion = new List<Vector2I>();

        foreach (var jointToClean in jointsByRegion[regionToCleanFrom])
        {
            var regionsForJoint = regionsByJoint[jointToClean];
            regionsForJoint.Remove(regionToClean);

            if (regionsForJoint.Count == 0)
            {
                // This joint is no longer a valid joint since it only connected to an already merged joint
                regionsByJoint.Remove(jointToClean);
            }
            else
            {
                remainingJointsForThisRegion.Add(jointToClean);
            }
        }

        // Replace any remaining joints
        if (remainingJointsForThisRegion.Count > 0)
        {
            jointsByRegion[regionToCleanFrom] = remainingJointsForThisRegion;
        }
        else
        {
            jointsByRegion.Remove(regionToCleanFrom);
        }
    }

    private void CarveRooms()
    {
        for (var numAttempts = 0; numAttempts < roomGenerationAttempts; numAttempts++)
        {
            var roomSize = new Vector2I(
                GD.RandRange(minRoomSize.X, maxRoomSize.X),
                GD.RandRange(minRoomSize.Y, maxRoomSize.Y)
            );

            // Make sure to remove room size before generating the location since
            // there's no point in generating a location guaranteed to be out of bounds.
            var roomLocation = VectorUtil.RandomInBounds(bounds - roomSize);

            // Make sure we have space for this room
            if (ValidateRoomFits(roomLocation, roomSize))
            {
                rooms.Add(new Rect2I(roomLocation, roomSize));

                // Add room
                for (var xSize = 0; xSize < roomSize.X; xSize++)
                {
                    for (var ySize = 0; ySize < roomSize.Y; ySize++)
                    {
                        var x = xSize + roomLocation.X;
                        var y = ySize + roomLocation.Y;

                        visitedSpaces.Add(new Vector2I(x, y));
                        regionsByPos[new Vector2I(x, y)] = currentRegion;

                        // Figure out which kind of wall we need to add in
                        // First start with no walls
                        maze[x, y] = Wall.None;

                        // Then add in walls conditionally based on what edge we're on (if any)
                        if (x == roomLocation.X)
                        {
                            maze[x, y] = maze[x, y].With(Wall.Left);
                        }
                        else if (x == roomLocation.X + roomSize.X - 1)
                        {
                            maze[x, y] = maze[x, y].With(Wall.Right);
                        }

                        if (y == roomLocation.Y)
                        {
                            maze[x, y] = maze[x, y].With(Wall.Up);
                        }
                        else if (y == roomLocation.Y + roomSize.Y - 1)
                        {
                            maze[x, y] = maze[x, y].With(Wall.Down);
                        }
                    }
                }

                currentRegion++;
            }
        }
    }

    /// <summary>
    ///     Validates that a given room can fit in our maze, returning true if so
    /// </summary>
    private bool ValidateRoomFits(Vector2I roomLocation, Vector2I roomSize)
    {
        for (var xSize = 0; xSize < roomSize.X; xSize++)
        {
            for (var ySize = 0; ySize < roomSize.Y; ySize++)
            {
                var x = xSize + roomLocation.X;
                var y = ySize + roomLocation.Y;

                if (!IsInBounds(x, y) || maze[x, y] != Wall.All)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void CarveCorridors(Stack<Vector2I> cellsToProcess)
    {
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
                var chosenWall = WallExtensions.DirectionToWall[chosenDirection];
                // Clear the wall in our current direction and the next cell in the opposite direction
                maze[currentCell.X, currentCell.Y] = maze[currentCell.X, currentCell.Y].Without(chosenWall);
                maze[nextCell.X, nextCell.Y] = maze[nextCell.X, nextCell.Y].Without(chosenWall.Invert());

                regionsByPos[nextCell] = currentRegion;
                visitedSpaces.Add(nextCell);
                cellsToProcess.Push(nextCell);
            }
        }
    }

    private List<Vector2I> CollectDeadEnds()
    {
        var knownDeadEnds = new List<Vector2I>();
        for (var x = 0; x < bounds.X; x++)
        {
            for (var y = 0; y < bounds.Y; y++)
            {
                if (maze[x, y].IsDeadEnd())
                {
                    knownDeadEnds.Add(new Vector2I(x, y));
                }
            }
        }

        return knownDeadEnds;
    }

    private void TrimDeadEnds()
    {
        var knownDeadEnds = CollectDeadEnds();
        // Make sure this is a queue not a stack/list so that we don't re-process the same dead end over and over
        // instead we'll spread the trimming out across the whole maze
        var deadEndQueue = new Queue<Vector2I>(knownDeadEnds);
        var currentDeadEndsTrimmed = 0;

        while (deadEndQueue.Count > 0 && currentDeadEndsTrimmed < deadEndsToTrim)
        {
            // Get a dead end
            var deadEnd = deadEndQueue.Dequeue();

            // Trim it
            // Note that simply marking this cell as all walls and removing the walls of the cell
            // next to it is okay since by definition there's only one cell connected to this one.
            var oldWall = maze[deadEnd.X, deadEnd.Y];
            var oldOpening = oldWall.Invert();
            maze[deadEnd.X, deadEnd.Y] = Wall.All;
            Vector2I nextCell;

            // Repair the next cell
            // This logic goes:
            // If the old opening was Down then the dead end was "facing" up
            // therefore, if we had an up opening we now need a down wall
            /*
             * ----------
             * |  x   y
             * ----------
             * has a right opening. If we close off x we need to fill in the left wall of y
             *      -----
             *      |  y
             *      -----
             */
            switch (oldOpening)
            {
                case Wall.Up:
                    nextCell = new Vector2I(deadEnd.X, deadEnd.Y - 1);
                    maze[nextCell.X, nextCell.Y] = maze[nextCell.X, nextCell.Y].With(Wall.Down);
                    break;
                case Wall.Right:
                    nextCell = new Vector2I(deadEnd.X + 1, deadEnd.Y);
                    maze[nextCell.X, nextCell.Y] = maze[nextCell.X, nextCell.Y].With(Wall.Left);
                    break;
                case Wall.Down:
                    nextCell = new Vector2I(deadEnd.X, deadEnd.Y + 1);
                    maze[nextCell.X, nextCell.Y] = maze[nextCell.X, nextCell.Y].With(Wall.Up);
                    break;
                case Wall.Left:
                    nextCell = new Vector2I(deadEnd.X - 1, deadEnd.Y);
                    maze[nextCell.X, nextCell.Y] = maze[nextCell.X, nextCell.Y].With(Wall.Right);
                    break;
                case Wall.None:
                case Wall.All:
                    // Fix up our uninitialized state by re-checking our current cell.
                    // This should hopefully turn into a no-op
                    nextCell = deadEnd;
                    // No walls and all walls... shouldn't have happened.
                    // but if they do we can just ignore it
                    GD.PushWarning(
                        "Ran into a none or all wall processing dead end " + deadEnd + ". This may be an issue."
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // If, after trimming we still have a dead end, add it onto the trim queue!
            if (maze[nextCell.X, nextCell.Y].IsDeadEnd())
            {
                deadEndQueue.Enqueue(nextCell);
            }

            currentDeadEndsTrimmed++;
        }
    }

    private Wall[,] InitMaze()
    {
        var newMaze = new Wall[bounds.X, bounds.Y];

        for (var x = 0; x < bounds.X; x++)
        {
            for (var y = 0; y < bounds.Y; y++)
            {
                newMaze[x, y] = Wall.All;
            }
        }

        return newMaze;
    }

    public Vector2I GenerateRandomRoomLocation()
    {
        return RngUtil.Pick(rooms).RandPointIn();
    }
}
