using Framework.Engine;

public class Ball : GameObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public float DX { get; set; } = 0;
    public float DY { get; set; } = 1;

    private bool _Waiting = true;
    private float _moveTimer = 0f;
    private float _moveIntervalUp = 0.08f;
    private float _moveIntervalDown = 0.08f;
    private Paddle paddle;
    private List<Brick> bricks;
    private Brick? _lastHitBrick = null;
    private int _hitCooldown = 0;

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
        float interval = DY > 0 ? _moveIntervalDown : _moveIntervalUp;
        if (_moveTimer < interval) return;
        _moveTimer -= interval;

        if (_hitCooldown > 0) _hitCooldown--;

        float prevX = X;
        float prevY = Y;

        // X 이동 후 X방향 충돌 체크
        X += DX;
        CheckBrickCollisionX(prevX, prevY);

        // Y 이동 후 Y방향 충돌 체크
        Y += DY;
        CheckBrickCollisionY(prevX, prevY);

        // 왼쪽 벽 충돌
        if (X < 1 && DX < 0) { DX *= -1; X = 1f; }
        // 오른쪽 벽 충돌
        else if (X > 58 && DX > 0) { DX *= -1; X = 58f; }
        // 위쪽 벽 충돌
        if (Y <= 1 && DY < 0) { DY *= -1; Y = 1f; }

        // 패들 충돌
        if (X >= paddle.X && X <= paddle.X + paddle.Width && Y >= paddle.Y - 1 && Y <= paddle.Y + 1 && DY > 0)
        {
            DY *= -1;
            Y = paddle.Y - 1;

            float hitpos = X - paddle.X;
            float center = hitpos - paddle.Width / 2f;

            DX = Math.Clamp((float)Math.Round(center / (paddle.Width / 2f)), -1f, 1f);
            if (DX == 0) DX = center >= 0 ? 1 : -1;
        }
    }
    public void Launch()
    {
        _Waiting = false;
        DY = 1;
        DX = 0;
    }

    private void CheckBrickCollisionX(float prevX, float prevY)
    {
        // DX가 0이면 X이동이 없으니 체크 불필요
        if (DX == 0) return;

        int rx = DX < 0 ? (int)Math.Ceiling(X) : (int)Math.Floor(X);
        int ry = (int)Math.Round(Y);

        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;
            if (B == _lastHitBrick && _hitCooldown > 0) continue;

            int bLeft = (int)B.X;
            int bRight = (int)B.X + 1;
            int bTop = (int)B.Y;
            int bBottom = (int)B.Y + 1;

            // X방향에서만 진입했을 때만 판정
            // 이전 Y위치가 벽돌 범위 안에 있어야 옆에서 온 것
            if (rx >= bLeft && rx <= bRight &&
                ry >= bTop && ry <= bBottom &&
                prevY >= bTop && prevY <= bBottom + 1)
            {
                DX *= -1;
                X = prevX;
                _lastHitBrick = B;
                _hitCooldown = 3;
                B.Hit();
                break;
            }
        }
    }

    private void CheckBrickCollisionY(float prevX, float prevY)
    {
       
        int rx = (int)Math.Round(X);
        int ry = (int)Math.Round(Y);

        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;
            if (B == _lastHitBrick && _hitCooldown > 0) continue;

            if (rx >= (int)B.X && rx <= (int)B.X + 4 &&
                ry >= (int)B.Y && ry <= (int)B.Y + 1)
            {
                DY *= -1;
                Y = prevY;
                _lastHitBrick = B;
                _hitCooldown = 3;
                B.Hit();
                break;
            }
        }
    }
}