using Godot;

namespace mazer;

public static class RectExtensions
{
    public static Vector2I RandPointIn(this Rect2I rect)
    {
        // Impl note: Only works if the size of the rect is non-negative
        return new Vector2I(
            GD.RandRange(rect.Position.X, rect.End.X - 1),
            GD.RandRange(rect.Position.Y, rect.End.Y - 1)
        );
    }
}
