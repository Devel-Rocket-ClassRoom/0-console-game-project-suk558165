using Framework.Engine;

public class Ball : GameObject
{
    public float X { get; set; }  // X좌표
    public float Y { get; set; }  // Y좌표
    public float DX { get; set; } = 0; // X의 이동방향
    public float DY { get; set; } = 1; // Y의 이동방향

    private bool _Waiting = true; // 스페이스바를 누르기 전 대기 상태 여부

    private float _moveTimer = 0f; // 이동 누적 시간

    private float _moveIntervalUp = 0.08f; // 올라갈 때 속도
    private float _moveIntervalDown = 0.08f; // 내려올 때 속도
    private Paddle paddle; // 충돌 체크용 패들 참조
    private List<Brick> bricks; // 충돌 체크용 벽돌 참조
    private Brick? _lastHitBrick = null;
    private int _hitCooldown = 0;

    public Ball(Scene scene, Paddle paddle, List<Brick> bricks) : base(scene) // 생성자
    {
        this.paddle = paddle;
        this.bricks = bricks;
        X = 30; // 초기 위치
        Y = 16; // 초기 위치
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)Math.Round(X), (int)Math.Round(Y), "●", ConsoleColor.Blue);
    }

    public override void Update(float deltaTime)
    {
        if (_Waiting) // 대기 상태
        {
            if (Input.IsKeyDown(ConsoleKey.Spacebar)) // 스페이스바 입력 시 시작
            {
                _Waiting = false;
            }
            return;
        }

        _moveTimer += deltaTime;  // 이동 타이머 누적

        float interval = DY > 0 ? _moveIntervalDown : _moveIntervalUp; // 이동 방향에 따라 속도 간격 결정
        if (_moveTimer < interval) return; // 간격에 도달하지 않으면 이동 하지 않는다/.
        _moveTimer -= interval; // 타이머 에서 간격만큼 차감 

        float prevX = X;  // 이전 위치 저장
        float prevY = Y;

        X += DX; // 현재 방향으로 한 칸 이동
        Y += DY;

        // 왼쪽 벽 충돌
        if (X < 1 && DX < 0)
        {
            DX *= -1;
            X = 1.1f;

        }

        // 오른쪽 벽 충돌
        else if (X > 58 && DX > 0)
        {
            DX *= -1;
            X = 57.9f;
        }

        // 위쪽 벽 충돌
        if (Y < 1 && DY < 0)
        {
            DY *= -1;
            Y = 1.1f;
        }

        // 패들 충돌감지 : 공이 패들 범위 내에 있고 같은 y행에 위치할때
        if (X >= paddle.X && X <= paddle.X + paddle.Width && Y >= paddle.Y - 1 && Y <= paddle.Y + 1 && DY > 0)
        {
            DY *= -1;
            Y = paddle.Y - 1;

            float hitpos = X - paddle.X;
            float center = hitpos - paddle.Width / 2f;

            DX = (float)Math.Round(center / (paddle.Width / 2f) * 2f);

            // DY 반전 전 기준으로 방향 설정
            if (DX == 0) DX = center >= 0 ? 1 : -1;  // 중앙 기준 오른쪽이면 오른쪽, 왼쪽이면 왼쪽
        }

        bool hit = false;
        if (_hitCooldown > 0) _hitCooldown--;

        // 충돌 체크 전에 반올림된 위치로 판정
        int rx = (int)Math.Round(X);
        int ry = (int)Math.Round(Y);
        int prevRx = (int)Math.Round(prevX);  
        int prevRy = (int)Math.Round(prevY);  

        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;
            if (hit) break;
            if (B == _lastHitBrick && _hitCooldown > 0) continue;

            float bLeft = B.X;
            float bRight = B.X + 2;
            float bTop = B.Y;
            float bBottom = B.Y + 1;

            // 반올림된 위치로 체크
            if (rx < bLeft || rx > bRight || ry < bTop || ry > bBottom) continue;

            hit = true;
            _lastHitBrick = B;
            _hitCooldown = 2;

            bool fromTop = prevRy < bTop;
            bool fromBottom = prevRy > bBottom;
            bool fromLeft = prevRx < bLeft;
            bool fromRight = prevRx > bRight;

            if (fromTop) { DY *= -1; Y = bTop - 1f; }
            else if (fromBottom) { DY *= -1; Y = bBottom + 1f; }
            else if (fromLeft) { DX *= -1; X = bLeft - 1f; }
            else if (fromRight) { DX *= -1; X = bRight + 1f; }
            else { DY *= -1; Y = bTop - 1f; }

            B.Hit();
            DX = Math.Clamp(DX, -2f, 2f);
            DY = Math.Clamp(DY, -1f, 1f);
        }
    }
}