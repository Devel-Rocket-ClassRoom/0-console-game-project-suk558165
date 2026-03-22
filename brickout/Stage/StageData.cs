using System;

// ════════════════════════════════════════════════════════════
// 스테이지 데이터 컨테이너 — 하나의 스테이지에 필요한 모든 정보를 담음
//
// StageManager 생성자에서 스테이지별로 생성되고
// PlayScene에서 GetStage(n)으로 꺼내 사용
// ════════════════════════════════════════════════════════════
public class StageData
{
    // 스테이지 번호 (1~10) — 현재는 표시용으로만 사용
    public int StageNumber;

    // 공 이동 간격 (초) — 작을수록 빠름
    // Ball.SetInterval()에 그대로 전달
    // Stage 1: 0.080f (느림) → Stage 10: 0.044f (빠름)
    public float BallInterval;

    // 속도 레벨 (1~5) — PlayScene HUD의 SPD ★★★☆☆ 표시에 사용
    // BallInterval에서 파생되는 시각적 표현용 값
    public int BallSpeedLevel;

    // 벽돌 배치 목록 — (x좌표, y좌표, 타입문자열) 튜플의 리스트
    // 타입 문자열: "normal" / "hard" / "bomb" / "invincible"
    // PlayScene의 Load()에서 순회하며 벽돌 오브젝트를 생성
    public List<(int x, int y, string type)> BrickPositions;

    public StageData(int stageNumber, float ballInterval, int ballSpeedLevel,
                     List<(int x, int y, string type)> brickPositions)
    {
        StageNumber    = stageNumber;
        BallInterval   = ballInterval;
        BallSpeedLevel = ballSpeedLevel;
        BrickPositions = brickPositions;
    }
}
