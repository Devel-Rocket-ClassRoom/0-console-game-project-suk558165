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
        if (X >= paddle.X && X <= paddle.X + paddle.Width && (int)Y == (int)paddle.Y)
        {
            DY *= -1; //Y 방향 반전
            Y = paddle.Y - 1;  // 패들 위로 밀어넣기

            float hitpos = X - paddle.X; // x 방향 각도 조절
            float center = hitpos - paddle.Width / 2f; // 패들 왼쪽 끝 기준 충돌 위치
            DX = (float)Math.Round(center / (paddle.Width / 2f) * 2f); // 중심 

            if (DX == 0) DX = (DY > 0 ? 1 : -1); // 수직 반사 방지
        }

        bool hit = false; // 한 프레임 여러 벽돌 중복 충돌 방지

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