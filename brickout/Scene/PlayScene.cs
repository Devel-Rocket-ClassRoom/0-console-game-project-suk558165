using System;
using Framework.Engine;
using System.Linq;

public class PlayScene : Scene
{
    private SceneManager<Scene> sceneManager;
    private StageManager _stageManager = new StageManager();
    private int _currenStage = 1;
    private List<Brick>? _bricks;
    private int _lives = 3;
    private Ball? _ball;
    private Paddle? _paddle;
    private int _stagecount = 1;
    private List<BombBrick> _explodingBombs = new List<BombBrick>();
    private List<Ball> _balls = new List<Ball>();
    private float _slowTimer = 0f;
    private GameObject? _bottomWall = null;
    private Random _random = new Random();
    private int _itemCount = 0;
    private const int _maxItems = 3;       // 동시 최대 아이템 수 (5→3으로 감소)

    // ── 아이템 드랍 확률: 1스테이지 0%, 이후 8% ──
    // (Brick.cs의 ItemDropChance와 맞춰서 PlayScene의 OnHit 확률도 제거 — Brick 쪽에서 이미 판정)
    // OnHit 자체가 확률 판정 후에만 불리므로 여기선 그냥 생성만 함

    public PlayScene(SceneManager<Scene> sceneManager)
    {
        this.sceneManager = sceneManager;
    }

    public void AddBall()
    {
        if (_paddle == null || _bricks == null) return;
        var newBall = new Ball(this, _paddle, _bricks);
        var stage = _stageManager.GetStage(_currenStage);
        newBall.SetInterval(stage.BallInterval); // 멀티볼도 동일 속도
        newBall.Launch();
        _balls.Add(newBall);
        AddGameObject(newBall);
    }

    public void SlowPaddle()
    {
        if (_paddle == null) return;
        _paddle.Speed = 10f;
        _slowTimer = 2f;
    }

    public void AddLife()
    {
        _lives++;
    }

    // wall 아이템 생성 — 이미 벽이 있으면 무시
    public void AddWall()
    {
        if (_bottomWall != null) return;
        _bottomWall = new Wall(this, 1, 23, 58, 1);
        AddGameObject(_bottomWall);
        Ball.BottomWallActive = true; // 공이 하단 벽에서 튕기도록 활성화
    }

    // wall 아이템 만료(5초) 후 AddItem에서 호출 — 벽 제거
    public void RemoveWall()
    {
        if (_bottomWall == null) return;
        RemoveGameObject(_bottomWall);
        _bottomWall = null;
        Ball.BottomWallActive = false; // 공 하단 벽 판정 해제
    }

    public override void Draw(ScreenBuffer buffer)
    {
        DrawGameObjects(buffer);
        buffer.WriteText(52, 0, $"Lives: {_lives}", ConsoleColor.Yellow);
        buffer.WriteText(1, 0, $"Stage: {_stagecount}", ConsoleColor.Yellow);

        // 현재 속도 레벨 표시 (★ 개수)
        var stage = _stageManager.GetStage(_currenStage);
        string speedStars = new string('★', stage.BallSpeedLevel) +
                            new string('☆', 5 - stage.BallSpeedLevel);
        buffer.WriteText(20, 0, $"SPD {speedStars}", ConsoleColor.Cyan);

        foreach (var bomb in _explodingBombs)
            bomb.DrawExplode(buffer);
    }

