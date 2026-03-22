using Framework.Engine;

public class Brick : GameObject
{
    public float X { get; set; } // 벽돌 왼쪽 상단 X 좌표
    public float Y { get; set; } // 벽돌 왼쪽 상단 Y 좌표

    // ── 현재 스테이지 번호 (static — 모든 벽돌 인스턴스가 공유) ──
    // PlayScene의 Load()에서 스테이지 시작 시 설정
    // 1스테이지이면 아이템 드랍 완전 차단
    public static int CurrentStage { get; set; } = 1;

    // ── 공유 난수 생성기 (static — 모든 벽돌 인스턴스가 공유) ──
    // protected: 자식 클래스(HardBrick, BombBrick 등)에서도 접근 가능
    // static으로 하나만 유지: 매번 new Random()을 호출하면 빠르게 연속 호출 시
    // 시스템 시각 기반 시드가 동일해져 항상 같은 결과가 나오는 편향 문제 방지
    protected static readonly Random _rng = new Random();

    // 아이템 드랍 확률 — 2스테이지 이상에서 8% 확률로 드랍
    private const float ItemDropChance = 0.08f;

    public Brick(Scene scene, int x, int y) : base(scene)
    {
        X = x;
        Y = y;
    }

    // ── 피격 콜백 ──
    // 벽돌이 완전히 깨질 때 PlayScene에서 등록한 람다가 호출됨
    // 아이템 생성, 점수 추가 등의 처리를 PlayScene에서 담당
    // 확률 판정은 Hit() 내부에서 선행 — 통과 시에만 Invoke
    public Action? OnHit;

    // ── 피격 처리 (가상 메서드 — 자식 클래스에서 오버라이드) ──
    public virtual void Hit()
    {
        IsActive = false; // 벽돌 비활성화 → 다음 프레임부터 Draw/충돌 판정 제외

        // 1스테이지: 아이템 드랍 없음 (플레이어가 게임 시스템에 익숙해지는 단계)
        // 2스테이지 이상: ItemDropChance(8%) 확률로 아이템 드랍
        if (CurrentStage > 1 && _rng.NextDouble() < ItemDropChance)
            OnHit?.Invoke();
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // "■■": 가로 2칸, 빨간색 일반 벽돌
        // BrickW=2와 일치해야 충돌 판정과 시각적 크기가 맞음
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Red);
    }

    public override void Update(float deltaTime) { } // 일반 벽돌은 상태 업데이트 없음
}
