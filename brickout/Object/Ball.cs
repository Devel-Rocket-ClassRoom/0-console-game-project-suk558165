using Framework.Engine;
using System.Linq;

public class Ball : GameObject
{
    public float X { get; set; }
    public float Y { get; set; }

    // DX/DY는 항상 정수 (-1, 0, 1)
    // 콘솔 그리드 기반이므로 float 벡터는 충돌 오차 원인
    public float DX { get; set; } = 0f;
    public float DY { get; set; } = 1f;

    private bool _Waiting = true;
    private float _moveTimer = 0f;
    private float _moveInterval = 0.08f;
    private Paddle paddle;
    private List<Brick> bricks;

    public static bool BottomWallActive { get; set; } = false;
    private const float BottomWallY = 23f;

    private const float BrickW = 2f;
    private const float BrickH = 1f;

    // 판정 여유 — 0.45f 미만이면 모서리 통과 위험, 초과면 판정이 너무 넓어짐
    private const float Margin = 0.45f;

    // 축별 쿨다운
    private int _xCD = 0;
    private int _yCD = 0;
    private int _pCD = 0; // 패들
    private const int CD = 4;

    // 마지막 벽돌 자동 조준
    public bool AutoAim { get; set; } = false;
    private float _autoAimTimer = 0f;
    private const float AutoAimInterval = 1.5f;

    // 패들 반사 후 다음 X이동 방향 예약
    // (패들 판정 직후 DX를 바꾸면 같은 프레임 X이동에 반영되어 이상해지는 문제 방지)
    private float? _pendingDX = null;

