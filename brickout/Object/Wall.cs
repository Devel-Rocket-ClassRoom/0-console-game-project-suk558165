using System;
using Framework.Engine;

// ════════════════════════════════════════
// 벽(Wall) 오브젝트 — 게임 영역 경계 및 하단 wall 아이템 구현
//
// 사용 예:
//   게임 영역 경계: new Wall(this, 0, 0, 60, 25)  → 전체 화면 테두리
//   하단 벽 아이템: new Wall(this, 1, 23, 58, 1)  → 바닥 한 줄 벽
//
// 공의 벽 충돌은 Ball.cs에서 좌표 직접 비교로 처리
// (Wall 오브젝트는 시각적 렌더링 전담, 충돌 판정은 Ball이 담당)
// ════════════════════════════════════════
public class Wall : GameObject
{
    private int X;      // 벽 왼쪽 상단 X 좌표
    private int Y;      // 벽 왼쪽 상단 Y 좌표
    private int Width;  // 벽 가로 크기
    private int Height; // 벽 세로 크기

    public Wall(Scene scene, int x, int y, int width, int height) : base(scene)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // DrawBox: X,Y 위치에 Width×Height 크기의 테두리 박스를 그림
        // 내부는 비어 있고 테두리 선만 렌더링
        buffer.DrawBox(X, Y, Width, Height, ConsoleColor.White);
    }

    // 벽은 정적 오브젝트 — 움직이거나 상태가 바뀌지 않으므로 Update 불필요
    public override void Update(float deltaTime) { }
}
