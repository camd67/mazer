using Godot;

namespace mazer;

public partial class Player : CharacterBody2D
{
    [Export]
    private float speed = 300;

    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.AssignedAnimation = "bounce_move";
    }

    public override void _Process(double delta)
    {
        ManageAnimationPlayback();
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        var direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        if (direction != Vector2.Zero)
        {
            velocity = speed * direction;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
            velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void ManageAnimationPlayback()
    {
        if (Velocity.IsZeroApprox())
        {
            if (animationPlayer.IsPlaying())
            {
                animationPlayer.Stop();
            }
        }
        else if (!animationPlayer.IsPlaying())
        {
            animationPlayer.Play();
        }
    }
}
