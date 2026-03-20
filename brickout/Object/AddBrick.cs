using Framework.Engine;

public class InvincibleBrick : Brick
{
    public InvincibleBrick(Scene scene, int x, int y) : base(scene, x, y) { }
    public override void Hit() { }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "□□", ConsoleColor.Gray);
    }
}

public class HardBrick : Brick
{
    private int _hp = 2;
    public HardBrick(Scene scene, int x, int y) : base(scene, x, y) { }
    public override void Hit()
    {
        _hp--;
        if (_hp <= 0) IsActive = false;
    }
    public override void Draw(ScreenBuffer buffer)
    {
        ConsoleColor color = _hp == 2 ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
        buffer.WriteText((int)X, (int)Y, "□□", color);
    }
}

public class BombBrick : Brick
{
    private List<Brick> _bricks;
    public BombBrick(Scene scene, int x, int y, List<Brick> bricks) : base(scene, x, y)
    {
        _bricks = bricks;
    }
    public override void Hit()
    {
        IsActive = false;
        foreach (var b in _bricks)
            if (Math.Abs(b.X - X) <= 4 && Math.Abs(b.Y - Y) <= 2)
                b.IsActive = false;
    }
    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "□□", ConsoleColor.Yellow);
    }
}