    public override void Load()
    {
        _bricks = null;
        _ball = null;
        _paddle = null;
        _bottomWall = null;
        _balls.Clear();
        _explodingBombs.Clear();
        _itemCount = 0;

        // 스테이지 변경 시 정적 상태 초기화
        Brick.CurrentStage = _currenStage;
        Ball.BottomWallActive = false;

        AddGameObject(new Wall(this, 0, 0, 60, 25));

        _paddle = new Paddle(this);
        AddGameObject(_paddle);

        var stage = _stageManager.GetStage(_currenStage);
        _bricks = new List<Brick>();

        foreach (var (x, y, type) in stage.BrickPositions)
        {
            Brick brick = type switch
            {
                "hard"       => new HardBrick(this, x, y),
                "bomb"       => new BombBrick(this, x, y, _bricks),
                "invincible" => new InvincibleBrick(this, x, y),
                _            => new Brick(this, x, y)
            };

            float bx = brick.X;
            float by = brick.Y;

            // OnHit: Brick.Hit() 내부에서 이미 스테이지·확률 판정을 통과한 경우에만 호출됨
            // → 여기선 동시 아이템 개수(_maxItems)만 추가로 체크
            brick.OnHit = () =>
            {
                if (_itemCount >= _maxItems) return; // 동시 최대치 초과 시 드랍 안 함

                _itemCount++;
                string itemType = _random.Next(4) switch
                {
                    0 => "multiball",
                    1 => "slow",
                    2 => "life",
                    3 => "wall",
                    _ => "life"
                };
                var item = new AddItem(this, bx, by, itemType, this, _paddle!);
                item.OnDeactivate = () => _itemCount--;
                AddGameObject(item);
            };

            if (brick is BombBrick bomb)
                bomb.OnExplode = () => _explodingBombs.Add(bomb);

            _bricks.Add(brick);
            AddGameObject(brick);
        }

        _ball = new Ball(this, _paddle, _bricks);
        _ball.SetInterval(stage.BallInterval); // 스테이지별 공 속도 적용
        AddGameObject(_ball);
    }

    public override void Update(float deltaTime)
    {
        // 패들 슬로우 타이머
        if (_slowTimer > 0 && _paddle != null)
        {
            _slowTimer -= deltaTime;
            if (_slowTimer <= 0)
                _paddle.Speed = 30f;
        }

        UpdateGameObjects(deltaTime);

        // 폭탄 벽돌 폭발 연출 업데이트
        for (int i = _explodingBombs.Count - 1; i >= 0; i--)
        {
            _explodingBombs[i].UpdateExplode(deltaTime);
            if (!_explodingBombs[i].IsExploding)
                _explodingBombs.RemoveAt(i);
        }

        if (_ball == null || _bricks == null || _paddle == null) return;

        // ── 공이 바닥 아래로 떨어졌을 때 처리 ──
        // wall 아이템이 활성화된 경우 Ball.cs에서 튕겨내므로 여기까지 오지 않음
        if (_ball.Y > 24)
        {
            _lives--;
            _ball.IsActive = false;
            RemoveGameObject(_ball);

            // 공이 떨어지면 하단 벽도 함께 제거 (wall 아이템 효과 리셋)
            if (_bottomWall != null)
                RemoveWall();

            if (_lives <= 0)
                sceneManager.ChangeScene(new GameOverScene(sceneManager));
            else
            {
                _ball = new Ball(this, _paddle, _bricks);
                AddGameObject(_ball);
            }
        }

        // 멀티볼 처리 — 추가 공이 바닥 아래로 떨어지면 제거
        for (int i = _balls.Count - 1; i >= 0; i--)
        {
            if (_balls[i].Y > 24)
            {
                RemoveGameObject(_balls[i]);
                _balls.RemoveAt(i);
            }
        }

        // 모든 벽돌 클리어 시 다음 스테이지 또는 게임 클리어
        if (_bricks?.All(b => !b.IsActive || b is InvincibleBrick) == true)
        {
            if (_currenStage >= _stageManager.GetTotalStages())
                sceneManager.ChangeScene(new ClearScene(sceneManager));
            else
            {
                _currenStage++;
                _stagecount++;
                ClearGameObjects();
                Load();
            }
        }
    }

    public override void Unload()
    {
        Ball.BottomWallActive = false; // 씬 종료 시 정적 상태 초기화
        ClearGameObjects();
    }
}
