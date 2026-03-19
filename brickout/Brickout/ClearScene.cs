using Framework.Engine;


class ClearScene : Scene
{
    private SceneManager<Scene> _sceneManager;

    public ClearScene(SceneManager<Scene> manager)
    {
        _sceneManager = manager;
    }
    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteTextCentered(9, "★ Stage Clear ★", ConsoleColor.Green);
        buffer.WriteTextCentered(10, "Congratulations!", ConsoleColor.Green);
        buffer.WriteTextCentered(15, "Press ENTER to Start ", ConsoleColor.Green);
    }

    public override void Update(float deltaTime)
    {
        if (Input.IsKeyDown(ConsoleKey.Enter))
        {
            _sceneManager.ChangeScene(new TitleScene(_sceneManager));
        }
    }
    public override void Load()
    {
    }

    public override void Unload()
    {
    }
}
