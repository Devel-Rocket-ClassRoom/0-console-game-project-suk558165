using Framework.Engine;

public class Ball : GameObject
{
    public float X { get; set; }  // X좌표
    public float Y { get; set; }  // Y좌표
    public float DX { get; set; } = 0; // X의 이동방향
    public float DY { get; set; } = 1; // Y의 이동방향

    private bool _Waiting = true;

    private float _moveTimer = 0f; // 이동 누적 시간

    private float _moveInterval = 0.05f; // 이동 간격
    private Paddle paddle; // 충돌 체크용 패들 참조
    private List<Brick> bricks; // 충돌 체크용 벽돌 참조


    public Ball(Scene scene, Paddle paddle, List<Brick> bricks) : base(scene) // 생성자
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
            {
                _Waiting = false;
            }
            return;
        }

        _moveTimer += deltaTime;
        if (_moveTimer < _moveInterval) return;
        _moveTimer = 0f;

        float prevX = X;  // 이전 위치 저장
        float prevY = Y;

        X += DX;
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

        // 패들 충돌
        if (X >= paddle.X && X <= paddle.X + paddle.Width && (int)Y == (int)paddle.Y)
        {
            DY *= -1;
            Y = paddle.Y - 1;  // 패들 위로 밀어넣기

            float hitpos = X - paddle.X;
            float center = hitpos - paddle.Width / 2f;
            DX = (float)Math.Round(center / (paddle.Width / 2f) * 2f);
            if (DX == 0) DX = (DY > 0 ? 1 : -1);
        }

        bool hit = false;
        // 벽돌 충돌
        foreach (Brick B in bricks)
        {
            if (B.IsActive && (int)X >= B.X && (int)X <= B.X + 2 && (int)Y >= B.Y && (int)Y <= B.Y + 1)
            {
                if (!hit)
                {
                    if (prevY < B.Y || prevY > B.Y + 1)
                    {
                        DY *= -1;  // 위아래 충돌
                    }
                    else
                    {
                        DX *= -1;  // 옆 충돌
                    }
                    B.IsActive = false;
                }
            }
        }
    }
}