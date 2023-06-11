using Godot;

namespace mazer;

public partial class MazeViewer : Node2D
{
    private Maze maze;

    public override void _Ready()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);

        maze = GetNode<Maze>("Maze");
        maze.MazeGenerated += HandleMazeGenerated;

        maze.GenerateMaze();
    }

    private void HandleMazeGenerated(Vector2I startingLocation, Vector2I exitLocation)
    {

        GetNode<Sprite2D>("Player").GlobalPosition = maze.LocationToGlobal(startingLocation);
        GetNode<Sprite2D>("Exit").GlobalPosition = maze.LocationToGlobal(exitLocation);
    }
}
