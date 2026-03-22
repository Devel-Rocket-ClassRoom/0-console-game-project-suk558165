using Framework.Engine;
using System.Linq;

// ════════════════════════════════════════════════════════════
// 스테이지 관리자 — 전체 10개 스테이지의 벽돌 배치와 공 속도를 정의
//
// 생성자에서 모든 스테이지를 미리 생성해 _stages 목록에 저장
// PlayScene에서 GetStage(n)으로 원하는 스테이지 데이터를 꺼냄
//
// 난이도 상승 구조:
//   ① 공 속도: Stage 1(0.080s) → Stage 10(0.044s) — 프레임당 이동 간격 단축
//   ② 특수 벽돌 비율: Stage 1(0%) → Stage 10(95%) — 스테이지당 약 9%씩 증가
//   ③ 벽돌 배치 패턴: 단순 격자 → 요새 → 고밀도 전면 배치
// ════════════════════════════════════════════════════════════
public class StageManager
{
    private List<StageData> _stages; // 전체 스테이지 데이터 목록 (인덱스 0 = Stage 1)
    private Random _random = new Random(); // 특수 벽돌 타입 결정용 난수 생성기

    // ────────────────────────────────────────────────────────
    // 특수 벽돌 타입 결정
    //
    // 두 단계 확률 판정:
    //   1단계) 이 벽돌이 특수 벽돌인가?
    //          specialChance% 확률 — 스테이지가 높을수록 증가
    //   2단계) 특수 벽돌이라면 어떤 종류인가?
    //          bomb / invincible / hard 가중치 판정
    //
    // 스테이지별 특수 벽돌 등장 확률:
    //   Stage 1: 0% (일반만 — 시스템 학습 단계)
    //   Stage 2: 15%  Stage 3: 24%  Stage 4: 33%  Stage 5: 42%
    //   Stage 6: 51%  Stage 7: 60%  Stage 8: 69%  Stage 9: 78%  Stage 10: 87%(cap 95%)
    //
    // 특수 벽돌 내 구성 변화 (스테이지별 가중치):
    //   Stage 2: bomb 5%, invincible  0%, hard 95%  → hard 위주, bomb 소량
    //   Stage 6: bomb 9%, invincible 16%, hard 75%  → invincible 등장
    //   Stage 10: bomb 13%, invincible 48%, hard 39% → invincible 대폭 증가
    // ────────────────────────────────────────────────────────
    private string GetRandomType(int stage)
    {
        if (stage == 1) return "normal"; // 1스테이지는 무조건 일반 벽돌

        // 특수 벽돌 등장 확률 계산
        // Stage 2: 15 + 0*9 = 15%  Stage 10: 15 + 8*9 = 87% → cap 95%
        int specialChance = Math.Min(15 + (stage - 2) * 9, 95);

        // specialChance% 미만의 확률로만 특수 벽돌 등장
        if (_random.Next(100) >= specialChance)
            return "normal";

        // ── 특수 벽돌 종류 결정 ──
        // 각 타입의 가중치(%)를 합산해 0~99 범위에서 판정
        int bombW       = 5 + (stage - 2);                       // Stage 2: 5% → Stage 10: 13%
        int invincibleW = Math.Max(0, (stage - 4) * 8);          // Stage 4부터 등장: 0 → 48%
        // hardW는 나머지 전부 (100 - bombW - invincibleW)
        // Stage 2: 95%  Stage 10: 39%

        int roll = _random.Next(100);
        if (roll < bombW)               return "bomb";       // 연쇄 폭발
        if (roll < bombW + invincibleW) return "invincible"; // 파괴 불가
        return "hard";                                        // 2번 타격 필요
    }

    // ────────────────────────────────────────────────────────
    // 공 이동 간격 계산 (초)
    // 값이 작을수록 공이 빠르게 이동
    //
    // Stage 1: 0.080s (기준)
    // 스테이지당 0.004s씩 단축
    // Stage 10: 0.080 - 9*0.004 = 0.044s (최소값 cap)
    // ────────────────────────────────────────────────────────
    private float GetBallInterval(int stage)
    {
        return Math.Max(0.044f, 0.080f - (stage - 1) * 0.004f);
    }

