using System.Collections.Generic;
using Godot;

namespace mazer;

public static class RngUtil
{
    /// <summary>
    /// Picks a random element from the given collection of values
    /// </summary>
    public static T Pick<T>(IList<T> values)
    {
        return values[GD.RandRange(0, values.Count - 1)];
    }

    /// <summary>
    /// Returns true approximately 1 in count times
    /// </summary>
    public static bool OneIn(int count)
    {
        var chance = 1.0 / count;
        return GD.Randf() < chance;
    }
}
