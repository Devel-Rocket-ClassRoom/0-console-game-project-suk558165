using Framework.Engine;


public class AddItem(Scene scene, float x, float y, string type, PlayScene playScene, Paddle paddle) : GameObject(scene)
{
    private float _x = x;
    private float _y = y;
    private string _type = type;              // 아이템 종류 (multiball, slow, life, wall)
    private PlayScene _playScene = playScene; // 효과 발동 시 PlayScene 메서드 호출용
    private Paddle _paddle = paddle;          // 패들 충돌 체크용
    private float _fallTimer = 0f;
    private float _fallInterval = 0.1f;       // 낙하 속도 (작을수록 빠름)

    // ── wall 아이템 전용: 벽이 생성된 뒤 5초 후 자동 제거 ──
    private float _wallLifeTimer = 0f;        // 경과 시간
    private bool _wallActive = false;         // 벽이 현재 활성화 중인지
    private const float WallLifetime = 5f;    // 벽 유지 시간 (초)

    public float X => _x;
    public float Y => _y;

    public Action? OnDeactivate; // 소멸 시 호출 (PlayScene에서 리스트 정리용)

    public override void Draw(ScreenBuffer buffer)
    {
        // 아이템 종류별 다른 문자와 색상으로 표시
        (string text, ConsoleColor color) = _type switch
        {
            "multiball" => ("★", ConsoleColor.Cyan),   // 공 추가
            "slow"      => ("↓", ConsoleColor.Blue),   // 패들 느리게
            "life"      => ("♥", ConsoleColor.Red),    // 목숨 추가
            "wall"      => ("■", ConsoleColor.Green),  // 하단 벽 생성
            _           => ("?", ConsoleColor.White)
        };
        buffer.WriteText((int)_x, (int)_y, text, color);
    }

    public override void Update(float deltaTime)
    {
        // ── wall 아이템이 발동된 뒤 타이머 카운트 ──
        if (_wallActive)
        {
            _wallLifeTimer += deltaTime;
            if (_wallLifeTimer >= WallLifetime)
            {
                _wallActive = false;
                _playScene.RemoveWall(); // PlayScene에서 하단 벽 제거
                Deactivate();
            }
            return; // 벽 대기 중에는 낙하·충돌 업데이트 생략
        }

        // ── 낙하 처리 ──
        _fallTimer += deltaTime;
        if (_fallTimer >= _fallInterval)
        {
            _fallTimer -= _fallInterval;
            _y++;
        }

        // ── 패들 범위 안에 들어오면 효과 발동 ──
        if (_x >= _paddle.X && _x <= _paddle.X + _paddle.Width
            && (int)_y == (int)_paddle.Y)
        {
            Apply();
            if (_type != "wall")
            {
                IsActive = false;
                OnDeactivate?.Invoke();
            }
            // wall 아이템은 벽 수명 타이머 시작 후 Update 계속 실행
        }

        // ── 바닥을 벗어나면 소멸 ──
        if (_y > 24)
        {
            IsActive = false;
            OnDeactivate?.Invoke();
        }
    }

    // 아이템 종류에 따라 PlayScene의 해당 메서드 호출
    private void Apply()
    {
        switch (_type)
        {
            case "multiball": _playScene.AddBall();    break; // 공 추가
            case "slow":      _playScene.SlowPaddle(); break; // 패들 느리게
            case "life":      _playScene.AddLife();    break; // 목숨 +1
            case "wall":
                _playScene.AddWall();   // 하단 벽 생성
                _wallActive = true;     // 타이머 시작
                _wallLifeTimer = 0f;
                break;
        }
    }

    private void Deactivate()
    {
        IsActive = false;
        OnDeactivate?.Invoke();
    }
}
