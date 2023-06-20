using Godot;

namespace mazer;

public partial class Player : CharacterBody2D
{
    [Export]
    private float speed = 300;

    private AnimationPlayer animationPlayer;
    private PlayerState playerState;

    public override void _Ready()
    {
        playerState = GetNode<PlayerState>("/root/PlayerState");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.AssignedAnimation = "bounce_move";
        var sprite = GetNode<Sprite2D>("Sprite");
        sprite.RegionRect = new Rect2(playerState.playerAtlas, sprite.RegionRect.Size);
    }

    public override void _Process(double delta)
    {
        ManageAnimationPlayback();
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        var direction = Input.GetVector("player_left", "player_right", "player_up", "player_down");

        if (playerState.mode == PlayerState.PlayerMode.Paused)
        {
            // If we're "paused" just prevent like no buttons were pressed
            direction = Vector2.Zero;
        }

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
