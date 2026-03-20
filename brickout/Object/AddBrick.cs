using Framework.Engine;

// 무적 벽돌 — 공에 맞아도 절대 깨지지 않음
public class InvincibleBrick : Brick
{
    public InvincibleBrick(Scene scene, int x, int y) : base(scene, x, y) { }

    public override void Hit() { } // 아무 동작도 하지 않음 — 깨지지 않음

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Gray);
    }
}

// 강화 벽돌 — 두 번 맞아야 깨짐, hp에 따라 색상 변경
public class HardBrick : Brick
{
    private int _hp = 2; // 내구도 (2번 맞으면 깨짐)

    public HardBrick(Scene scene, int x, int y) : base(scene, x, y) { }

    public override void Hit()
    {
        _hp--;
        if (_hp <= 0)
        {
            IsActive = false;   // 내구도 0이 되면 비활성화
            OnHit?.Invoke();    // 완전히 깨질 때만 콜백 호출
        }
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // hp 2 = 노란색 (온전한 상태), hp 1 = 어두운 노란색 (금이 간 상태)
        ConsoleColor color = _hp == 2 ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
        buffer.WriteText((int)X, (int)Y, "■■", color);
    }
}

// 폭탄 벽돌 — 맞으면 주변 벽돌도 같이 파괴, 폭발 연출 재생
public class BombBrick : Brick
{
    private List<Brick> _bricks;      // 주변 벽돌 참조 (폭발 범위 체크용)
    private float _explodeTimer = 0f; // 폭발 연출 타이머
    private bool _exploding = false;  // 폭발 연출 재생 중 여부

    public BombBrick(Scene scene, int x, int y, List<Brick> bricks) : base(scene, x, y)
    {
        _bricks = bricks;
    }

    public bool IsExploding => _exploding; // 폭발 중인지 여부 (PlayScene에서 체크)
    public Action? OnExplode; // 폭발 시 호출되는 콜백 (PlayScene에서 폭발 목록에 추가)

    // PlayScene에서 IsActive가 false여도 폭발 타이머를 직접 업데이트
    public void UpdateExplode(float deltaTime)
    {
        _explodeTimer -= deltaTime;
        if (_explodeTimer <= 0) _exploding = false; // 타이머 종료 시 연출 종료
    }

    // PlayScene의 Draw에서 IsActive와 무관하게 폭발 연출 직접 그리기
    public void DrawExplode(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X - 1, (int)Y - 1, "*   *", ConsoleColor.Yellow);
        buffer.WriteText((int)X - 1, (int)Y, " * * ", ConsoleColor.Red);
        buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
    }

    public override void Hit()
    {
        IsActive = false;      // 자신 비활성화
        _exploding = true;     // 폭발 연출 시작
        _explodeTimer = 0.3f;  // 0.3초간 폭발 연출
        OnExplode?.Invoke();   // PlayScene에 폭발 알림

        // 주변 4칸 이내 벽돌 파괴 (무적 벽돌 제외)
        foreach (var b in _bricks)
            if (Math.Abs(b.X - X) <= 4 && Math.Abs(b.Y - Y) <= 2)
                if (b is not InvincibleBrick)
                    b.IsActive = false;
    }

    public override void Update(float deltaTime) { } // UpdateExplode로 대체

    public override void Draw(ScreenBuffer buffer)
    {
        if (_exploding) // 폭발 중이면 연출 표시
        {
            buffer.WriteText((int)X - 1, (int)Y - 1, "*   *", ConsoleColor.Yellow);
            buffer.WriteText((int)X - 1, (int)Y, " * * ", ConsoleColor.Red);
            buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
        }
        else if (IsActive) // 평상시 벽돌 표시
        {
            buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Magenta);
        }
    }
}