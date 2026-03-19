using System;
using Framework.Engine;

class PlayScene : Scene
{
    private SceneManager<Scene> sceneManager;

    public PlayScene(SceneManager<Scene> sceneManager)
    {
        this.sceneManager = sceneManager;
    }
    

    public override void Draw(ScreenBuffer buffer) // 화면 출력
    {
        DrawGameObjects(buffer);
    }

    public override void Load()  // 무대 세팅
    {
        AddGameObject(new Wall(this, 0, 0, 60, 25));
        Paddle paddle = new Paddle(this);
        AddGameObject(paddle);
        Ball ball = new Ball(this);
        AddGameObject(ball);
        Brick brick = new Brick(this);
        AddGameObject(brick);
    }

    public override void Update(float deltaTime) // 게임 클리어 
    {
        UpdateGameObjects(deltaTime);
    }
    public override void Unload() // 게임 승패 조건 체크, 공과 발판 움직임 
    {
        ClearGameObjects();
    }
}
