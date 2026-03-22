using Framework.Engine;

// 무적 벽돌 — 공에 맞아도 절대 깨지지 않음
public class InvincibleBrick : Brick
{
    public InvincibleBrick(Scene scene, int x, int y) : base(scene, x, y) { }

    public override void Hit() { } // 아무 동작도 하지 않음

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Gray);
    }
}

// 강화 벽돌 — 두 번 맞아야 깨짐, hp에 따라 색상 변경
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

            // Fix3: new Random() 매번 생성 → Brick 공유 static _rng 사용
            if (CurrentStage > 1 && _rng.NextDouble() < 0.08)
                OnHit?.Invoke();
        }
    }

    public override void Draw(ScreenBuffer buffer)
    {
        ConsoleColor color = _hp == 2 ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
        buffer.WriteText((int)X, (int)Y, "■■", color);
    }
}

// 폭탄 벽돌 — 맞으면 주변 벽돌도 같이 파괴, 폭발 연출 재생
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
        buffer.WriteText((int)X - 1, (int)Y,     " * * ", ConsoleColor.Red);
        buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
    }

    public override void Hit()
    {
        IsActive = false;
        _exploding = true;
        _explodeTimer = 0.3f;
        OnExplode?.Invoke();

        // Fix5: 주변 벽돌 파괴 시 b.IsActive = false 직접 할당 대신
        //       b.Hit() 호출 → OnHit 콜백(점수, 아이템 드랍)이 정상 동작
        foreach (var b in _bricks)
        {
            if (!b.IsActive) continue;
            if (b is InvincibleBrick) continue;
            if (Math.Abs(b.X - X) <= 4 && Math.Abs(b.Y - Y) <= 2)
                b.Hit(); // IsActive = false + OnHit 콜백까지 정상 처리
        }
    }

    public override void Update(float deltaTime) { }

    public override void Draw(ScreenBuffer buffer)
    {
        if (_exploding)
        {
            buffer.WriteText((int)X - 1, (int)Y - 1, "*   *", ConsoleColor.Yellow);
            buffer.WriteText((int)X - 1, (int)Y,     " * * ", ConsoleColor.Red);
            buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
        }
        else if (IsActive)
        {
            buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Magenta);
        }
    }
}
