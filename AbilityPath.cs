using Godot;

namespace mazer;

/// <summary>
///     Manages spawning multiple ability paths
/// </summary>
public partial class AbilityPath : Node2D
{
    [Export]
    private PackedScene abilitySfxScene;

    private int distance;
    private Vector2[] abilityPath;
    private Maze maze;
    private Timer pathSpawnTimer;
    private int abilitySfxIndex;
    private Color color;

    public void Init(Maze maze, int distance, Vector2[] abilityPath, Color color)
    {
        this.maze = maze;
        this.distance = distance;
        this.abilityPath = abilityPath;
        this.color = color;
    }

    public override void _Ready()
    {
        pathSpawnTimer = GetNode<Timer>("PathSpawnTimer");
        pathSpawnTimer.Timeout += HandleAbilityTimer;
        pathSpawnTimer.Start();
    }

    private void HandleAbilityTimer()
    {
        if (abilityPath == null || abilityPath.IsEmpty() || abilitySfxIndex > distance)
        {
            pathSpawnTimer.Stop();
            return;
        }

        var drawnLocations = maze.DrawDirectedPathAtIndex(abilityPath, abilitySfxIndex, color);
        var globalCellCenters = maze.ConvertCellCoordsToGlobalCenter(drawnLocations);

        foreach (var globalCoord in globalCellCenters)
        {
            var abilitySfx = abilitySfxScene.Instantiate<PathSfx>();
            abilitySfx.GlobalPosition = globalCoord;
            abilitySfx.color = color;
            AddChild(abilitySfx);
        }
        abilitySfxIndex++;
    }
}
