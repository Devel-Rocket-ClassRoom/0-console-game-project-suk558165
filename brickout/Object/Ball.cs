using Framework.Engine;

public class Ball : GameObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public float DX { get; set; } = 0;
    public float DY { get; set; } = -1; // Fix6: 위로 발사

    private bool _Waiting = true;
    private float _moveTimer = 0f;
    private float _moveInterval = 0.08f; // 스테이지별로 SetInterval()로 조정
    private Paddle paddle;
    private List<Brick> bricks;
    private Brick? _lastHitBrick = null;
    private int _hitCooldown = 0;

    public static bool BottomWallActive { get; set; } = false;
    private const float BottomWallY = 23f; // Fix9: float

    private const float BrickW = 2f;
    private const float BrickH = 1f;
    private const float Margin = 0.45f;

    private int _xCooldown = 0;
    private int _yCooldown = 0;
    private const int CooldownFrames = 3;

    public Ball(Scene scene, Paddle paddle, List<Brick> bricks) : base(scene)
    {
        this.paddle = paddle;
        this.bricks = bricks;
        X = 30;
        Y = 16;
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

        if (_hitCooldown > 0) _hitCooldown--;
        if (_xCooldown > 0)   _xCooldown--;
        if (_yCooldown > 0)   _yCooldown--;

        float prevX = X;
        float prevY = Y;

        // X 이동 → X축 충돌
        X += DX;
        CheckBrickCollisionX(prevX, prevY);

        // Y 이동 → Y축 충돌
        Y += DY;
        CheckBrickCollisionY(prevX, prevY);

        // 벽 충돌 (방향 강제)
        if (X < 1f && DX < 0)       { DX = Math.Abs(DX);  X = 1f; }
        else if (X > 58f && DX > 0) { DX = -Math.Abs(DX); X = 58f; }
        if (Y <= 1f && DY < 0)      { DY = Math.Abs(DY);  Y = 1f; }

        // 하단 wall 아이템 충돌 — Fix8: Math.Abs 방향 강제
        if (BottomWallActive && Y >= BottomWallY && DY > 0)
        {
            DY = -Math.Abs(DY);
            Y  = BottomWallY - 1f;
        }

        // 패들 충돌 — Fix8: Math.Abs 방향 강제로 진동 방지
        if (DY > 0 &&
            X >= paddle.X - Margin && X <= paddle.X + paddle.Width + Margin &&
            Y >= paddle.Y - 0.5f   && Y <= paddle.Y + 0.5f)
        {
            DY = -Math.Abs(DY);
            Y  = paddle.Y - 0.5f;

            float hitpos = X - paddle.X;
            float center = hitpos - paddle.Width / 2f;
            DX = Math.Clamp((float)Math.Round(center / (paddle.Width / 2f)), -1f, 1f);
            if (DX == 0) DX = center >= 0 ? 1f : -1f;
        }
    }

    public void Launch()
    {
        _Waiting = false;
        DY = -1f;
        DX = 0f;
    }

    // 스테이지 시작 시 PlayScene에서 호출 — StageData.BallInterval 값을 그대로 넘기면 됨
    public void SetInterval(float interval) => _moveInterval = interval;

    // X 이동 직후 호출 — 이 시점에 Y는 아직 이동 전(Y == prevY)
    // Fix1: prevY 하나로만 Y범위 체크 (이동 전/후 동일하므로 정확)
    private void CheckBrickCollisionX(float prevX, float prevY)
    {
        if (DX == 0 || _xCooldown > 0) return;

        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;
            if (B == _lastHitBrick && _hitCooldown > 0) continue;

            float bLeft   = B.X;
            float bRight  = B.X + BrickW;
            float bTop    = B.Y;
            float bBottom = B.Y + BrickH;

            // Fix1: X이동 시점엔 Y == prevY이므로 prevY만으로 체크
            if (prevY < bTop - Margin || prevY > bBottom + Margin) continue;

            if (DX > 0)
            {
                // Fix4: < 로 수정 (경계선 위에 있을 때도 판정)
                if (prevX + Margin < bLeft && X + Margin >= bLeft)
                {
                    DX = -Math.Abs(DX);
                    X  = bLeft - Margin;
                    _lastHitBrick = B;
                    _hitCooldown  = CooldownFrames;
                    _xCooldown    = CooldownFrames;
                    B.Hit();
                    break;
                }
            }
            else
            {
                // Fix4: > 로 수정
                if (prevX - Margin > bRight && X - Margin <= bRight)
                {
                    DX = Math.Abs(DX);
                    X  = bRight + Margin;
                    _lastHitBrick = B;
                    _hitCooldown  = CooldownFrames;
                    _xCooldown    = CooldownFrames;
                    B.Hit();
                    break;
                }
            }
        }
    }

    // Y 이동 직후 호출 — 이 시점에 X는 이미 이동 완료
    // prevX, X 둘 다 체크해서 X이동으로 인한 오판 방지
    private void CheckBrickCollisionY(float prevX, float prevY)
    {
        if (_yCooldown > 0) return;

        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;
            if (B == _lastHitBrick && _hitCooldown > 0) continue;

            float bLeft   = B.X;
            float bRight  = B.X + BrickW;
            float bTop    = B.Y;
            float bBottom = B.Y + BrickH;

            // X는 이동 완료 — 현재 X와 이동 전 prevX 중 하나라도 범위 안이면 인정
            bool xInRange = (X     >= bLeft - Margin && X     <= bRight + Margin) ||
                            (prevX >= bLeft - Margin && prevX <= bRight + Margin);
            if (!xInRange) continue;

            if (DY > 0)
            {
                // Fix4: <
                if (prevY + Margin < bTop && Y + Margin >= bTop)
                {
                    DY = -Math.Abs(DY);
                    Y  = bTop - Margin;
                    _lastHitBrick = B;
                    _hitCooldown  = CooldownFrames;
                    _yCooldown    = CooldownFrames;
                    B.Hit();
                    break;
                }
            }
            else
            {
                // Fix4: >
                if (prevY - Margin > bBottom && Y - Margin <= bBottom)
                {
                    DY = Math.Abs(DY);
                    Y  = bBottom + Margin;
                    _lastHitBrick = B;
                    _hitCooldown  = CooldownFrames;
                    _yCooldown    = CooldownFrames;
                    B.Hit();
                    break;
                }
            }
        }
    }
}
