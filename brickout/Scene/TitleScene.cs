using Framework.Engine;

public class TitleScene : Scene
{

    private SceneManager<Scene> _sceneManager; // 필드

    public TitleScene(SceneManager<Scene> manager) // 생성자
    {
        _sceneManager = manager;
    }
    public override void Draw(ScreenBuffer buffer) // 화면 출력
    {
        buffer.WriteTextCentered(3, "╔══════════════════════╗", ConsoleColor.Cyan);
        buffer.WriteTextCentered(4, "║       B R I C K      ║", ConsoleColor.Cyan);
        buffer.WriteTextCentered(5, "║       B R E A K      ║", ConsoleColor.Cyan);
        buffer.WriteTextCentered(6, "╚══════════════════════╝", ConsoleColor.Cyan);
        buffer.WriteText(17, 11, "← →   : 좌우로 이동", ConsoleColor.Yellow);
        buffer.WriteText(17, 12, "Space : 공 발사", ConsoleColor.Yellow);
        buffer.WriteText(17, 13, "Enter : 시작", ConsoleColor.Yellow);
        buffer.WriteText(15, 17, "□□", ConsoleColor.Red);       
        buffer.WriteText(17, 17, " 일반  ", ConsoleColor.White);
        buffer.WriteText(24, 17, "□□", ConsoleColor.Yellow);     
        buffer.WriteText(26, 17, " 강함  ", ConsoleColor.White);
        buffer.WriteText(33, 17, "□□", ConsoleColor.Magenta);    
        buffer.WriteText(35, 17, " 폭탄  ", ConsoleColor.White);
        buffer.WriteText(42, 17, "□□", ConsoleColor.Gray);       
        buffer.WriteText(44, 17, " 무적", ConsoleColor.White);
        // 위쪽 장식
        for (int x = 1; x < 58; x += 3)
            buffer.WriteText(x, 1, "□", ConsoleColor.Yellow);

        // 아래쪽 장식
        for (int x = 1; x < 58; x += 3)
            buffer.WriteText(x, 23, "□", ConsoleColor.Yellow);
    }

    public override void Load() { }
    
    public override void Unload() { }

    public override void Update(float deltaTime) // 엔터 누르면 화면 플레이화면으로 바뀜
    {
        if (Input.IsKeyDown(ConsoleKey.Enter))
        {
            _sceneManager.ChangeScene(new PlayScene(_sceneManager));
        }
    }
}