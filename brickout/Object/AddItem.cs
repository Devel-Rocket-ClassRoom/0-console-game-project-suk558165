using Framework.Engine;

public class AddItem(Scene scene, float x, float y, string type, PlayScene playScene, Paddle paddle) : GameObject(scene)
{
    private float _x = x;
    private float _y = y;
    private string _type = type;
    private PlayScene _playScene = playScene;
    private Paddle _paddle = paddle;
    private float _fallTimer = 0f;
    private float _fallInterval = 0.1f;

    // wall 아이템 전용 수명 타이머
    private float _wallLifeTimer = 0f;
    private bool _wallActive = false;
    private const float WallLifetime = 5f;

    // Fix2: 중복 호출 방지용 플래그
    private bool _deactivated = false;

    public float X => _x;
    public float Y => _y;

    public Action? OnDeactivate;

    public override void Draw(ScreenBuffer buffer)
    {
        // wall 아이템은 발동 후 화면에 표시하지 않음 (벽 자체가 표시됨)
        if (_wallActive) return;

        (string text, ConsoleColor color) = _type switch
        {
            "multiball" => ("★", ConsoleColor.Cyan),
            "slow"      => ("↓", ConsoleColor.Blue),
            "life"      => ("♥", ConsoleColor.Red),
            "wall"      => ("■", ConsoleColor.Green),
            _           => ("?", ConsoleColor.White)
        };
        buffer.WriteText((int)_x, (int)_y, text, color);
    }

    public override void Update(float deltaTime)
    {
        // wall 아이템 수명 타이머
        if (_wallActive)
        {
            _wallLifeTimer += deltaTime;
            if (_wallLifeTimer >= WallLifetime)
            {
                _wallActive = false;
                _playScene.RemoveWall();
                Deactivate(); // Fix2: 단일 진입점으로 중복 방지
            }
            return;
        }

        // 낙하
        _fallTimer += deltaTime;
        if (_fallTimer >= _fallInterval)
        {
            _fallTimer -= _fallInterval;
            _y++;
        }

        // 패들 충돌
        if (_x >= _paddle.X && _x <= _paddle.X + _paddle.Width
            && (int)_y == (int)_paddle.Y)
        {
            Apply();
            if (_type != "wall")
                Deactivate(); // Fix2: 단일 진입점
            // wall은 _wallActive = true 후 타이머로 소멸
            return;
        }

        // 바닥 이탈
        if (_y > 24)
            Deactivate(); // Fix2: 단일 진입점
    }

    private void Apply()
    {
        switch (_type)
        {
            case "multiball": _playScene.AddBall();    break;
            case "slow":      _playScene.SlowPaddle(); break;
            case "life":      _playScene.AddLife();    break;
            case "wall":
                _playScene.AddWall();
                _wallActive = true;
                _wallLifeTimer = 0f;
                break;
        }
    }

    // Fix2: _deactivated 플래그로 OnDeactivate 중복 호출 완전 차단
    private void Deactivate()
    {
        if (_deactivated) return;
        _deactivated = true;
        IsActive = false;
        OnDeactivate?.Invoke();
    }
}
