using Godot;

namespace mazer;

public static class VectorUtil
{
    /// <summary>
    /// Returns a new random Vector2I within bounds exclusive.
    /// </summary>
    public static Vector2I RandomInBounds(Vector2I bounds)
    {
        return new Vector2I(
            GD.RandRange(0, bounds.X - 1),
            GD.RandRange(0, bounds.Y - 1)
        );
    }

    /// <summary>
    /// Returns a new random Vector2I within bounds inclusive.
    /// </summary>
    public static Vector2I RandomInBoundsInclusive(Vector2I bounds)
    {
        return new Vector2I(
            GD.RandRange(0, bounds.X),
            GD.RandRange(0, bounds.Y)
        );
    }
}
