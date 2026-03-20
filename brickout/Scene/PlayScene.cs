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
    private const int _maxItems = 5;

    public PlayScene(SceneManager<Scene> sceneManager)
    {
        this.sceneManager = sceneManager;
    }

    public void AddBall()
    {
        if (_paddle == null || _bricks == null) return;
        var newBall = new Ball(this, _paddle, _bricks);
        newBall.Launch(); // 바로 발사
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

    public void AddWall()
    {
        if (_bottomWall != null) return;
        _bottomWall = new Wall(this, 1, 23, 58, 1);
        AddGameObject(_bottomWall);
    }

    public override void Draw(ScreenBuffer buffer)
    {
        DrawGameObjects(buffer);
        buffer.WriteText(52, 0, $"Lives: {_lives}", ConsoleColor.Yellow);
        buffer.WriteText(1, 0, $"Stage: {_stagecount}", ConsoleColor.Yellow);

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

        AddGameObject(new Wall(this, 0, 0, 60, 25));

        _paddle = new Paddle(this);
        AddGameObject(_paddle);

        var stage = _stageManager.GetStage(_currenStage);
        _bricks = new List<Brick>();

        foreach (var (x, y, type) in stage.BrickPositions)
        {
            Brick brick = type switch
            {
                "hard" => new HardBrick(this, x, y),
                "bomb" => new BombBrick(this, x, y, _bricks),
                "invincible" => new InvincibleBrick(this, x, y),
                _ => new Brick(this, x, y)
            };

            float bx = brick.X;
            float by = brick.Y;
            brick.OnHit = () =>
            {
                if (_itemCount < _maxItems && _random.Next(100) < 30)
                {
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
                }
            };

            if (brick is BombBrick bomb)
                bomb.OnExplode = () => _explodingBombs.Add(bomb);

            _bricks.Add(brick);
            AddGameObject(brick);
        }

        _ball = new Ball(this, _paddle, _bricks);
        AddGameObject(_ball);
    }

    public override void Update(float deltaTime)
    {
        if (_slowTimer > 0 && _paddle != null)
        {
            _slowTimer -= deltaTime;
            if (_slowTimer <= 0)
                _paddle.Speed = 30f;
        }

        UpdateGameObjects(deltaTime);

        for (int i = _explodingBombs.Count - 1; i >= 0; i--)
        {
            _explodingBombs[i].UpdateExplode(deltaTime);
            if (!_explodingBombs[i].IsExploding)
                _explodingBombs.RemoveAt(i);
        }

        if (_ball == null || _bricks == null || _paddle == null) return;

        if (_ball.Y > 24)
        {
            _lives--;
            _ball.IsActive = false;
            RemoveGameObject(_ball);

            if (_lives <= 0)
                sceneManager.ChangeScene(new GameOverScene(sceneManager));
            else
            {
                _ball = new Ball(this, _paddle, _bricks);
                AddGameObject(_ball);
            }
        }

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
        ClearGameObjects();
    }
}