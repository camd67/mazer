using System;

namespace mazer;

[Flags]
public enum Wall
{
    /*
        None - 0
        Up - 1
        Right - 2
        Up, Right - 3
        Down - 4
        Up, Down - 5
        Right, Down - 6
        Up, Right, Down - 7
        Left - 8
        Up, Left - 9
        Right, Left - 10
        Up, Right, Left - 11
        Down, Left - 12
        Up, Down, Left - 13
        Right, Down, Left - 14
        All - 15
     */
    None = 0b0000,

    Up = 0b0001,
    Right = 0b0010,
    Down = 0b0100,
    Left = 0b1000,

    All = 0b1111,
}
