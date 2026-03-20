using System;
using Framework.Engine;
using System.Linq;

class PlayScene : Scene
{
    private SceneManager<Scene> sceneManager;
    private StageManager _stageManager = new StageManager();
    private int _currenStage = 1;
    private List<Brick>? _bricks;
    private int _lives = 3;
    private Ball? _ball;
    private Paddle? _paddle;
    private int _stagecount = 1;

    public PlayScene(SceneManager<Scene> sceneManager)
    {
        this.sceneManager = sceneManager;
    }
    
    public override void Draw(ScreenBuffer buffer) // 화면 출력
    {
        DrawGameObjects(buffer);
        buffer.WriteText(52, 0, $"Lives: {_lives}", ConsoleColor.Yellow);
        buffer.WriteText(1, 0, $"Stage: {_stagecount}", ConsoleColor.Yellow);
    }

    public override void Load()  // 무대 세팅
    {
        AddGameObject(new Wall(this, 0, 0, 60, 25)); // 벽 생성

        _paddle = new Paddle(this); // 패들 생성
        AddGameObject(_paddle);

        var stage = _stageManager.GetStage(_currenStage);
        _bricks = new List<Brick>();
        foreach (var (x, y, type) in stage.BrickPositions)
        {
            Brick brick = type switch
            {
                "haed" => new HardBrick(this, x, y),
                "bomb" => new BombBrick(this, x, y, _bricks),
                "invincible" => new InvincibleBrick(this, x, y),
                _ => new Brick(this, x, y)
            };
            _bricks.Add(brick);
            AddGameObject(brick);
        }

        _ball = new Ball(this, _paddle, _bricks);
        AddGameObject(_ball);
    }

    public override void Update(float deltaTime) // 게임 클리어 
    {
        UpdateGameObjects(deltaTime);

        if (_ball == null || _bricks == null || _paddle == null) return;
        if (_ball.Y > 24)
        {
            _lives--;
            _ball.IsActive = false;
            RemoveGameObject(_ball);

            if (_lives <= 0)
            {
                sceneManager.ChangeScene(new GameOverScene(sceneManager));
            }
            else
            {
                _ball = new Ball(this, _paddle, _bricks);
                AddGameObject(_ball);

            }
        }

        if (_bricks?.All(b => !b.IsActive || b is InvincibleBrick) == true)
        {
            if (-_currenStage >= _stageManager.GetTotalStages())
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
    public override void Unload() // 게임 승패 조건 체크, 공과 발판 움직임 
    {
        ClearGameObjects();
    }
}
