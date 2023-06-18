using Godot;

namespace mazer;

/// <summary>
/// Autoload node storing player information
/// </summary>
public partial class PlayerState : Node
{
    public Vector2I playerAtlas;

    public override void _Ready()
    {
        GD.Print("PlayerState init");
    }

    public override void _Process(double delta)
    {
        // GD.Print(playerAtlas);
    }
}
