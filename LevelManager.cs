using Godot;

namespace mazer;

public partial class LevelManager : Node2D
{
    [Export]
    private PackedScene playerScene;

    [Export]
    private PackedScene exitScene;

    private Map map;
    private TimerLabel timerLabel;

    public override void _Ready()
    {
        timerLabel = GetNode<TimerLabel>("GlobalUi/TimerLabel");

        map = GetNode<Map>("Map");
        map.MazeGenerated += HandleMazeGenerated;

        map.GenerateMaze();
    }

    private void HandleMazeGenerated(Vector2I startingLocation, Vector2I exitLocation)
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
            if (body == player)
            {
                timerLabel.Stop();
                GD.Print("You won!");
                exit.QueueFree();
                player.QueueFree();
            }
        };

        timerLabel.Start();
    }
}
