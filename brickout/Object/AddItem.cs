using Framework.Engine;

public class AddItem : GameObject
{
    private float _x;
    private float _y;
    private string _type;
    private PlayScene _playScene;
    private Paddle _paddle;
    private float _fallTimer = 0f;
    private float _fallInterval = 0.1f; // 낙하 속도

    public float X => _x;
    public float Y => _y;

    public AddItem(Scene scene, float x, float y, string type, PlayScene playScene, Paddle paddle) : base(scene)
    {
        _x = x;
        _y = y;
        _type = type;
        _playScene = playScene;
        _paddle = paddle;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        (string text, ConsoleColor color) = _type switch
        {
            "multiball" => ("★", ConsoleColor.Cyan),
            "slow" => ("↓", ConsoleColor.Blue),
            "life" => ("♥", ConsoleColor.Red),
            "wall" => ("■", ConsoleColor.Green),
            _ => ("?", ConsoleColor.White)
        };
        buffer.WriteText((int)_x, (int)_y, text, color);
    }

    public override void Update(float deltaTime)
    {
        // 낙하
        _fallTimer += deltaTime;
        if (_fallTimer >= _fallInterval)
        {
            _fallTimer -= _fallInterval;
            _y++;
        }

        // 패들 충돌 체크
        if (_x >= _paddle.X && _x <= _paddle.X + _paddle.Width
            && (int)_y == (int)_paddle.Y)
        {
            Apply();
            IsActive = false;
        }

        // 바닥에 떨어지면 소멸
        if (_y > 24) IsActive = false;
    }

    private void Apply()
    {
        switch (_type)
        {
            case "multiball": _playScene.AddBall(); break;
            case "slow": _playScene.SlowPaddle(); break;
            case "life": _playScene.AddLife(); break;
            case "wall": _playScene.AddWall(); break;
        }
    }
}