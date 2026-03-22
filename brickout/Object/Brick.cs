using Framework.Engine;

public class Brick : GameObject
{
    public float X { get; set; } // X 좌표
    public float Y { get; set; } // Y 좌표

    // 현재 스테이지를 외부(PlayScene)에서 설정 — 1스테이지면 아이템 드랍 없음
    public static int CurrentStage { get; set; } = 1;

    // 아이템 드랍 확률 (0.0 ~ 1.0) — 기본 8% (1스테이지는 강제 0%)
    private const float ItemDropChance = 0.08f;

    private static readonly Random _rng = new Random();

    public Brick(Scene scene, int x, int y) : base(scene)
    {
        X = x;
        Y = y;
    }

    // 벽돌이 완전히 깨질 때 호출되는 콜백 (아이템 생성 등)
    // 확률 판정 통과 시에만 Invoke — PlayScene에서 아이템 생성 로직을 연결
    public Action? OnHit;

    public virtual void Hit() // 공에 맞았을 때 호출 — 자식 클래스에서 오버라이드
    {
        IsActive = false; // 벽돌 비활성화 (화면에서 제거)

        // 1스테이지: 아이템 드랍 없음
        // 2스테이지 이상: ItemDropChance 확률로만 드랍
        if (CurrentStage > 1 && _rng.NextDouble() < ItemDropChance)
            OnHit?.Invoke();
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Red);
    }

    public override void Update(float deltaTime) { }
}