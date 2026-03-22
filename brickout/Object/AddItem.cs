using Framework.Engine;

// ════════════════════════════════════════════════════════════
// 아이템 오브젝트 — 벽돌 파괴 시 생성되어 아래로 낙하
// 패들로 수거하면 종류에 따른 효과 발동
//
// 아이템 종류:
//   multiball — 공 하나 추가 (Launch() 상태로 즉시 발사)
//   slow      — 패들 속도 2초간 감소
//   life      — 목숨 +1
//   wall      — 하단 벽 생성 (5초 후 자동 제거)
//
// wall 아이템의 특수 동작:
//   패들로 수거 → 벽 생성 → 5초 타이머 시작 → 만료 시 RemoveWall() 호출
//   이 동안 AddItem 오브젝트는 IsActive=true 상태 유지 (타이머 관리를 위해)
//   타이머 만료 후 Deactivate()로 완전 소멸
// ════════════════════════════════════════════════════════════
public class AddItem(Scene scene, float x, float y, string type, PlayScene playScene, Paddle paddle) : GameObject(scene)
{
    private float _x = x;             // 현재 X 위치
    private float _y = y;             // 현재 Y 위치 (낙하 중 증가)
    private string _type = type;      // 아이템 종류 문자열
    private PlayScene _playScene = playScene; // 효과 발동 시 PlayScene 메서드 호출용
    private Paddle _paddle = paddle;  // 패들 충돌 체크용

    private float _fallTimer    = 0f;   // 낙하 간격 누적 타이머
    private float _fallInterval = 0.1f; // 낙하 속도 — 0.1초마다 1칸씩 내려감

    // ── wall 아이템 전용 수명 타이머 ──
    private float _wallLifeTimer = 0f;    // 벽 생성 후 경과 시간
    private bool _wallActive = false;     // 벽이 현재 활성화 중인지
    private const float WallLifetime = 5f; // 벽 유지 시간 (초)

    // ── 중복 소멸 방지 플래그 ──
    // IsActive=false 설정 + OnDeactivate 호출이 여러 경로에서 중복 실행되면
    // PlayScene의 _itemCount가 음수로 내려가는 버그 발생
    // → _deactivated = true이면 Deactivate()를 즉시 리턴
    private bool _deactivated = false;

    public float X => _x;
    public float Y => _y;

    // PlayScene에서 등록 — 아이템 소멸 시 _itemCount-- 처리
    public Action? OnDeactivate;

    public override void Draw(ScreenBuffer buffer)
    {
        // wall 아이템이 발동된 후에는 아이템 심볼을 표시하지 않음
        // (벽 오브젝트 자체가 화면에 표시되므로 중복 표시 방지)
        if (_wallActive) return;

        // 아이템 종류별 다른 심볼과 색상으로 표시
        (string text, ConsoleColor color) = _type switch
        {
            "multiball" => ("★", ConsoleColor.Cyan),   // 공 추가
            "slow"      => ("↓", ConsoleColor.Blue),   // 패들 속도 감소
            "life"      => ("♥", ConsoleColor.Red),    // 목숨 추가
            "wall"      => ("■", ConsoleColor.Green),  // 하단 벽 생성
            _           => ("?", ConsoleColor.White)   // 알 수 없는 타입
        };
        buffer.WriteText((int)_x, (int)_y, text, color);
    }

    public override void Update(float deltaTime)
    {
        // ── wall 아이템 수명 타이머 ──
        // 벽이 활성화된 후 WallLifetime(5초)이 지나면 제거
        if (_wallActive)
        {
            _wallLifeTimer += deltaTime;
            if (_wallLifeTimer >= WallLifetime)
            {
                _wallActive = false;
                _playScene.RemoveWall(); // PlayScene에서 벽 오브젝트 제거 + Ball.BottomWallActive = false
                Deactivate();            // 아이템 오브젝트 자체도 소멸
            }
            return; // 타이머 카운트 중에는 낙하·충돌 처리 생략
        }

        // ── 낙하 처리 ──
        // _fallInterval마다 1칸씩 아래로 이동
        _fallTimer += deltaTime;
        if (_fallTimer >= _fallInterval)
        {
            _fallTimer -= _fallInterval;
            _y++;
        }

        // ── 패들 충돌 체크 ──
        // X: 패들 가로 범위 안 / Y: 패들 Y와 같은 행
        if (_x >= _paddle.X && _x <= _paddle.X + _paddle.Width
            && (int)_y == (int)_paddle.Y)
        {
            Apply(); // 효과 발동

            if (_type != "wall")
                Deactivate(); // wall 이외 아이템은 즉시 소멸
            // wall 아이템은 _wallActive = true가 되어 타이머로 소멸 관리
            return;
        }

        // ── 바닥 이탈 처리 ──
        // 패들로 수거하지 못하고 바닥 아래로 떨어지면 소멸
        if (_y > 24)
            Deactivate();
    }

    // 아이템 종류에 따라 PlayScene의 해당 메서드 호출
    private void Apply()
    {
        switch (_type)
        {
            case "multiball":
                _playScene.AddBall();    // 공 하나 추가 (Launch 상태로 즉시 발사)
                break;
            case "slow":
                _playScene.SlowPaddle(); // 패들 속도 2초간 감소
                break;
            case "life":
                _playScene.AddLife();    // 목숨 +1
                break;
            case "wall":
                _playScene.AddWall();    // 하단 벽 생성 + Ball.BottomWallActive = true
                _wallActive = true;      // 수명 타이머 시작
                _wallLifeTimer = 0f;
                break;
        }
    }

    // 아이템 소멸 처리 — 모든 소멸 경로의 단일 진입점
    // _deactivated 플래그로 중복 호출 완전 차단
    private void Deactivate()
    {
        if (_deactivated) return; // 이미 소멸 처리됨 → 즉시 리턴
        _deactivated = true;
        IsActive = false;         // 오브젝트 비활성화 → 다음 프레임부터 Update/Draw 제외
        OnDeactivate?.Invoke();   // PlayScene의 _itemCount-- 처리
    }
}
