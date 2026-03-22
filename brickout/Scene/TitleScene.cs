using Framework.Engine;

// ════════════════════════════════════════
// 타이틀 씬 — 게임 시작 전 메인 화면
//
// 표시 내용:
//   - 게임 타이틀 (BRICK BREAK)
//   - 조작 방법 안내
//   - 벽돌 종류 범례
//   - 상하단 장식 벽돌
//
// 전환: Enter 키 → PlayScene
// ════════════════════════════════════════
public class TitleScene : Scene
{
    private SceneManager<Scene> _sceneManager; // 씬 전환 관리자

    public TitleScene(SceneManager<Scene> manager)
    {
        _sceneManager = manager;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        // ── 타이틀 박스 ──
        // WriteTextCentered: 화면 가로 기준 중앙 정렬
        buffer.WriteTextCentered(3, "╔══════════════════════╗", ConsoleColor.Cyan);
        buffer.WriteTextCentered(4, "║       B R I C K      ║", ConsoleColor.Cyan);
        buffer.WriteTextCentered(5, "║       B R E A K      ║", ConsoleColor.Cyan);
        buffer.WriteTextCentered(6, "╚══════════════════════╝", ConsoleColor.Cyan);

        // ── 조작 방법 안내 ──
        buffer.WriteText(17, 11, "← →   : 좌우로 이동", ConsoleColor.Yellow);
        buffer.WriteText(17, 12, "Space : 공 발사",     ConsoleColor.Yellow);
        buffer.WriteText(17, 13, "Enter : 시작",        ConsoleColor.Yellow);

        // ── 벽돌 종류 범례 ──
        // 실제 게임에서 등장하는 색상과 동일하게 표시
        buffer.WriteText(15, 17, "□□", ConsoleColor.Red);     // 일반 벽돌
        buffer.WriteText(17, 17, " 일반  ", ConsoleColor.White);
        buffer.WriteText(24, 17, "□□", ConsoleColor.Yellow);  // 강화 벽돌 (2번 타격)
        buffer.WriteText(26, 17, " 강함  ", ConsoleColor.White);
        buffer.WriteText(33, 17, "□□", ConsoleColor.Magenta); // 폭탄 벽돌 (연쇄 파괴)
        buffer.WriteText(35, 17, " 폭탄  ", ConsoleColor.White);
        buffer.WriteText(42, 17, "□□", ConsoleColor.Gray);    // 무적 벽돌 (파괴 불가)
        buffer.WriteText(44, 17, " 무적",  ConsoleColor.White);

        // ── 상단 장식 ──
        // x=1부터 3칸 간격으로 노란 벽돌 배치
        for (int x = 1; x < 58; x += 3)
            buffer.WriteText(x, 1, "□", ConsoleColor.Yellow);

        // ── 하단 장식 ──
        for (int x = 1; x < 58; x += 3)
            buffer.WriteText(x, 23, "□", ConsoleColor.Yellow);
    }

    public override void Load()   { } // 타이틀은 동적 오브젝트 없음
    public override void Unload() { }

    public override void Update(float deltaTime)
    {
        // Enter 키 누르면 PlayScene으로 전환
        // IsKeyDown: 키를 누르는 순간 한 번만 발동 (IsKey는 누르는 동안 매 프레임)
        if (Input.IsKeyDown(ConsoleKey.Enter))
            _sceneManager.ChangeScene(new PlayScene(_sceneManager));
    }
}
