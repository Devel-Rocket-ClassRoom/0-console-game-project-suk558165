using Framework.Engine;
using System.Linq;

public class Ball : GameObject
{
    // ── 공의 현재 위치 (float — 서브픽셀 정밀도로 보정값 누적 방지) ──
    public float X { get; set; }
    public float Y { get; set; }

    // ── 이동 방향 벡터 ──
    // 콘솔은 정수 좌표 기반이므로 DX/DY는 항상 -1, 0, 1 중 하나만 사용
    // float 벡터(0.6 등)를 쓰면 렌더링(정수 반올림)과 충돌(float) 위치가 달라져 흔들림 발생
    public float DX { get; set; } = 0f;   // X 방향: -1(왼쪽), 0(수직), 1(오른쪽)
    public float DY { get; set; } = 1f;   // Y 방향: 1(아래, 시작 상태), -1(위)

    private bool _Waiting = true;          // 스페이스바 입력 대기 중 여부
    private float _moveTimer    = 0f;      // 이동 간격 누적 타이머
    private float _moveInterval = 0.08f;   // 프레임당 이동 간격(초) — SetInterval()로 스테이지마다 조정

    private Paddle paddle;                 // 패들 참조 (충돌 판정용)
    private List<Brick> bricks;            // 벽돌 목록 참조 (충돌 판정용)

    // ── 하단 벽(wall 아이템) 관련 ──
    // PlayScene에서 wall 아이템 발동 시 true로 설정
    // true이면 BottomWallY 라인에서 공을 위로 튕겨냄
    public static bool BottomWallActive { get; set; } = false;
    private const float BottomWallY = 23f; // 하단 벽 Y 좌표 (Wall 오브젝트의 Y와 일치해야 함)

    // ── 벽돌 크기 상수 ──
    // Draw에서 "■■"(2칸 너비, 1칸 높이)로 그리므로 충돌 판정도 동일하게 맞춤
    private const float BrickW = 2f;
    private const float BrickH = 1f;

    // ── 충돌 판정 여유(Margin) ──
    // 공 중심에서 이 값만큼 이내로 벽돌 경계에 접근하면 충돌로 인정
    // 0.45f: 콘솔 1칸 단위에서 모서리 통과 없이 판정되는 최적값
    //   - 너무 작으면(< 0.3): 모서리를 통과하는 터널링 발생
    //   - 너무 크면(> 0.5):   인접 벽돌에 닿지 않았는데 판정되는 오작동 발생
    private const float Margin = 0.45f;

    // ── 축별 충돌 쿨다운 ──
    // 충돌 직후 같은 축에서 재판정되어 방향이 다시 뒤집히는 흔들림 방지
    // X충돌, Y충돌, 패들충돌 각각 독립적으로 관리
    // 쿨다운 > 0인 동안 해당 축 판정을 완전히 건너뜀
    private int _xCD = 0;   // 벽돌 X(좌/우면) 충돌 쿨다운
    private int _yCD = 0;   // 벽돌 Y(위/아래면) 충돌 쿨다운
    private int _pCD = 0;   // 패들 충돌 쿨다운
    private const int CD = 4; // 쿨다운 프레임 수 — 4프레임이면 공이 경계에서 확실히 벗어남

    // ── 마지막 벽돌 자동 조준 ──
    // PlayScene에서 남은 파괴 가능 벽돌이 1개 이하일 때 true로 설정
    // 일정 주기마다 DX를 마지막 벽돌 방향으로 보정해서 플레이어 부담을 줄여줌
    public bool AutoAim { get; set; } = false;
    private float _autoAimTimer = 0f;
    private const float AutoAimInterval = 1.5f; // 보정 주기(초)

    // ── 패들 반사 후 DX 예약 ──
    // 패들 판정은 Y이동 후에 실행되므로 DX를 즉시 바꿔도 이미 X이동은 끝난 상태
    // 하지만 명확한 의도 표현을 위해 예약 변수로 관리
    // null이면 예약 없음, 값이 있으면 다음 프레임 시작 시 DX에 적용
    private float? _pendingDX = null;

