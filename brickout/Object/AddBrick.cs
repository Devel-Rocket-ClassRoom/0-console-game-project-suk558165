using Framework.Engine;

public class InvincibleBrick : Brick
{
    public InvincibleBrick(Scene scene, int x, int y) : base(scene, x, y) { }
    public override void Hit() { }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Gray);
    }
}

public class HardBrick : Brick
{
    private int _hp = 2;
    public HardBrick(Scene scene, int x, int y) : base(scene, x, y) { }
    public override void Hit()
    {
        _hp--;
        if (_hp <= 0)
        {
            IsActive = false;
            OnHit?.Invoke(); // 완전히 깨질 때만 호출
        }
    }
    public override void Draw(ScreenBuffer buffer)
    {
        ConsoleColor color = _hp == 2 ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
        buffer.WriteText((int)X, (int)Y, "■■", color);
    }
}


    public class BombBrick : Brick
    {
        private List<Brick> _bricks;
        private float _explodeTimer = 0f;
        private bool _exploding = false;

        public BombBrick(Scene scene, int x, int y, List<Brick> bricks) : base(scene, x, y)
        {
            _bricks = bricks;
        }
    public bool IsExploding => _exploding;
    public Action? OnExplode;

    public void UpdateExplode(float deltaTime)
    {
        _explodeTimer -= deltaTime;
        if (_explodeTimer <= 0) _exploding = false;
    }

    public void DrawExplode(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X - 1, (int)Y - 1, "*   *", ConsoleColor.Yellow);
        buffer.WriteText((int)X - 1, (int)Y, " * * ", ConsoleColor.Red);
        buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
    }

    public override void Hit()
        {
        IsActive = false;
        _exploding = true;
        _explodeTimer = 0.3f;
        OnExplode?.Invoke(); // 폭발 알림
        foreach (var b in _bricks)
            if (Math.Abs(b.X - X) <= 4 && Math.Abs(b.Y - Y) <= 2)
                if (b is not InvincibleBrick)
                    b.IsActive = false;

    }

    public override void Update(float deltaTime)
        {
          
        }

        public override void Draw(ScreenBuffer buffer)
        {
            if (_exploding)
            {
                buffer.WriteText((int)X - 1, (int)Y - 1, "*   *", ConsoleColor.Yellow);
                buffer.WriteText((int)X - 1, (int)Y, " * * ", ConsoleColor.Red);
                buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
            }
            else if (IsActive)
            {
                buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Magenta);
            }
        }
    }
