using Framework.Engine;

 class GameOverScene : Scene
    {
    private SceneManager<Scene> _sceneManager;

    public GameOverScene(SceneManager<Scene> manager)
    {
        _sceneManager = manager;
    }
public override void Draw(ScreenBuffer buffer)
    {
        // 박스 테두리
        buffer.DrawBox(10, 5, 40, 15, ConsoleColor.Red);

        // 게임오버 텍스트
        buffer.WriteTextCentered(9, "★ GAME OVER ★", ConsoleColor.Red);
        buffer.WriteTextCentered(12, "Try Again!", ConsoleColor.Yellow);
        buffer.WriteTextCentered(15, "Press Enter to Retry", ConsoleColor.Gray);

    }

    public override void Update(float deltaTime)
    {
        if ( Input.IsKeyDown(ConsoleKey.Enter))
        {
            _sceneManager.ChangeScene(new TitleScene(_sceneManager));
        }
    }
    public override void Load() { }
    public override void Unload() { }
}

