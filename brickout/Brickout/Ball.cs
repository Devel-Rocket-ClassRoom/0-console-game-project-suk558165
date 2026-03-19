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
        _moveTimer += deltaTime; // 경과 시간 누적
        if (_moveTimer < _moveInterval) return;
        _moveTimer = 0f;

        X += DX;  
        Y += DY;

        if (X < 1 && DX < 0) // 왼쪽 벽 충돌: 왼쪽으로 가다가 경계 넘으면 오른쪽으로 반전
        {
            DX *= -1;
            X = 0.1f; // 벽 안쪽으로 밀어넣기 (연속 반전 방지)
        }

        else if (X > 58 && DX > 0) // 오른쪽 벽 충돌: 오른쪽으로 가다가 경계 넘으면 왼쪽으로 반전
        {
            DX *= -1;
            X = 57.9f;
        }
        if (Y < 1 && DY < 0) // 위로 가다 천장에 닿으면
        {
            DY *= -1;
            Y = 0.1f;
        }
     
        if (X >= paddle.X && X <= paddle.X + paddle.Width && Y >= paddle.Y && Y <= paddle.Y + 1) // 패들과의 충돌 판정 
        {
            DY *= -1;

            float hitpos = X - paddle.X; // 패들 왼쪽 끝 기준 거리
         


        } 
        foreach (Brick B in bricks) // 벽돌 순회문
        {
            if (B.IsActive && (int)X >= B.X && (int)X <= B.X + 2 && (int)Y >= B.Y && (int)Y <= B.Y + 1)
                {
                DY *= -1; // 튕기기
                B.IsActive = false;  // 벽돌 삭제
                }
        }
    }
}