using Godot;

namespace mazer;

/// <summary>
/// Autoload node storing player information
/// </summary>
public partial class PlayerState : Node
{
    public enum PlayerMode
    {
        Paused = 0,
        Playing = 1,
    }

    public Vector2I playerAtlas;

    public PlayerMode mode = PlayerMode.Playing;
    public Color color;
    public int characterIndex;

    public override void _Process(double delta)
    {
        // GD.Print(playerAtlas);
    }
}
