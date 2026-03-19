using System;
using Framework.Engine;
using System.Linq;

class PlayScene : Scene
{
    private SceneManager<Scene> sceneManager;
    private List<Brick>? _bricks;
    private int _lives = 3;
    private Ball? _ball;
    private Paddle? _paddle;

    public PlayScene(SceneManager<Scene> sceneManager)
    {
        this.sceneManager = sceneManager;
    }
    

    public override void Draw(ScreenBuffer buffer) // 화면 출력
    {
        DrawGameObjects(buffer);
        buffer.WriteText(1, 0, $"Lives: {_lives}", ConsoleColor.Yellow);
    }

    public override void Load()  // 무대 세팅
    {
        AddGameObject(new Wall(this, 0, 0, 60, 25)); // 벽 생성

        _paddle = new Paddle(this); // 패들 생성
        AddGameObject(_paddle);

       _bricks = new List<Brick>(); // 벽돌 배열
        Random random = new Random();
        int count = random.Next(10, 30);

        for (int i = 0; i < count; i++) // 배열 배치
        {
            int x = random.Next(2, 55);
            int y = random.Next(2, 10);
            Brick brick = (new Brick(this, x, y));
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

        if (_bricks?.All(b => !b.IsActive) == true)
        {
            sceneManager.ChangeScene(new ClearScene(sceneManager));
        }
        
    }
    public override void Unload() // 게임 승패 조건 체크, 공과 발판 움직임 
    {
        ClearGameObjects();
    }
}
