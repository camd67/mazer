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

    [Export]
    private Color screenDarkColor = new(0, 0, 0);

    [Export]
    private Color screenLighterColor = new(.5f, .5f, .5f);

    private Maze maze;
    private TimerLabel timerLabel;
    private Control levelComplete;
    private CanvasModulate screenDarkener;

    public override void _Ready()
    {
        screenDarkener = GetNode<CanvasModulate>("ScreenDarkener");
        timerLabel = GetNode<TimerLabel>("GlobalUi/TimerLabel");
        levelComplete = GetNode<Control>("GlobalUi/LevelComplete");
        levelComplete.Connect("level_restart", Callable.From(HandleLevelRestart));

        maze = GetNode<Maze>("Maze");
        maze.MazeGenerated += HandleMazeGenerated;

        maze.GenerateMaze();
    }

    private void HandleLevelRestart()
    {
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
                levelComplete.Visible = true;
                levelComplete.Call("update_results_time", timerLabel.CurrentMillis());
                timerLabel.Visible = false;
                screenDarkener.Color = screenLighterColor;
                exit.QueueFree();
                player.QueueFree();
            }
        };

        screenDarkener.Color = screenDarkColor;
        timerLabel.Visible = true;
        levelComplete.Visible = false;
        timerLabel.Start();
    }
}
