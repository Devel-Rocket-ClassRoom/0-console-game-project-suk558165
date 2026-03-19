using System;
using Framework.Engine;



public class Wall : GameObject
{
    private int X, Y, Width, Height;

    public Wall(Scene scene, int x, int y, int width, int height) : base(scene)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.DrawBox(X, Y, Width, Height, ConsoleColor.White);
    }

    public override void Update(float deltaTime)
    {
    }
}

