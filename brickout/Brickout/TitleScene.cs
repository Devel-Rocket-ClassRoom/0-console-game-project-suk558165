using Framework.Engine;

public class TitleScene : Scene
{

    private SceneManager<Scene> _sceneManager;

    public TitleScene(SceneManager<Scene> manager)
    {
        _sceneManager = manager;
    }
    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteTextCentered(7, "Brick Break", ConsoleColor.Cyan);
        buffer.WriteText(19,10, "← → : 좌우키로 움직이세요.", ConsoleColor.White);
        buffer.WriteText(19,15, "Space : 공을 발사하세요.", ConsoleColor.White);
        buffer.WriteTextCentered(17, "Press Enter to Start", ConsoleColor.Green);
    }

    public override void Load()
    {
    }

    public override void Unload()
    {
    }

    public override void Update(float deltaTime)
    {
        if (Input.IsKeyDown(ConsoleKey.Enter))
        {
            _sceneManager.ChangeScene(new PlayScene(_sceneManager));
        }
    }
}