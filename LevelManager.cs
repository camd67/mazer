using Godot;

namespace mazer;

public partial class LevelManager : Node2D
{
    [Export]
    private PackedScene playerScene;

    [Export]
    private PackedScene exitScene;

    private Map map;

    public override void _Ready()
    {
        map = GetNode<Map>("Map");
        map.MazeGenerated += (startingLocation, exitLocation) =>
        {
            var player = playerScene.Instantiate<Player>();
            AddChild(player);
            player.GlobalPosition = map.LocationToGlobal(startingLocation);

            var exit = exitScene.Instantiate<Area2D>();
            AddChild(exit);
            // TODO: What if this is the player position?
            exit.GlobalPosition = map.LocationToGlobal(exitLocation);
            exit.BodyEntered += body =>
            {
                GD.Print(body.Name);
                if (body == player)
                {
                    GD.Print("You won!");
                    exit.QueueFree();
                    player.QueueFree();
                }
            };
        };

        map.GenerateMaze();
    }
}
