using System.Diagnostics;
using Godot;

namespace mazer;

public partial class TimerLabel : Label
{
    private Stopwatch timer = new();

    public override void _Process(double delta)
    {
        Text = Time.GetTimeStringFromUnixTime(timer.ElapsedMilliseconds);
    }

    public void Start()
    {
        timer = Stopwatch.StartNew();
    }

    public void Stop()
    {
        timer.Stop();
    }

    public long CurrentMillis()
    {
        return timer.ElapsedMilliseconds;
    }
}
