using Framework.Engine;

public class Ball : GameObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public float DX { get; set; } = 1;
    public float DY { get; set; } = -1;
   
    private float _moveTimer = 0f;

    private float _moveInterval = 0.05f;


    public Ball(Scene scene) : base(scene)
    {
        X = 30;
        Y = 16;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)Math.Round(X), (int)Math.Round(Y), "●", ConsoleColor.Blue);
    }

    public override void Update(float deltaTime)
    {
        _moveTimer += deltaTime;
        if (_moveTimer < _moveInterval) return;
        _moveTimer = 0f;

        X += DX;  
        Y += DY;

        if (X < 1 && DX < 0) // 0보다 작으면서 "왼쪽으로 가고 있을 때만"
        {
            DX *= -1;
            X = 0.1f;
        }

        else if (X > 58 && DX > 0) // 59보다 크면서 "오른쪽으로 가고 있을 때만"
        {
            DX *= -1;
            X = 57.9f;
        }
        if (Y < 1 && DY < 0) // 위로 가다 천장에 닿으면
        {
            DY *= -1;
            Y = 0.1f;
        }
        else if (Y > 24 && DY > 0) // 아래로 가다 바닥에 닿으면
        {
            DY *= -1;
            Y = 23.9f;
        }
    }
}