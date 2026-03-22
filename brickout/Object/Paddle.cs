using System;
using Framework.Engine;

public class Paddle : GameObject
{
    public float X { get; set; }      // 패들 왼쪽 끝 X 좌표 (float — 부드러운 이동을 위해)
    public float Y { get; set; }      // 패들 Y 좌표 (고정 — 수직 이동 없음)
    public float Speed { get; set; } = 30f; // 이동 속도 (초당 칸 수)
                                            // SlowPaddle 아이템: 10f로 감소, 2초 후 30f로 복원
    public int Width { get; private set; } = 12; // 패들 가로 길이 (칸 수)
                                                  // Draw의 "◀■■■■■■■■■▶" = 12칸과 일치

    public Paddle(Scene scene) : base(scene)
    {
        X = 25; // 화면(60칸) 중앙 근처에서 시작
        Y = 22; // 바닥(Y=24) 위 2칸 — 공이 바닥에 닿기 전 여유 공간 확보
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // ◀ ▶: 패들 끝 표시로 범위를 직관적으로 전달
        // ■■■■■■■■■: 9개 × 1칸 = 9칸 + ◀▶ 각 1칸 = 총 11칸
        // Width=12와 맞추려면 공백 포함 렌더링 방식에 따라 조정 필요
        buffer.WriteText((int)X, (int)Y, "◀■■■■■■■■■▶", ConsoleColor.White);
    }

    public override void Update(float deltaTime)
    {
        // ── 좌우 이동 ──
        // Speed * deltaTime: 프레임레이트와 무관하게 일정 속도 유지
        if (Input.IsKey(ConsoleKey.LeftArrow))
            X -= Speed * deltaTime;

        if (Input.IsKey(ConsoleKey.RightArrow))
            X += Speed * deltaTime;

        // ── 화면 경계 클램핑 ──
        // X < 0: 왼쪽 벽 안으로 들어가지 않도록
        // X > 60 - Width: 오른쪽 벽 안으로 들어가지 않도록
        if (X < 0)          X = 0;
        if (X > 60 - Width) X = 60 - Width;
    }
}