    public Ball(Scene scene, Paddle paddle, List<Brick> bricks) : base(scene)
    {
        this.paddle = paddle;
        this.bricks = bricks;
        // 패들 중앙 바로 위에서 시작 — 자연스러운 발사 준비 위치
        X = paddle.X + paddle.Width / 2f;
        Y = paddle.Y - 1f;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // Math.Round: float 위치를 가장 가까운 정수 셀에 그림
        // (int)캐스팅은 버림이라 0.9 → 0이 되는 오차 발생하므로 Round 사용
        buffer.WriteText((int)Math.Round(X), (int)Math.Round(Y), "●", ConsoleColor.Blue);
    }

    public override void Update(float deltaTime)
    {
        // 스페이스바 입력 대기 — 입력 전까지는 공이 패들 위에 고정
        if (_Waiting)
        {
            if (Input.IsKeyDown(ConsoleKey.Spacebar))
                _Waiting = false;
            return;
        }

        // ── 이동 간격 타이머 ──
        // deltaTime 누적 → _moveInterval마다 한 칸 이동
        // 이렇게 하면 프레임레이트와 무관하게 일정 속도 유지
        _moveTimer += deltaTime;
        if (_moveTimer < _moveInterval) return;
        _moveTimer -= _moveInterval; // 초과분은 다음 프레임으로 이월 (누락 없이 정확)

        // ── 예약된 DX 적용 ──
        if (_pendingDX.HasValue)
        {
            DX = _pendingDX.Value;
            _pendingDX = null;
        }

        // ── 쿨다운 차감 ──
        // 이동·충돌 처리 전에 먼저 차감
        // → 충돌 발생 프레임에 CD값이 세팅되고, 다음 프레임부터 CD-1, CD-2... 로 줄어듦
        // → 이동 후에 차감하면 보호 프레임이 1 짧아지는 타이밍 문제 발생
        if (_xCD > 0) _xCD--;
        if (_yCD > 0) _yCD--;
        if (_pCD > 0) _pCD--;

        float prevX = X; // 이번 프레임 이동 전 X 위치 (충돌 판정 기준점)
        float prevY = Y; // 이번 프레임 이동 전 Y 위치

        // ── X 이동 → X축(좌/우면) 벽돌 충돌 ──
        // X만 먼저 이동하고 충돌 체크 → 이 시점에 Y는 아직 이동 전(Y == prevY)
        // Y가 고정돼 있으므로 "공이 벽돌 Y범위 안에 있는가" 체크가 정확
        X += DX;
        if (_xCD == 0) ResolveX(prevX, prevY);

        // ── Y 이동 → Y축(위/아래면) 벽돌 충돌 ──
        // Y 이동 후 충돌 체크 → 이 시점에 X는 이미 이동 완료 + 보정까지 끝남
        // ResolveX에서 보정된 현재 X를 기준으로 "X범위 안인가" 체크
        Y += DY;
        if (_yCD == 0) ResolveY(prevY);

        // ── 사이드 벽 충돌 ──
        // 절댓값 방식(Math.Abs) 대신 명시적 1f/-1f 대입 — float 오차 누적 방지
        if (X < 1f && DX < 0)       { DX =  1f; X = 1f; }
        else if (X > 58f && DX > 0) { DX = -1f; X = 58f; }

        // ── 천장 충돌 ──
        if (Y <= 1f && DY < 0)      { DY =  1f; Y = 1f; }

        // ── 하단 wall 아이템 충돌 ──
        // wall 아이템이 활성화된 경우에만 동작
        // 공이 Y=23 라인에 도달하면 위로 튕겨냄 (목숨 잃지 않고 한 번 더 기회)
        if (BottomWallActive && Y >= BottomWallY && DY > 0)
        {
            DY = -1f;
            Y  = BottomWallY - 1f; // 벽 바로 위로 보정
        }

        // ── 패들 충돌 ──
        // 조건: ① 아래로 이동 중(DY > 0)
        //       ② 공의 Y가 패들 윗면 범위 안 (paddle.Y-1 ~ paddle.Y)
        //       ③ 공의 X가 패들 가로 범위 안 (paddle.X ~ paddle.X + Width)
        //       ④ 패들 쿨다운이 0 (충돌 직후 재판정 방지)
        if (_pCD == 0 && DY > 0 &&
            Y  >= paddle.Y - 1f && Y <= paddle.Y &&
            X  >= paddle.X      && X <= paddle.X + paddle.Width)
        {
            // 위치 보정 — 패들 윗면 바로 위로 고정
            // 보정 없이 두면 다음 프레임에 Y가 패들 범위 안에 그대로 남아 재판정
            Y = paddle.Y - 1f;

            // DY는 무조건 위로 반사
            DY = -1f;

            // ── 반사 각도: 히트 위치로 DX 결정 ──
            // 패들을 3구역으로 나눠 각도 조절
            // [왼쪽 1/3] → DX = -1 (왼쪽으로)
            // [중앙 1/3] → DX = 기존 방향 유지 (수직에 가까운 느낌)
            // [오른쪽 1/3] → DX = +1 (오른쪽으로)
            float third = paddle.Width / 3f;
            float relX  = X - paddle.X; // 패들 왼쪽 끝 기준 상대 위치

            float newDX;
            if (relX < third)
                newDX = -1f;                    // 왼쪽 구역 → 왼쪽으로
            else if (relX > paddle.Width - third)
                newDX = 1f;                     // 오른쪽 구역 → 오른쪽으로
            else
                newDX = DX == 0f ? 1f : DX;    // 중앙 구역 → 기존 방향 유지
                                                // DX가 0(수직)이면 오른쪽으로 기본값

            // ── 패들 이동 방향 영향 ──
            // 플레이어가 패들을 움직이는 방향으로 공을 치면 해당 방향으로 강제
            // 예: 왼쪽으로 이동하며 오른쪽 구역을 맞히면 → 왼쪽으로 꺾임
            if (Input.IsKey(ConsoleKey.LeftArrow)  && newDX > 0f) newDX = -1f;
            if (Input.IsKey(ConsoleKey.RightArrow) && newDX < 0f) newDX =  1f;

            DX = newDX;
            _pCD = CD; // 패들 쿨다운 시작 — CD프레임 동안 재판정 차단
        }

        // ── 마지막 벽돌 자동 조준 ──
        // 남은 파괴 가능 벽돌이 1개일 때 PlayScene에서 AutoAim = true로 설정
        // AutoAimInterval마다 한 번씩 DX를 벽돌 방향으로 보정
        // 강제 조준이 아니라 "반대 방향일 때만 뒤집기"라 자연스럽고
        // 플레이어가 여전히 패들로 방향 조작 가능
        if (AutoAim)
        {
            _autoAimTimer += deltaTime;
            if (_autoAimTimer >= AutoAimInterval)
            {
                _autoAimTimer = 0f;
                // InvincibleBrick은 깰 수 없으므로 조준 대상에서 제외
                var target = bricks.FirstOrDefault(b => b.IsActive && b is not InvincibleBrick);
                if (target != null)
                {
                    float targetCenterX = target.X + BrickW / 2f;
                    // 거리 3칸 이상 벌어져 있고 방향이 반대일 때만 보정
                    // (너무 가까우면 보정 없이 자연스럽게 맞도록)
                    if (Math.Abs(X - targetCenterX) > 3f)
                    {
                        bool right = targetCenterX > X;
                        if (right  && DX < 0f) DX = 1f;  // 벽돌이 오른쪽인데 왼쪽으로 가면 → 오른쪽으로
                        if (!right && DX > 0f) DX = -1f; // 벽돌이 왼쪽인데 오른쪽으로 가면 → 왼쪽으로
                    }
                }
            }
        }
    }

