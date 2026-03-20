using Framework.Engine;

Console.OutputEncoding = System.Text.Encoding.UTF8;
new BrickoutApp().Run();
public class BrickoutApp : GameApp
{

    private SceneManager<Scene> _sceneManager = new();

    public BrickoutApp() : base(60, 25) { }

    protected override void Draw()
    {
        _sceneManager.CurrentScene?.Draw(Buffer);
    }

    protected override void Initialize()
    {
        _sceneManager.ChangeScene(new TitleScene(_sceneManager));
    }

    protected override void Update(float deltaTime)
    {
        _sceneManager.CurrentScene?.Update(deltaTime);
    }
}

