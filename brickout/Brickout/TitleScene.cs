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
        buffer.WriteTextCentered(9, "Brick Break", ConsoleColor.Cyan);
        buffer.WriteTextCentered(15, "Press Enter to Start", ConsoleColor.Green);
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