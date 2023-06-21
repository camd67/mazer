using Godot;

namespace mazer;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void PlayerAbilityEventHandler(Vector2 location, int distance);

    [Export]
    private float speed = 300;

    [Export]
    private int abilityDistance;

    private AnimationPlayer animationPlayer;
    private PlayerState playerState;
    private Timer abilityCooldown;
    private TextureProgressBar abilityIcon;
    private Texture2D abilityOverTexture;

    public override void _Ready()
    {
        abilityIcon = GetNode<TextureProgressBar>("AbilityUi/AbilityIcon");
        // store and clear our over texture. We'll replace it later
        abilityOverTexture = abilityIcon.TextureOver;
        abilityIcon.TextureOver = null;

        abilityCooldown = GetNode<Timer>("AbilityCooldown");
        abilityCooldown.Timeout += () =>
        {
            abilityIcon.TextureOver = abilityOverTexture;
        };
        playerState = GetNode<PlayerState>("/root/PlayerState");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.AssignedAnimation = "bounce_move";
        var sprite = GetNode<Sprite2D>("Sprite");
        sprite.RegionRect = new Rect2(playerState.playerAtlas, sprite.RegionRect.Size);
    }

    public override void _Process(double delta)
    {
        ManageAnimationPlayback();
        abilityIcon.Value = 100 * (1 - abilityCooldown.TimeLeft / abilityCooldown.WaitTime);
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

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ability"))
        {
            if (abilityCooldown.TimeLeft <= 0)
            {
                EmitSignal(SignalName.PlayerAbility, GlobalPosition, abilityDistance);
                abilityCooldown.Start();
                abilityIcon.TextureOver = null;
            }
        }
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
