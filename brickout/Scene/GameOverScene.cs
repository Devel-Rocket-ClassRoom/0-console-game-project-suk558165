using Framework.Engine;

// ════════════════════════════════════════
// 게임오버 씬 — 목숨이 0이 되면 표시
//
// 표시 내용: 게임오버 박스 + 재시도 안내
// 전환: Enter 키 → TitleScene
// ════════════════════════════════════════
class GameOverScene : Scene
{
    private SceneManager<Scene> _sceneManager;

    public GameOverScene(SceneManager<Scene> manager)
    {
        _sceneManager = manager;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // 빨간 테두리 박스 — 게임오버 분위기 강조
        buffer.DrawBox(10, 5, 40, 15, ConsoleColor.Red);

        // 게임오버 텍스트 — 중앙 정렬
        buffer.WriteTextCentered(9,  "★ GAME OVER ★",       ConsoleColor.Red);
        buffer.WriteTextCentered(12, "Try Again!",            ConsoleColor.Yellow);
        buffer.WriteTextCentered(15, "Press Enter to Retry", ConsoleColor.Gray);
    }

    public override void Update(float deltaTime)
    {
        // Enter 키 → 타이틀 화면으로 돌아감 (PlayScene을 새로 시작하지 않고 타이틀 경유)
        if (Input.IsKeyDown(ConsoleKey.Enter))
            _sceneManager.ChangeScene(new TitleScene(_sceneManager));
    }

    public override void Load()   { }
    public override void Unload() { }
}