    public Ball(Scene scene, Paddle paddle, List<Brick> bricks) : base(scene)
    {
        this.paddle = paddle;
        this.bricks = bricks;
        X = paddle.X + paddle.Width / 2f;
        Y = paddle.Y - 1f;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)Math.Round(X), (int)Math.Round(Y), "●", ConsoleColor.Blue);
    }

    public override void Update(float deltaTime)
    {
        if (_Waiting)
        {
            if (Input.IsKeyDown(ConsoleKey.Spacebar))
                _Waiting = false;
            return;
        }

        _moveTimer += deltaTime;
        if (_moveTimer < _moveInterval) return;
        _moveTimer -= _moveInterval;

        // 예약된 DX 적용
        if (_pendingDX.HasValue)
        {
            DX = _pendingDX.Value;
            _pendingDX = null;
        }

        // 쿨다운 차감 — 이동 전에 먼저
        if (_xCD > 0) _xCD--;
        if (_yCD > 0) _yCD--;
        if (_pCD > 0) _pCD--;

        float prevX = X;
        float prevY = Y;

        // ── X 이동 + 벽돌 X충돌 ──
        X += DX;
        if (_xCD == 0) ResolveX(prevX, prevY);

        // ── Y 이동 + 벽돌 Y충돌 ──
        Y += DY;
        if (_yCD == 0) ResolveY(prevY);

        // ── 사이드 벽 ──
        if (X < 1f && DX < 0)        { DX =  1f; X = 1f; }
        else if (X > 58f && DX > 0)  { DX = -1f; X = 58f; }

        // ── 천장 ──
        if (Y <= 1f && DY < 0)       { DY =  1f; Y = 1f; }

        // ── 하단 wall ──
        if (BottomWallActive && Y >= BottomWallY && DY > 0)
        {
            DY = -1f;
            Y  = BottomWallY - 1f;
        }

        // ── 패들 충돌 ──
        // 판정: 공이 DY>0(아래로) + 패들 Y범위 안 + 패들 X범위 안
        if (_pCD == 0 && DY > 0 &&
            Y  >= paddle.Y - 1f && Y <= paddle.Y &&
            X  >= paddle.X      && X <= paddle.X + paddle.Width)
        {
            // 위치 보정 — 패들 바로 위로
            Y = paddle.Y - 1f;

            // DY는 무조건 위로
            DY = -1f;

            // ── 반사 각도: 히트 위치에 따라 DX 결정 ──
            // 패들을 3구역으로 나눔
            // 왼쪽 1/3  → DX = -1
            // 중앙 1/3  → DX = 기존 DX 유지 (수직에 가깝게)
            // 오른쪽 1/3 → DX = +1
            float third = paddle.Width / 3f;
            float relX  = X - paddle.X;

            float newDX;
            if (relX < third)
                newDX = -1f;                          // 왼쪽 → 왼쪽으로
            else if (relX > paddle.Width - third)
                newDX = 1f;                           // 오른쪽 → 오른쪽으로
            else
                newDX = DX == 0f ? 1f : DX;          // 중앙 → 기존 방향 유지

            // 패들 이동 방향 영향 — 같은 방향으로 치면 끝 방향으로 강제
            if (Input.IsKey(ConsoleKey.LeftArrow)  && newDX > 0f) newDX = -1f;
            if (Input.IsKey(ConsoleKey.RightArrow) && newDX < 0f) newDX =  1f;

            // DX는 다음 프레임에 적용 (이번 프레임 X이동이 끝난 후이므로 즉시 적용해도 무방)
            DX = newDX;

            _pCD = CD;
        }

        // ── 마지막 벽돌 자동 조준 ──
        if (AutoAim)
        {
            _autoAimTimer += deltaTime;
            if (_autoAimTimer >= AutoAimInterval)
            {
                _autoAimTimer = 0f;
                var target = bricks.FirstOrDefault(b => b.IsActive && b is not InvincibleBrick);
                if (target != null)
                {
                    float targetCenterX = target.X + BrickW / 2f;
                    if (Math.Abs(X - targetCenterX) > 3f)
                    {
                        bool right = targetCenterX > X;
                        if (right  && DX < 0f) DX = 1f;
                        if (!right && DX > 0f) DX = -1f;
                    }
                }
            }
        }
    }

    public void Launch()
    {
        _Waiting = false;
        DY = -1f;
        DX =  0f;
    }

    public void SetInterval(float interval) => _moveInterval = interval;

    // X 이동 후 벽돌 좌/우면 충돌
    // Y는 미이동 → prevY == Y → prevY로만 Y범위 체크
    private void ResolveX(float prevX, float prevY)
    {
        if (DX == 0f) return;

        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;

            float bL = B.X;
            float bR = B.X + BrickW;
            float bT = B.Y;
            float bB = B.Y + BrickH;

            // Y범위 체크 (미이동이므로 prevY == Y)
            if (prevY < bT - Margin || prevY > bB + Margin) continue;

            // 이전에 이미 X범위 안이었으면 X방향 진입 아님
            if (prevX >= bL - Margin && prevX <= bR + Margin) continue;

            if (DX > 0f && X + Margin >= bL && prevX + Margin < bL)
            {
                DX = -1f;
                X  = bL - Margin - 0.01f;
                _xCD = CD;
                B.Hit();
                return;
            }
            if (DX < 0f && X - Margin <= bR && prevX - Margin > bR)
            {
                DX = 1f;
                X  = bR + Margin + 0.01f;
                _xCD = CD;
                B.Hit();
                return;
            }
        }
    }

    // Y 이동 후 벽돌 위/아래면 충돌
    // X는 이동 완료 → 현재 X만으로 범위 체크
    private void ResolveY(float prevY)
    {
        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;

            float bL = B.X;
            float bR = B.X + BrickW;
            float bT = B.Y;
            float bB = B.Y + BrickH;

            // X범위 체크 (이동 완료된 현재 X만 사용)
            if (X < bL - Margin || X > bR + Margin) continue;

            // 이전에 이미 Y범위 안이었으면 Y방향 진입 아님
            if (prevY >= bT - Margin && prevY <= bB + Margin) continue;

            if (DY > 0f && Y + Margin >= bT && prevY + Margin < bT)
            {
                DY = -1f;
                Y  = bT - Margin - 0.01f;
                _yCD = CD;
                B.Hit();
                return;
            }
            if (DY < 0f && Y - Margin <= bB && prevY - Margin > bB)
            {
                DY = 1f;
                Y  = bB + Margin + 0.01f;
                _yCD = CD;
                B.Hit();
                return;
            }
        }
    }
}
