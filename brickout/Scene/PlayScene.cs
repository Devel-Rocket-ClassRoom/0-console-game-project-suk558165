using System;
using Framework.Engine;
using System.Linq;

public class PlayScene : Scene
{
    // ── 씬 전환 관리자 ──
    // 게임오버/클리어/타이틀 전환 시 사용
    private SceneManager<Scene> sceneManager;

    // ── 스테이지 관리자 ──
    // 스테이지별 벽돌 배치, 공 속도, 특수 벽돌 비율 정보를 제공
    private StageManager _stageManager = new StageManager();

    private int _currenStage = 1;  // 현재 스테이지 번호 (내부 인덱스용)
    private int _stagecount  = 1;  // 화면 표시용 스테이지 번호 (동일하게 유지)

    private List<Brick>? _bricks;  // 현재 스테이지의 벽돌 목록
                                   // null: 아직 Load() 전 / Load() 후 초기화됨

    private int _lives = 5;        // 남은 목숨 수 (0이 되면 GameOverScene으로 전환)

    private Ball?   _ball;         // 메인 공 오브젝트 (바닥 낙하 감지 및 AutoAim 설정용)
    private Paddle? _paddle;       // 패들 오브젝트 (공/아이템 생성 시 참조)

    // ── 폭탄 벽돌 폭발 연출 목록 ──
    // BombBrick은 IsActive=false가 된 후에도 폭발 연출을 재생해야 함
    // → UpdateGameObjects/DrawGameObjects 대신 여기서 직접 Update/Draw 호출
    private List<BombBrick> _explodingBombs = new List<BombBrick>();

    // ── 멀티볼 목록 ──
    // AddBall() 아이템으로 추가된 공들 — 바닥 낙하 감지 및 제거용
    // _ball(메인 공)은 별도 관리, 멀티볼만 여기에 추가
    private List<Ball> _balls = new List<Ball>();

    // ── 패들 슬로우 타이머 ──
    // SlowPaddle() 호출 시 2초 카운트다운 → 만료 시 원래 속도로 복원
    private float _slowTimer = 0f;

    // ── 하단 벽 오브젝트 ──
    // wall 아이템 발동 시 생성, 5초 후 AddItem에서 RemoveWall() 호출로 제거
    // null이면 벽 없음
    private GameObject? _bottomWall = null;

    private Random _random = new Random(); // 아이템 종류 랜덤 결정용

    // ── 아이템 동시 존재 수 제한 ──
    // 화면에 너무 많은 아이템이 동시에 떨어지면 게임 밸런스 붕괴
    // _itemCount가 _maxItems에 도달하면 새 아이템 드랍 차단
    private int _itemCount = 0;
    private const int _maxItems = 3;

    public PlayScene(SceneManager<Scene> sceneManager)
    {
        this.sceneManager = sceneManager;
    }

    // ── multiball 아이템 효과 ──
    // 새 공을 생성하고 즉시 발사 — 현재 스테이지 속도 동일하게 적용
    public void AddBall()
    {
        if (_paddle == null || _bricks == null) return;
        var newBall = new Ball(this, _paddle, _bricks);
        var stage = _stageManager.GetStage(_currenStage);
        newBall.SetInterval(stage.BallInterval); // 메인 공과 동일한 속도
        newBall.Launch();                        // 대기 없이 즉시 발사
        _balls.Add(newBall);
        AddGameObject(newBall);
    }

    // ── slow 아이템 효과 ──
    // 패들 속도를 2초간 절반 이하로 줄여 조작을 어렵게 함
    public void SlowPaddle()
    {
        if (_paddle == null) return;
        _paddle.Speed = 10f; // 기본 30f → 10f로 감소
        _slowTimer = 2f;     // 2초 후 Update에서 30f로 복원
    }

    // ── life 아이템 효과 ──
    public void AddLife() => _lives++;

    // ── wall 아이템 효과: 하단 벽 생성 ──
    // 이미 벽이 있으면 중복 생성 방지
    // Ball.BottomWallActive = true → Ball.cs에서 Y=23에서 공을 위로 튕겨냄
    public void AddWall()
    {
        if (_bottomWall != null) return;
        _bottomWall = new Wall(this, 1, 23, 58, 1);
        AddGameObject(_bottomWall);
        Ball.BottomWallActive = true;
    }

