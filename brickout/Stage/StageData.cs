using System;

public class StageData
{
    public int StageNumber;
    public float BallInterval;   // 공 이동 간격(초) — 작을수록 빠름
    public int BallSpeedLevel;   // PlayScene에서 참고용 (1~5)
    public List<(int x, int y, string type)> BrickPositions;

    public StageData(int stageNumber, float ballInterval, int ballSpeedLevel,
                     List<(int x, int y, string type)> brickPositions)
    {
        StageNumber   = stageNumber;
        BallInterval  = ballInterval;
        BallSpeedLevel = ballSpeedLevel;
        BrickPositions = brickPositions;
    }
}
