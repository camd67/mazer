using Godot;

namespace mazer;

public partial class LevelManager : Node2D
{
    [Signal]
    public delegate void PlayLevelEventHandler();

    [Signal]
    public delegate void StartPlayerSelectionEventHandler();

    [Export]
    private PackedScene playerScene;

    [Export]
    private PackedScene exitScene;

    private Maze maze;
    private TimerLabel timerLabel;

    public override void _Ready()
    {
        timerLabel = GetNode<TimerLabel>("GlobalUi/TimerLabel");

        maze = GetNode<Maze>("Maze");
        maze.MazeGenerated += HandleMazeGenerated;

        maze.GenerateMaze();
    }

    private void HandleMazeGenerated(Vector2I startingLocation, Vector2I exitLocation)
    {
        var player = playerScene.Instantiate<Player>();
        AddChild(player);
        player.GlobalPosition = maze.LocationToGlobal(startingLocation);

        var exit = exitScene.Instantiate<Area2D>();
        AddChild(exit);
        // TODO: What if this is the player position?
        exit.GlobalPosition = maze.LocationToGlobal(exitLocation);
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
