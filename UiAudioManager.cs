using Godot;

namespace mazer;

/// <summary>
/// Handles global UI audio including focus, forward, and back sounds.
/// </summary>
public partial class UiAudioManager : Node
{
    private AudioStreamPlayer uiNext;
    private AudioStreamPlayer uiBack;
    private AudioStreamPlayer uiForward;

    public override void _Ready()
    {
        uiForward = GetNode<AudioStreamPlayer>("UiForward");
        uiBack = GetNode<AudioStreamPlayer>("UiBack");
        uiNext = GetNode<AudioStreamPlayer>("UiNext");
    }

    private void PlayPressed()
    {
        uiForward.Play();
    }

    private void PlayPressedBack()
    {
        uiBack.Play();
    }

    private void PlayFocusEntered()
    {
        uiNext.Play();
    }

    public void RegisterForward(BaseButton[] buttons)
    {
        foreach (var button in buttons)
        {
            RegisterForward(button);
        }
    }

    public void RegisterForward(BaseButton button)
    {
        button.Connect(Control.SignalName.FocusEntered, Callable.From(PlayFocusEntered));
        button.Connect(BaseButton.SignalName.Pressed, Callable.From(PlayPressed));
    }

    public void RegisterBack(BaseButton button)
    {
        button.Connect(Control.SignalName.FocusEntered, Callable.From(PlayFocusEntered));
        button.Connect(BaseButton.SignalName.Pressed, Callable.From(PlayPressedBack));
    }
}