    // 스페이스바로 발사 시 호출 (멀티볼 추가 시에도 사용)
    public void Launch()
    {
        _Waiting = false;
        DY = -1f; // 위로 발사 (패들 위에서 출발하므로 위로)
        DX =  0f; // 수직으로 시작
    }

    // PlayScene에서 스테이지 로드 시 호출 — 스테이지별 공 속도 적용
    // StageData.BallInterval 값을 그대로 넘기면 됨 (작을수록 빠름)
    public void SetInterval(float interval) => _moveInterval = interval;

    // ════════════════════════════════════════════════════════
    // X 이동 후 벽돌 좌/우면 충돌 처리
    //
    // 호출 시점: X += DX 직후, Y는 아직 이동 전
    // → Y == prevY이므로 prevY만으로 "Y범위 안에 있는가" 정확히 체크 가능
    //
    // prevOverlapX 체크: 이전 프레임에 이미 X범위 안이었다면
    //   → 이번 이동이 X방향 진입이 아님 (이미 겹쳐있던 것)
    //   → Y충돌로 처리해야 하므로 스킵
    // ════════════════════════════════════════════════════════
    private void ResolveX(float prevX, float prevY)
    {
        if (DX == 0f) return; // 수직 이동 중이면 X충돌 없음

        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue; // 이미 깨진 벽돌 스킵

            float bL = B.X;           // 벽돌 왼쪽 경계
            float bR = B.X + BrickW;  // 벽돌 오른쪽 경계
            float bT = B.Y;           // 벽돌 위쪽 경계
            float bB = B.Y + BrickH;  // 벽돌 아래쪽 경계

            // Y는 미이동 → prevY(== 현재 Y)로만 Y범위 체크
            if (prevY < bT - Margin || prevY > bB + Margin) continue;

            // 이전 프레임에 이미 X범위 안이었으면 → X방향 신규 진입 아님 → 스킵
            if (prevX >= bL - Margin && prevX <= bR + Margin) continue;

            if (DX > 0f && X + Margin >= bL && prevX + Margin < bL)
            {
                // 오른쪽으로 이동 중 → 벽돌 왼쪽 면에 충돌
                DX = -1f;                  // 왼쪽으로 반사
                X  = bL - Margin - 0.01f; // 벽돌 왼쪽 면 바로 앞으로 보정
                                           // 0.01f 추가 여유: 다음 프레임 판정 조건(prevX + Margin < bL)을 확실히 만족 못하게
                _xCD = CD;                 // X축 쿨다운 시작
                B.Hit();                   // 벽돌 피격 처리 (HP 감소, 아이템 드랍 등)
                return;                    // 한 프레임에 벽돌 하나만 처리
            }
            if (DX < 0f && X - Margin <= bR && prevX - Margin > bR)
            {
                // 왼쪽으로 이동 중 → 벽돌 오른쪽 면에 충돌
                DX = 1f;
                X  = bR + Margin + 0.01f;
                _xCD = CD;
                B.Hit();
                return;
            }
        }
    }

    // ════════════════════════════════════════════════════════
    // Y 이동 후 벽돌 위/아래면 충돌 처리
    //
    // 호출 시점: Y += DY 직후, X는 이미 이동 완료 + ResolveX 보정까지 끝남
    // → 현재 X(보정 후)만으로 "X범위 안에 있는가" 체크
    // → prevX를 같이 쓰면 ResolveX 보정 전 위치가 섞여 오판 발생
    //
    // prevOverlapY 체크: 이전 프레임에 이미 Y범위 안이었다면
    //   → 이번 이동이 Y방향 진입이 아님 (이미 겹쳐있던 것)
    //   → X충돌로 처리해야 하므로 스킵
    // ════════════════════════════════════════════════════════
    private void ResolveY(float prevY)
    {
        foreach (Brick B in bricks)
        {
            if (!B.IsActive) continue;

            float bL = B.X;
            float bR = B.X + BrickW;
            float bT = B.Y;
            float bB = B.Y + BrickH;

            // X는 이동 완료 → 현재 X(보정 후)만으로 X범위 체크
            if (X < bL - Margin || X > bR + Margin) continue;

            // 이전 프레임에 이미 Y범위 안이었으면 → Y방향 신규 진입 아님 → 스킵
            if (prevY >= bT - Margin && prevY <= bB + Margin) continue;

            if (DY > 0f && Y + Margin >= bT && prevY + Margin < bT)
            {
                // 아래로 이동 중 → 벽돌 위쪽 면에 충돌
                DY = -1f;                  // 위로 반사
                Y  = bT - Margin - 0.01f; // 벽돌 위쪽 면 바로 위로 보정
                _yCD = CD;
                B.Hit();
                return;
            }
            if (DY < 0f && Y - Margin <= bB && prevY - Margin > bB)
            {
                // 위로 이동 중 → 벽돌 아래쪽 면에 충돌
                DY = 1f;
                Y  = bB + Margin + 0.01f;
                _yCD = CD;
                B.Hit();
                return;
            }
        }
    }
}