    public StageManager()
    {
        _stages = new List<StageData>();

        // ══════════════════════════════════════════
        // Stage 1 — 튜토리얼
        // 일반 벽돌만, 넓고 규칙적인 3행 격자
        // 공 속도 최저, 게임 흐름 파악에 집중
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int y = 3; y <= 9; y += 3)      // Y: 3, 6, 9 (3행)
                for (int x = 4; x <= 52; x += 5) // X: 5칸 간격 (넓은 격자)
                    pos.Add((x, y, GetRandomType(1)));
            _stages.Add(new StageData(1, GetBallInterval(1), 1, pos));
        }

        // ══════════════════════════════════════════
        // Stage 2 — 두 덩어리 패턴
        // 화면 좌우에 벽돌 덩어리 2개, hard 벽돌 소량 등장
        // 빈 공간을 활용한 각도 조절 연습
        // ══════════════════════════════════════════
        {
            var raw = new List<(int, int)>
            {
                (20,3),(24,3),(16,4),(20,4), // 왼쪽 덩어리 상단
                (32,3),(36,3),(36,4),(40,4), // 오른쪽 덩어리 상단
                (20,6),(24,6),(16,7),(20,7), // 왼쪽 덩어리 중단
                (32,6),(36,6),(36,7),(40,7), // 오른쪽 덩어리 중단
                (20,9),(24,9),(16,8),(20,8), // 왼쪽 덩어리 하단
                (32,9),(36,9),(36,8),(40,8)  // 오른쪽 덩어리 하단
            };
            var pos = raw.Select(p => (p.Item1, p.Item2, GetRandomType(2))).ToList();
            _stages.Add(new StageData(2, GetBallInterval(2), 2, pos));
        }

        // ══════════════════════════════════════════
        // Stage 3 — 다이아몬드(◇) 형태
        // 중앙에서 바깥으로 퍼지는 피라미드 구조
        // 꼭짓점을 노리는 각도 조절이 관건
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int i = 0; i <= 4; i++) // i=0: 중앙 1개 → i=4: 가장 넓은 줄
            {
                int startX = 28 - i * 4; // 중앙(28)에서 왼쪽으로 i*4
                int endX   = 28 + i * 4; // 중앙(28)에서 오른쪽으로 i*4
                for (int x = startX; x <= endX; x += 3)
                    pos.Add((x, 3 + i, GetRandomType(3)));
            }
            _stages.Add(new StageData(3, GetBallInterval(3), 2, pos));
        }

        // ══════════════════════════════════════════
        // Stage 4 — 체스판 패턴
        // 짝수 Y행에만 벽돌 배치 → 빈 줄과 교차
        // bomb 벽돌 첫 등장 — 연쇄 폭발 학습
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 10; y += 2) // 짝수 행만
                for (int x = 4; x <= 52; x += 4)
                    pos.Add((x, y, GetRandomType(4)));
            _stages.Add(new StageData(4, GetBallInterval(4), 2, pos));
        }

        // ══════════════════════════════════════════
        // Stage 5 — 지그재그 밀집 배치
        // 홀수/짝수 행의 시작 X를 3칸씩 엇갈려 배치
        // 밀도가 높아 공이 빠르게 튕김, 속도 체감 시작
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 9; y++)
            {
                int startX = (y % 2 == 0) ? 4 : 7; // 짝수행: x=4, 홀수행: x=7 (지그재그)
                for (int x = startX; x <= 52; x += 4)
                    pos.Add((x, y, GetRandomType(5)));
            }
            _stages.Add(new StageData(5, GetBallInterval(5), 3, pos));
        }

        // ══════════════════════════════════════════
        // Stage 6 — 십자(+) 교차 패턴
        // 가로줄(Y=5) + 세로줄(X=28)이 교차
        // invincible 벽돌 첫 등장 — 피해가기 전략 필요
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int x = 4; x <= 52; x += 3)  // 가로줄: Y=5 전체
                pos.Add((x, 5, GetRandomType(6)));
            for (int y = 2; y <= 9; y++)       // 세로줄: X=28 전체
                pos.Add((28, y, GetRandomType(6)));
            _stages.Add(new StageData(6, GetBallInterval(6), 3, pos));
        }

        // ══════════════════════════════════════════
        // Stage 7 — 외곽 테두리 + 중앙 요새
        // 상단 가로줄 + 좌우 세로줄 + 중앙 블록(4x5)
        // 빈 공간이 줄어들어 각도 조절이 까다로워짐
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int x = 4; x <= 52; x += 3)   // 상단 가로줄 (Y=2)
                pos.Add((x, 2, GetRandomType(7)));
            for (int y = 3; y <= 9; y++)         // 좌우 세로줄
            {
                pos.Add((4,  y, GetRandomType(7))); // 왼쪽
                pos.Add((52, y, GetRandomType(7))); // 오른쪽
            }
            for (int y = 4; y <= 7; y++)         // 중앙 블록 (X=22~36, Y=4~7)
                for (int x = 22; x <= 36; x += 3)
                    pos.Add((x, y, GetRandomType(7)));
            _stages.Add(new StageData(7, GetBallInterval(7), 3, pos));
        }

        // ══════════════════════════════════════════
        // Stage 8 — 미로형 배치 (invincible 장벽)
        // invincible 수평 장벽 2줄이 화면을 구역으로 나눔
        //   장벽 1: Y=4, X=4~36 (왼쪽 구역 차단)
        //   장벽 2: Y=7, X=22~52 (오른쪽 구역 차단)
        // 나머지 공간은 일반/특수 벽돌로 채움
        // 장벽 때문에 특정 구역 진입이 어려워 전략적 플레이 필요
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int x = 4; x <= 36; x += 3)   // 장벽 1: 왼쪽 상단 (invincible 고정)
                pos.Add((x, 4, "invincible"));
            for (int x = 22; x <= 52; x += 3)  // 장벽 2: 오른쪽 하단 (invincible 고정)
                pos.Add((x, 7, "invincible"));
            for (int y = 2; y <= 9; y++)        // 나머지 영역 채우기
                for (int x = 4; x <= 52; x += 3)
                {
                    bool isBarrier = (y == 4 && x <= 36) || (y == 7 && x >= 22);
                    if (!isBarrier) // 장벽 위치는 이미 추가했으므로 중복 방지
                        pos.Add((x, y, GetRandomType(8)));
                }
            _stages.Add(new StageData(8, GetBallInterval(8), 4, pos));
        }

        // ══════════════════════════════════════════
        // Stage 9 — 고밀도 전면 배치
        // 화면 대부분을 벽돌로 채움 (Y=2~10, X=4~52, 3칸 간격)
        // 빈 공간 최소화 → 공이 오래 위쪽에서 튕기며 연속 히트
        // invincible 비율 대폭 증가로 돌파구 찾기 어려움
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 10; y++)
                for (int x = 4; x <= 52; x += 3)
                    pos.Add((x, y, GetRandomType(9)));
            _stages.Add(new StageData(9, GetBallInterval(9), 4, pos));
        }

        // ══════════════════════════════════════════
        // Stage 10 — 최종 보스 스테이지
        // Stage 9의 전면 배치 + 중앙 3x3 invincible 요새
        //   요새 위치: X=25~31, Y=5~7 (화면 정중앙)
        //   요새는 절대 파괴 불가 → 공이 중앙에서 계속 튕겨나옴
        // 공 속도 최고, 특수 벽돌 비율 최고 (95% cap)
        // ══════════════════════════════════════════
        {
            var pos = new List<(int, int, string)>();
            for (int y = 2; y <= 10; y++)
                for (int x = 4; x <= 52; x += 3)
                {
                    // 중앙 요새 영역은 invincible 고정 (GetRandomType 호출 안 함)
                    bool isFortress = (x >= 25 && x <= 31 && y >= 5 && y <= 7);
                    string type = isFortress ? "invincible" : GetRandomType(10);
                    pos.Add((x, y, type));
                }
            _stages.Add(new StageData(10, GetBallInterval(10), 5, pos));
        }
    }

    // 1-based 인덱스로 스테이지 데이터 반환
    // stageNumber=1 → _stages[0]
    public StageData GetStage(int stageNumber) => _stages[stageNumber - 1];

    // 전체 스테이지 수 반환 — PlayScene의 클리어 판정에 사용
    public int GetTotalStages() => _stages.Count;
}
