using Godot;

namespace mazer;

public partial class PathSfx : Node2D
{
    public Color color;

    public override void _Ready()
    {
        GetNode<GpuParticles2D>("InitialExplosion").Emitting = true;
        SetColor("InitialExplosion");
        SetColor("ContinualSparkles");
    }

    private void SetColor(string path)
    {
        ((ParticleProcessMaterial)GetNode<GpuParticles2D>(path).ProcessMaterial).Color = color;
    }
}
