namespace mazer;

public class Cell
{
    private bool up = true;
    private bool right = true;
    private bool down = true;
    private bool left = true;

    public Cell()
    {
        // Note that cells by default have all their walls up
    }

    public Cell(bool up, bool right, bool down, bool left)
    {
        this.up = up;
        this.right = right;
        this.down = down;
        this.left = left;
    }

    public void KnockDownTopWall()
    {
        up = false;
    }

    public void KnockDownRightWall()
    {
        right = false;
    }

    public void KnockDownBottomWall()
    {
        down = false;
    }

    public void KnockDownLeftWall()
    {
        left = false;
    }

    public bool CanMoveUp()
    {
        return up;
    }

    public bool CanMoveRight()
    {
        return right;
    }

    public bool CanMoveDown()
    {
        return down;
    }

    public bool CanMoveLeft()
    {
        return left;
    }
}
