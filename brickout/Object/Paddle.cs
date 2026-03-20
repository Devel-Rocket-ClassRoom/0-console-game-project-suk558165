using System;
using Framework.Engine;

public class Paddle : GameObject
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Speed { get; set; } = 30f; 
    public int Width { get; private set; } = 12; 

    public Paddle(Scene scene) : base(scene)
    {
        X = 25;
        Y = 22;
    }

    public override void Draw(ScreenBuffer buffer)
    {
        buffer.WriteText((int)X, (int)Y, "◀■■■■■■■■■▶", ConsoleColor.White);
    }

    public override void Update(float deltaTime)
    {
        if (Input.IsKey(ConsoleKey.LeftArrow))
        {
            X -= Speed * deltaTime;
        }
        if (Input.IsKey(ConsoleKey.RightArrow))
        {
            X += Speed * deltaTime;
        }
        if (X < 0)
        {
            X = 0;
        }
        if (X > 60 - Width)
        {
            X = 60 - Width;
        }
    }
}