    // ── wall 아이템 만료 처리 ──
    // AddItem의 5초 타이머 만료 시 호출
    // 벽 오브젝트 제거 + Ball의 하단 벽 충돌 판정 해제
    public void RemoveWall()
    {
        if (_bottomWall == null) return;
        RemoveGameObject(_bottomWall);
        _bottomWall = null;
        Ball.BottomWallActive = false;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        DrawGameObjects(buffer); // 등록된 모든 게임 오브젝트 Draw 호출

        // ── HUD 표시 ──
        buffer.WriteText(52, 0, $"Lives: {_lives}", ConsoleColor.Yellow);
        buffer.WriteText(1,  0, $"Stage: {_stagecount}", ConsoleColor.Yellow);

        // 현재 스테이지 속도 레벨을 별(★/☆)로 시각화
        // StageData.BallSpeedLevel: 1(느림) ~ 5(빠름)
        var stage = _stageManager.GetStage(_currenStage);
        string speedStars = new string('★', stage.BallSpeedLevel)
                          + new string('☆', 5 - stage.BallSpeedLevel);
        buffer.WriteText(20, 0, $"SPD {speedStars}", ConsoleColor.Cyan);

        // ── 폭발 연출 별도 렌더링 ──
        // BombBrick은 IsActive=false 이후에도 폭발 이펙트를 그려야 함
        // DrawGameObjects에서는 IsActive=false인 오브젝트를 건너뛰므로 직접 호출
        foreach (var bomb in _explodingBombs)
            bomb.DrawExplode(buffer);
    }

    public override void Load()
    {
        // ── 상태 초기화 ──
        // 스테이지 전환/재시작 시 이전 오브젝트 참조를 모두 해제
        _bricks     = null;
        _ball       = null;
        _paddle     = null;
        _bottomWall = null;
        _balls.Clear();
        _explodingBombs.Clear();
        _itemCount = 0;

        // ── static 상태 초기화 ──
        // Brick.CurrentStage: 아이템 드랍 스테이지 판정에 사용
        // Ball.BottomWallActive: 씬 재로드 시 하단 벽 판정 초기화
        Brick.CurrentStage    = _currenStage;
        Ball.BottomWallActive = false;

        // ── 게임 영역 외곽 벽 생성 ──
        AddGameObject(new Wall(this, 0, 0, 60, 25));

        // ── 패들 생성 ──
        _paddle = new Paddle(this);
        AddGameObject(_paddle);

        // ── 벽돌 생성 ──
        var stage = _stageManager.GetStage(_currenStage);
        _bricks = new List<Brick>();

        foreach (var (x, y, type) in stage.BrickPositions)
        {
            // 타입 문자열에 따라 적절한 벽돌 클래스 인스턴스 생성
            Brick brick = type switch
            {
                "hard"       => new HardBrick(this, x, y),
                "bomb"       => new BombBrick(this, x, y, _bricks), // 폭발 범위 체크를 위해 전체 목록 전달
                "invincible" => new InvincibleBrick(this, x, y),
                _            => new Brick(this, x, y)               // "normal" 또는 알 수 없는 타입
            };

            // ── 아이템 드랍 콜백 등록 ──
            // Brick.Hit() 내부에서 스테이지/확률 판정을 이미 통과한 경우에만 OnHit 호출됨
            // 여기선 동시 아이템 수(_maxItems)만 추가로 체크
            float bx = brick.X; // 람다 캡처용 로컬 변수 (반복문 변수 캡처 오류 방지)
            float by = brick.Y;
            brick.OnHit = () =>
            {
                if (_itemCount >= _maxItems) return; // 동시 최대치 초과 → 드랍 안 함

                _itemCount++;

                // 아이템 종류 랜덤 결정 (4종류 균등 확률)
                string itemType = _random.Next(4) switch
                {
                    0 => "multiball",
                    1 => "slow",
                    2 => "life",
                    3 => "wall",
                    _ => "life"
                };

                var item = new AddItem(this, bx, by, itemType, this, _paddle!);
                // 아이템 소멸 시 _itemCount 감소 → 새 아이템 드랍 허용
                item.OnDeactivate = () => _itemCount--;
                AddGameObject(item);
            };

            // ── 폭탄 벽돌 폭발 콜백 등록 ──
            // 폭발 시 _explodingBombs에 추가 → Update/Draw를 직접 관리
            if (brick is BombBrick bomb)
                bomb.OnExplode = () => _explodingBombs.Add(bomb);

            _bricks.Add(brick);
            AddGameObject(brick);
        }

        // ── 메인 공 생성 ──
        _ball = new Ball(this, _paddle, _bricks);
        _ball.SetInterval(stage.BallInterval); // 스테이지별 속도 적용
        AddGameObject(_ball);
    }

