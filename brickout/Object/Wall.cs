using System;
using Framework.Engine;



public class Wall : GameObject
{
    private int X, Y, Width, Height; // 필드

    public Wall(Scene scene, int x, int y, int width, int height) : base(scene) // 생성자
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override void Draw(ScreenBuffer buffer) // 벽 생성
    {
        buffer.DrawBox(X, Y, Width, Height, ConsoleColor.White);
    }

    public override void Update(float deltaTime)
    {
    }
}

