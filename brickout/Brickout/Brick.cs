using Framework.Engine;

public class Brick : GameObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public Brick(Scene scene, int x, int y) : base(scene)
    {
        X = x;
        Y = y;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "□□", ConsoleColor.Red);
    }

    public override void Update(float deltaTime)
    {
    }
}