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
}
