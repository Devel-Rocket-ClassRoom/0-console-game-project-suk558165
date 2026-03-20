using Framework.Engine;


public class AddItem(Scene scene, float x, float y, string type, PlayScene playScene, Paddle paddle) : GameObject(scene)
{
    private float _x = x;
    private float _y = y;
    private string _type = type;         // 아이템 종류 (multiball, slow, life, wall)
    private PlayScene _playScene = playScene; // 효과 발동 시 PlayScene 메서드 호출용
    private Paddle _paddle = paddle;       // 패들 충돌 체크용
    private float _fallTimer = 0f;
    private float _fallInterval = 0.1f; // 낙하 속도 (작을수록 빠름)

    public float X => _x;
    public float Y => _y;

    public Action? OnDeactivate; // 소멸 시 호출

    // IsActive = false 할 때마다 호출
    private void Deactivate()
    {
        Deactivate();
        OnDeactivate?.Invoke();
    }
    public override void Draw(ScreenBuffer buffer)
    {
        // 아이템 종류별 다른 문자와 색상으로 표시
        (string text, ConsoleColor color) = _type switch
        {
            "multiball" => ("★", ConsoleColor.Cyan),   // 공 추가
            "slow" => ("↓", ConsoleColor.Blue),   // 패들 느리게
            "life" => ("♥", ConsoleColor.Red),    // 목숨 추가
            "wall" => ("■", ConsoleColor.Green),  // 하단 벽 생성
            _ => ("?", ConsoleColor.White)
        };
        buffer.WriteText((int)_x, (int)_y, text, color);
    }

    public override void Update(float deltaTime)
    {
        // 일정 간격으로 한 칸씩 낙하
        _fallTimer += deltaTime;
        if (_fallTimer >= _fallInterval)
        {
            _fallTimer -= _fallInterval;
            _y++;
        }

        // 패들 범위 안에 들어오면 효과 발동
        if (_x >= _paddle.X && _x <= _paddle.X + _paddle.Width
            && (int)_y == (int)_paddle.Y)
        {
            Apply();
            IsActive = false;
        }

        // 바닥을 벗어나면 소멸
        if (_y > 24) IsActive = false;
    }

    // 아이템 종류에 따라 PlayScene의 해당 메서드 호출
    private void Apply()
    {
        switch (_type)
        {
            case "multiball": _playScene.AddBall(); break;    // 공 추가
            case "slow": _playScene.SlowPaddle(); break; // 패들 느리게
            case "life": _playScene.AddLife(); break;    // 목숨 +1
            case "wall": _playScene.AddWall(); break;    // 하단 벽 생성
        }
    }
}