    public override void Update(float deltaTime)
    {
        // ── 패들 슬로우 타이머 ──
        if (_slowTimer > 0 && _paddle != null)
        {
            _slowTimer -= deltaTime;
            if (_slowTimer <= 0)
                _paddle.Speed = 30f; // 2초 경과 → 원래 속도 복원
        }

        UpdateGameObjects(deltaTime); // 모든 활성 오브젝트 Update 호출

        // ── 폭탄 벽돌 폭발 연출 업데이트 ──
        // IsActive=false 상태에서도 타이머를 직접 관리
        for (int i = _explodingBombs.Count - 1; i >= 0; i--)
        {
            _explodingBombs[i].UpdateExplode(deltaTime);
            if (!_explodingBombs[i].IsExploding)
                _explodingBombs.RemoveAt(i); // 연출 종료 → 목록에서 제거
        }

        if (_ball == null || _bricks == null || _paddle == null) return;

        // ── 마지막 벽돌 AutoAim 활성화 ──
        // 파괴 가능한 벽돌(InvincibleBrick 제외)이 1개 이하로 남으면 자동 조준 ON
        int remaining = _bricks.Count(b => b.IsActive && b is not InvincibleBrick);
        _ball.AutoAim = remaining <= 1;
        foreach (var b in _balls) b.AutoAim = remaining <= 1;

        // ── 메인 공 바닥 낙하 처리 ──
        // Ball.cs에서 wall 아이템 활성화 시 Y=23에서 튕겨내므로
        // Y>24가 되려면 wall이 없는 상태여야 함
        if (_ball.Y > 24)
        {
            _lives--;
            _ball.IsActive = false;
            RemoveGameObject(_ball);

            // 공이 떨어지면 하단 벽도 초기화 (다음 공에 깨끗한 상태 보장)
            if (_bottomWall != null)
                RemoveWall();

            if (_lives <= 0)
                sceneManager.ChangeScene(new GameOverScene(sceneManager));
            else
            {
                // 목숨 남아있으면 새 공 생성 (패들 위에서 대기 상태)
                _ball = new Ball(this, _paddle, _bricks);
                _ball.SetInterval(_stageManager.GetStage(_currenStage).BallInterval);
                AddGameObject(_ball);
            }
        }

        // ── 멀티볼 바닥 낙하 처리 ──
        // 멀티볼은 바닥에 떨어져도 목숨 감소 없이 제거만 함
        // 메인 공(_ball)만 목숨 감소 대상
        for (int i = _balls.Count - 1; i >= 0; i--)
        {
            if (_balls[i].Y > 24)
            {
                RemoveGameObject(_balls[i]);
                _balls.RemoveAt(i);
            }
        }

        // ── 스테이지 클리어 판정 ──
        // InvincibleBrick은 깰 수 없으므로 클리어 조건에서 제외
        // 모든 일반/특수 벽돌이 IsActive=false이면 클리어
        if (_bricks?.All(b => !b.IsActive || b is InvincibleBrick) == true)
        {
            if (_currenStage >= _stageManager.GetTotalStages())
                sceneManager.ChangeScene(new ClearScene(sceneManager)); // 마지막 스테이지 → 클리어
            else
            {
                // 다음 스테이지로 전환
                _currenStage++;
                _stagecount++;
                ClearGameObjects(); // 현재 스테이지 오브젝트 모두 제거
                Load();             // 다음 스테이지 로드
            }
        }
    }

    public override void Unload()
    {
        // 씬 종료 시 static 상태 초기화
        // 다른 씬으로 전환 후 PlayScene을 새로 시작할 때 오염 방지
        Ball.BottomWallActive = false;
        ClearGameObjects();
    }
}
