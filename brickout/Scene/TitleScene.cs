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
        buffer.WriteTextCentered(7, "Brick Break", ConsoleColor.Cyan);
        buffer.WriteText(19,10, "← → : 좌우키로 움직이세요.", ConsoleColor.Yellow);
        buffer.WriteText(19,15, "Space : 공을 발사하세요.", ConsoleColor.Yellow);
        buffer.WriteTextCentered(17, "Press Enter to Start", ConsoleColor.Green);
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