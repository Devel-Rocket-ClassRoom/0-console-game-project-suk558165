using System;

public class StageData
{
    public int StageNumber;
    public float BallSpeed;
    public List<(int x, int y, string type)> BrickPositions;

    public StageData(int stageNumber, float ballSpeed, List<(int x, int y, string type)> brickPositions)
    {
        StageNumber = stageNumber;
        BallSpeed = ballSpeed;
        BrickPositions = brickPositions;
    }
}