using System.Diagnostics;
using Godot;

namespace mazer;

public partial class TimerLabel : Label
{
    private Stopwatch timer = new();

    public override void _Process(double delta)
    {
        Text = timer.Elapsed.ToString("mm\\:ss\\:fff");
    }

    public void Start()
    {
        timer = Stopwatch.StartNew();
    }

    public void Stop()
    {
        timer.Stop();
    }
}
