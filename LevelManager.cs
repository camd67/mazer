using System;
using System.Linq;
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
    private Control pauseScreen;
    private PlayerState playerState;
    private Player player;
    private Area2D exit;

    public override void _Ready()
    {
        playerState = GetNode<PlayerState>("/root/PlayerState");
        playerState.mode = PlayerState.PlayerMode.Playing;

        pauseScreen = GetNode<Control>("GlobalUi/PauseScreen");
        pauseScreen.Connect("pause_menu_closed", Callable.From(HandlePauseMenuClosed));
        pauseScreen.Visible = false;

        screenDarkener = GetNode<CanvasModulate>("ScreenDarkener");
        screenDarkener.Visible = true;

        timerLabel = GetNode<TimerLabel>("GlobalUi/TimerLabel");

        levelComplete = GetNode<Control>("GlobalUi/LevelComplete");
        levelComplete.Connect("level_restart", Callable.From(HandleLevelRestart));
        levelComplete.Visible = false;

        maze = GetNode<Maze>("Maze");
        maze.MazeGenerated += HandleMazeGenerated;
        maze.GenerateMaze();
    }

    private void HandlePauseMenuClosed()
    {
        playerState.mode = PlayerState.PlayerMode.Playing;
        pauseScreen.Visible = false;
    }

    private void HandleLevelRestart()
    {
        maze.GenerateMaze();
    }

    private void HandleMazeGenerated(Vector2I startingLocation, Vector2I exitLocation)
    {
        player = playerScene.Instantiate<Player>();
        AddChild(player);
        player.GlobalPosition = maze.LocationToGlobal(startingLocation);
        player.PlayerAbility += HandlePlayerAbility;

        exit = exitScene.Instantiate<Area2D>();
        AddChild(exit);
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

    private void HandlePlayerAbility(Vector2 location, int distance)
    {
        var playerLocationIndex = maze.LocationToIndex(location);
        var exitLocationIndex = maze.LocationToIndex(exit.GlobalPosition);
        var path = maze.ComputePath(playerLocationIndex, exitLocationIndex);
        maze.DrawDirectedPath(path.Take(distance).ToArray());
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        // Only pause when the level isn't complete
        if (!levelComplete.Visible && @event.IsActionPressed("pause"))
        {
            if (playerState.mode == PlayerState.PlayerMode.Playing)
            {
                playerState.mode = PlayerState.PlayerMode.Paused;
                pauseScreen.Visible = true;
            }
            else
            {
                HandlePauseMenuClosed();
            }
        }
    }
}
