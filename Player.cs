using Godot;

namespace mazer;

public partial class Player : CharacterBody2D
{
    [Export]
    private float speed = 300;

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
}
