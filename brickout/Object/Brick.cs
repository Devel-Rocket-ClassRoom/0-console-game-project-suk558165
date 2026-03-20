using Framework.Engine;

public class Brick : GameObject
{
    public float X { get; set; } // X 좌표
    public float Y { get; set; } // Y 좌표

    public Brick(Scene scene, int x, int y) : base(scene)
    {
        X = x;
        Y = y;
    }

    public Action? OnHit; // 벽돌이 완전히 깨질 때 호출되는 콜백 (아이템 생성 등)

    public virtual void Hit() // 공에 맞았을 때 호출 — 자식 클래스에서 오버라이드
    {
        IsActive = false; // 벽돌 비활성화 (화면에서 제거)
        OnHit?.Invoke();  // 콜백 호출
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "■■", ConsoleColor.Red);
    }

    public override void Update(float deltaTime) { }
}