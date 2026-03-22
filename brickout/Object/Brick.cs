using Framework.Engine;

public class Brick : GameObject
{
    public float X { get; set; }
    public float Y { get; set; }

    // 현재 스테이지 — PlayScene에서 설정, 1스테이지는 아이템 드랍 없음
    public static int CurrentStage { get; set; } = 1;

    // Fix3: HardBrick, BombBrick 등 자식 클래스에서 공유할 수 있도록 protected
    //       매번 new Random() 생성 시 시드가 몰려 난수가 편향되는 문제 방지
    protected static readonly Random _rng = new Random();

    private const float ItemDropChance = 0.08f;

    public Brick(Scene scene, int x, int y) : base(scene)
    {
        X = x;
        Y = y;
    }

    public Action? OnHit;

    public virtual void Hit()
    {
        IsActive = false;

        // 1스테이지: 드랍 없음 / 2스테이지 이상: 8% 확률
        if (CurrentStage > 1 && _rng.NextDouble() < ItemDropChance)
            OnHit?.Invoke();
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Red);
    }

    public override void Update(float deltaTime) { }
}
