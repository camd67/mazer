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

    public void Init(Maze maze, int distance, Vector2[] abilityPath)
    {
        this.maze = maze;
        this.distance = distance;
        this.abilityPath = abilityPath;
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

        var drawnLocations = maze.DrawDirectedPathAtIndex(abilityPath, abilitySfxIndex);
        var globalCellCenters = maze.ConvertCellCoordsToGlobalCenter(drawnLocations);

        foreach (var globalCoord in globalCellCenters)
        {
            var abilitySfx = abilitySfxScene.Instantiate<Node2D>();
            abilitySfx.GlobalPosition = globalCoord;
            abilitySfx.GetNode<GpuParticles2D>("InitialExplosion").Emitting = true;
            AddChild(abilitySfx);
        }
        abilitySfxIndex++;
    }
}
