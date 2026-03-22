using Framework.Engine;

// ════════════════════════════════════════
// 클리어 씬 — 모든 스테이지 클리어 시 표시
//
// 표시 내용: 클리어 메시지 + 재시작 안내
// 전환: Enter 키 → TitleScene
// ════════════════════════════════════════
class ClearScene : Scene
{
    private SceneManager<Scene> _sceneManager;

    public ClearScene(SceneManager<Scene> manager)
    {
        _sceneManager = manager;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // 클리어 메시지 — 중앙 정렬
        buffer.WriteTextCentered(9,  "★ Stage Clear ★",      ConsoleColor.Green);
        buffer.WriteText(10, 15,     "Congratulations!",       ConsoleColor.Green);
        buffer.WriteTextCentered(15, "Press ENTER to Start ", ConsoleColor.Green);
    }

    public override void Update(float deltaTime)
    {
        // Enter 키 → 타이틀 화면으로 돌아감
        if (Input.IsKeyDown(ConsoleKey.Enter))
            _sceneManager.ChangeScene(new TitleScene(_sceneManager));
    }

    public override void Load()   { }
    public override void Unload() { }
}
