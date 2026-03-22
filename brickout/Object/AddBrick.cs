using Framework.Engine;

// ════════════════════════════════════════
// 무적 벽돌 — 공에 맞아도 절대 깨지지 않음
// 아이템 드랍 없음, 클리어 조건에서도 제외
// (PlayScene의 클리어 판정: All(b => !b.IsActive || b is InvincibleBrick))
// ════════════════════════════════════════
public class InvincibleBrick : Brick
{
    public InvincibleBrick(Scene scene, int x, int y) : base(scene, x, y) { }

    // Hit()을 빈 메서드로 오버라이드 → 아무 동작도 하지 않음
    // IsActive = false가 호출되지 않으므로 공이 맞아도 사라지지 않음
    public override void Hit() { }

    public override void Draw(ScreenBuffer buffer)
    {
        // 회색으로 표시 — 플레이어에게 "깰 수 없음"을 시각적으로 전달
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Gray);
    }
}

// ════════════════════════════════════════
// 강화 벽돌 — 두 번 맞아야 깨짐
// HP에 따라 색상이 변해 플레이어에게 피격 횟수를 시각적으로 알려줌
// ════════════════════════════════════════
public class HardBrick : Brick
{
    private int _hp = 2; // 내구도: 2 → 1 → 0(파괴)

    public HardBrick(Scene scene, int x, int y) : base(scene, x, y) { }

    public override void Hit()
    {
        _hp--;

        if (_hp <= 0)
        {
            IsActive = false; // 내구도 0이 되면 비활성화

            // Brick의 shared _rng 사용 (부모 클래스의 protected static 필드)
            // 1스테이지 아이템 드랍 없음, 2스테이지 이상 8% 확률
            if (CurrentStage > 1 && _rng.NextDouble() < 0.08)
                OnHit?.Invoke(); // 완전히 깨질 때만 아이템 드랍 판정
        }
        // _hp > 0이면 IsActive = true 유지 → 벽돌이 사라지지 않고 색만 바뀜
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // HP 2(온전): 노란색 / HP 1(금이 간 상태): 어두운 노란색
        // 플레이어가 몇 번 더 맞혀야 하는지 직관적으로 파악 가능
        ConsoleColor color = _hp == 2 ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
        buffer.WriteText((int)X, (int)Y, "■■", color);
    }
}

// ════════════════════════════════════════
// 폭탄 벽돌 — 맞으면 주변 벽돌 연쇄 파괴 + 폭발 연출 재생
//
// 설계 포인트:
//   - IsActive = false가 되어도 _exploding 중에는 DrawExplode()로 별도 렌더링
//   - UpdateExplode()는 PlayScene의 _explodingBombs 목록에서 직접 호출
//     (IsActive=false이면 UpdateGameObjects에서 Update가 호출되지 않으므로)
// ════════════════════════════════════════
public class BombBrick : Brick
{
    private List<Brick> _bricks;       // 전체 벽돌 목록 참조 — 폭발 범위 내 벽돌 파괴용
    private float _explodeTimer = 0f;  // 폭발 연출 남은 시간
    private bool _exploding = false;   // 폭발 연출 재생 중 여부

    public BombBrick(Scene scene, int x, int y, List<Brick> bricks) : base(scene, x, y)
    {
        _bricks = bricks;
    }

    // PlayScene에서 폭발 종료 여부 체크에 사용
    public bool IsExploding => _exploding;

    // 폭발 시 PlayScene이 this를 _explodingBombs 목록에 추가하도록 알림
    public Action? OnExplode;

    // PlayScene의 Update에서 직접 호출 (IsActive=false 상태에서도 타이머 감소 필요)
    public void UpdateExplode(float deltaTime)
    {
        _explodeTimer -= deltaTime;
        if (_explodeTimer <= 0) _exploding = false; // 타이머 종료 → 연출 종료
    }

    // PlayScene의 Draw에서 IsActive와 무관하게 직접 호출
    public void DrawExplode(ScreenBuffer buffer)
    {
        // 3x5 폭발 연출 (중심 기준 ±1칸)
        buffer.WriteText((int)X - 1, (int)Y - 1, "*   *", ConsoleColor.Yellow);
        buffer.WriteText((int)X - 1, (int)Y,     " * * ", ConsoleColor.Red);
        buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
    }

    public override void Hit()
    {
        IsActive = false;      // 자신 비활성화
        _exploding = true;     // 폭발 연출 시작
        _explodeTimer = 0.3f;  // 0.3초간 폭발 연출 유지
        OnExplode?.Invoke();   // PlayScene에 폭발 알림 → _explodingBombs에 추가

        // ── 주변 벽돌 연쇄 파괴 ──
        // 범위: X방향 ±4칸, Y방향 ±2칸 이내
        // b.Hit() 호출: IsActive=false + OnHit 콜백(아이템 드랍) 모두 정상 처리
        //   (b.IsActive = false 직접 할당하면 OnHit 콜백이 누락됨)
        // InvincibleBrick은 Hit()이 빈 메서드이므로 자연스럽게 제외됨
        foreach (var b in _bricks)
        {
            if (!b.IsActive) continue;           // 이미 파괴된 벽돌 스킵
            if (b is InvincibleBrick) continue;  // 무적 벽돌 명시적 제외
            if (Math.Abs(b.X - X) <= 4 && Math.Abs(b.Y - Y) <= 2)
                b.Hit();
        }
    }

    // UpdateExplode로 대체 — 일반 Update 루프에서는 처리하지 않음
    public override void Update(float deltaTime) { }

    public override void Draw(ScreenBuffer buffer)
    {
        if (_exploding)
        {
            // 폭발 연출 중 — 위치 고정으로 DrawExplode와 동일한 내용 표시
            buffer.WriteText((int)X - 1, (int)Y - 1, "*   *", ConsoleColor.Yellow);
            buffer.WriteText((int)X - 1, (int)Y,     " * * ", ConsoleColor.Red);
            buffer.WriteText((int)X - 1, (int)Y + 1, "*   *", ConsoleColor.Yellow);
        }
        else if (IsActive)
        {
            // 평상시 — 마젠타색으로 일반 벽돌과 구별
            buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Magenta);
        }
        // IsActive=false이고 _exploding=false → 완전 소멸, 아무것도 그리지 않음
    }
}
