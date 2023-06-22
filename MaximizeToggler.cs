using Godot;

namespace mazer;

public partial class MaximizeToggler : Node
{
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_maximize"))
        {
            if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Maximized)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);
            }
        }
    }
}
