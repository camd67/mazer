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
        GetNode<Camera2D>("Camera2D").GlobalPosition = maze.LocationToGlobal(
            new Vector2I(
                maze.mapSize.X / 2,
                maze.mapSize.Y / 2
            )
        );
    }

    private void HandleMazeGenerated(Vector2I startingLocation, Vector2I exitLocation)
    {
        GetNode<Sprite2D>("Player").GlobalPosition = maze.LocationToGlobal(startingLocation);
        GetNode<Sprite2D>("Exit").GlobalPosition = maze.LocationToGlobal(exitLocation);
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawArc(GetNode<Sprite2D>("Player").GlobalPosition, maze.minPlayerSpawnDistance, 0, 360, 50, Colors.Red);
        DrawArc(GetNode<Sprite2D>("Exit").GlobalPosition, maze.minPlayerSpawnDistance, 0, 360, 50, Colors.Red);
    }
}
