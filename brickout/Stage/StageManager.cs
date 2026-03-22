using Framework.Engine;
using System.Linq;

public class StageManager
{
    private List<StageData> _stages;
    private Random _random = new Random();

    // ── 특수 벽돌 타입 결정 ──
    // 스테이지가 올라갈수록 특수 벽돌 비율과 강도가 높아짐
    //
    // 특수 벽돌 등장 확률:
    //   Stage 1  → 0%   (일반만)
    //   Stage 2  → 15%
    //   Stage 3  → 25%
    //   Stage 4  → 35%
    //   Stage 5  → 45%
    //   Stage 6  → 55%
    //   Stage 7  → 65%
    //   Stage 8  → 75%
    //   Stage 9  → 85%
    //   Stage 10 → 95%
    //
    // 특수 벽돌 내 구성 (스테이지별 가중치 변화):
    //   초반: hard 위주, bomb 소량, invincible 없음
    //   중반: bomb 증가, invincible 등장
    //   후반: invincible 대폭 증가, bomb 유지
    private string GetRandomType(int stage)
    {
        if (stage == 1) return "normal";

        // 특수 벽돌 등장 확률: 15% + 스테이지당 8.9%씩 증가
        int specialChance = 15 + (stage - 2) * 9; // 2→15, 10→87 (최대 95 cap)
        specialChance = Math.Min(specialChance, 95);

        if (_random.Next(100) >= specialChance)
            return "normal";

        // 특수 벽돌 내 구성 — 스테이지별 가중치
        // bomb: 5→15%, hard: 90→30%, invincible: 5→55%
        int bombW       = 5  + (stage - 2) * 1;   // 5~14%
        int invincibleW = Math.Max(0, (stage - 4) * 8); // 4스테이지부터 등장, 최대 48%
        int hardW       = 100 - bombW - invincibleW;

        int roll = _random.Next(100);
        if (roll < bombW)                    return "bomb";
        if (roll < bombW + invincibleW)      return "invincible";
        return "hard";
    }

    // ── 공 이동 간격 (초) ──
    // 작을수록 빠름. 0.08 → 0.048 (스테이지당 약 4ms씩 단축)
    private float GetBallInterval(int stage)
    {
        // Stage 1: 0.080f  Stage 10: 0.044f
        return Math.Max(0.044f, 0.080f - (stage - 1) * 0.004f);
    }

    public StageManager()
    {
        _stages = new List<StageData>();

        // ── Stage 1 ── 일반 벽돌만, 넓고 규칙적인 격자
        {
            var pos = new List<(int, int, string)>();
            for (int y = 3; y <= 9; y += 3)
                for (int x = 4; x <= 52; x += 5)
                    pos.Add((x, y, GetRandomType(1)));
            _stages.Add(new StageData(1, GetBallInterval(1), 1, pos));
        }

        // ── Stage 2 ── 두 덩어리 패턴, hard 벽돌 소량 등장
        {
            var raw = new List<(int, int)>
            {
                (20,3),(24,3),(16,4),(20,4),
                (32,3),(36,3),(36,4),(40,4),
                (20,6),(24,6),(16,7),(20,7),
                (32,6),(36,6),(36,7),(40,7),
                (20,9),(24,9),(16,8),(20,8),
                (32,9),(36,9),(36,8),(40,8)
            };
            var pos = raw.Select(p => (p.Item1, p.Item2, GetRandomType(2))).ToList();
            _stages.Add(new StageData(2, GetBallInterval(2), 2, pos));
        }

        // ── Stage 3 ── 다이아몬드 형태 + 벽돌 밀도 증가
        {
            var pos = new List<(int, int, string)>();
            for (int i = 0; i <= 4; i++)
            {
                int startX = 28 - i * 4;
                int endX   = 28 + i * 4;
                for (int x = startX; x <= endX; x += 3)
                    pos.Add((x, 3 + i, GetRandomType(3)));
            }
            _stages.Add(new StageData(3, GetBallInterval(3), 2, pos));
        }

        // ── Stage 4 ── 체스판 패턴, bomb 등장 시작
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 10; y += 2)
                for (int x = 4; x <= 52; x += 4)
                    pos.Add((x, y, GetRandomType(4)));
            _stages.Add(new StageData(4, GetBallInterval(4), 2, pos));
        }

        // ── Stage 5 ── 지그재그 밀집, 속도 체감 시작
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 9; y++)
            {
                int startX = (y % 2 == 0) ? 4 : 7;
                for (int x = startX; x <= 52; x += 4)
                    pos.Add((x, y, GetRandomType(5)));
            }
            _stages.Add(new StageData(5, GetBallInterval(5), 3, pos));
        }

        // ── Stage 6 ── X자 교차 패턴, invincible 첫 등장
        {
            var pos = new List<(int, int, string)>();
            // 가로줄
            for (int x = 4; x <= 52; x += 3)
                pos.Add((x, 5, GetRandomType(6)));
            // 세로줄
            for (int y = 2; y <= 9; y++)
                pos.Add((28, y, GetRandomType(6)));
            _stages.Add(new StageData(6, GetBallInterval(6), 3, pos));
        }

        // ── Stage 7 ── 테두리 + 중앙 요새, 빈 공간 줄어듦
        {
            var pos = new List<(int, int, string)>();
            // 상단 테두리
            for (int x = 4; x <= 52; x += 3)
                pos.Add((x, 2, GetRandomType(7)));
            // 좌우 세로
            for (int y = 3; y <= 9; y++)
            {
                pos.Add((4,  y, GetRandomType(7)));
                pos.Add((52, y, GetRandomType(7)));
            }
            // 중앙 블록
            for (int y = 4; y <= 7; y++)
                for (int x = 22; x <= 36; x += 3)
                    pos.Add((x, y, GetRandomType(7)));
            _stages.Add(new StageData(7, GetBallInterval(7), 3, pos));
        }

        // ── Stage 8 ── 미로형 벽돌 배치, invincible 장벽
        {
            var pos = new List<(int, int, string)>();
            // 수평 장벽 2줄 (invincible 고정)
            for (int x = 4; x <= 36; x += 3)
                pos.Add((x, 4, "invincible"));
            for (int x = 22; x <= 52; x += 3)
                pos.Add((x, 7, "invincible"));
            // 나머지 영역 채우기
            for (int y = 2; y <= 9; y++)
                for (int x = 4; x <= 52; x += 3)
                {
                    bool isBarrier = (y == 4 && x <= 36) || (y == 7 && x >= 22);
                    if (!isBarrier)
                        pos.Add((x, y, GetRandomType(8)));
                }
            _stages.Add(new StageData(8, GetBallInterval(8), 4, pos));
        }

        // ── Stage 9 ── 고밀도 전면 배치, 빈 공간 최소화
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 10; y++)
                for (int x = 4; x <= 52; x += 3)
                    pos.Add((x, y, GetRandomType(9)));
            _stages.Add(new StageData(9, GetBallInterval(9), 4, pos));
        }

        // ── Stage 10 ── 최종 보스: 빽빽한 배치 + 중앙 invincible 요새
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 10; y++)
                for (int x = 4; x <= 52; x += 3)
                {
                    // 중앙 3x3 invincible 요새
                    bool isFortress = (x >= 25 && x <= 31 && y >= 5 && y <= 7);
                    string type = isFortress ? "invincible" : GetRandomType(10);
                    pos.Add((x, y, type));
                }
            _stages.Add(new StageData(10, GetBallInterval(10), 5, pos));
        }
    }

    public StageData GetStage(int stageNumber) => _stages[stageNumber - 1];
    public int GetTotalStages() => _stages.Count;
}
