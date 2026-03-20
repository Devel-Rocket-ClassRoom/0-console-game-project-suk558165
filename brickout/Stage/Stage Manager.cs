using Framework.Engine;
using System.Linq;

public class StageManager
{
    private List<StageData> _stages;
    private Random _random = new Random();

    private string GetRandomType(int stageNumber)
    {
        if (stageNumber == 1) return "normal";

        int specialChance = stageNumber * 10;
        if (_random.Next(100) >= specialChance)
            return "normal";

        int roll = _random.Next(100);
        if (roll < 10) return "bomb";       // 10%
        if (roll < 80) return "hard";       // 70%
        return "invincible";                // 20%
    }

    public StageManager()
    {
        _stages = new List<StageData>();

        // Stage 1
        var positions1 = new List<(int, int, string)>();
        for (int y = 3; y <= 9; y += 3)
            for (int x = 4; x <= 52; x += 5)
                positions1.Add((x, y, GetRandomType(1)));
        _stages.Add(new StageData(1, 0.08f, positions1));

        // Stage 2
        var raw2 = new List<(int, int)>
        {
            (20,3),(24,3),(16,4),(20,4),
            (32,3),(36,3),(36,4),(40,4),
            (20,6),(24,6),(16,7),(20,7),
            (32,6),(36,6),(36,7),(40,7),
            (20,9),(24,9),(16,8),(20,8),
            (32,9),(36,9),(36,8),(40,8)
        };
        var positions2 = raw2.Select(p => (p.Item1, p.Item2, GetRandomType(2))).ToList();
        _stages.Add(new StageData(2, 0.08f, positions2));

        // Stage 3
        var positions3 = new List<(int, int, string)>();
        for (int i = 0; i <= 3; i++)
        {
            int startX = 28 - i * 4;
            int endX = 28 + i * 4;
            for (int x = startX; x <= endX; x += 3)
                positions3.Add((x, 3 + i, GetRandomType(3)));
        }
        _stages.Add(new StageData(3, 0.08f, positions3));

        // Stage 4
        var raw4 = new List<(int, int)>
        {
            (20,3),(24,3),(16,4),(20,4),
            (32,3),(36,3),(36,4),(40,4),
            (20,6),(24,6),(16,7),(20,7),
            (32,6),(36,6),(36,7),(40,7)
        };
        var positions4 = raw4.Select(p => (p.Item1, p.Item2, GetRandomType(4))).ToList();
        _stages.Add(new StageData(4, 0.08f, positions4));

        // Stage 5
        var positions5 = new List<(int, int, string)>();
        for (int y = 3; y <= 9; y += 2)
            for (int x = 4; x <= 52; x += 3)
                if ((x / 4 + y) % 2 == 0)
                    positions5.Add((x, y, GetRandomType(5)));
        _stages.Add(new StageData(5, 0.075f, positions5));

        // Stage 6
        var raw6 = new List<(int, int)>
        {
            (28,2),(28,3),(24,4),(32,4),(20,5),(28,5),(36,5),(24,6),(32,6),(28,7),(28,8)
        };
        var positions6 = raw6.Select(p => (p.Item1, p.Item2, GetRandomType(6))).ToList();
        _stages.Add(new StageData(6, 0.073f, positions6));

        // Stage 7
        var raw7 = new List<(int, int)>
        {
            (4,2),(4,3),(4,4),(4,5),(8,5),
            (52,3),(52,4),(52,5),(48,5),
            (4,7),(8,7),(4,8),(4,9),
            (52,7),(48,7),(52,8),(52,9)
        };
        var positions7 = raw7.Select(p => (p.Item1, p.Item2, GetRandomType(7))).ToList();
        _stages.Add(new StageData(7, 0.071f, positions7));

        // Stage 8
        var positions8 = new List<(int, int, string)>();
        for (int x = 4; x <= 52; x += 3)
            positions8.Add((x, 5, GetRandomType(8)));
        for (int y = 2; y <= 9; y++)
            positions8.Add((28, y, GetRandomType(8)));
        _stages.Add(new StageData(8, 0.068f, positions8));

        // Stage 9
        var positions9 = new List<(int, int, string)>();
        for (int y = 3; y <= 9; y++)
        {
            int startX = (y % 2 == 0) ? 8 : 4;
            for (int x = startX; x <= 52; x += 3)
                positions9.Add((x, y, GetRandomType(9)));
        }
        _stages.Add(new StageData(9, 0.065f, positions9));

        // Stage 10
        var positions10 = new List<(int, int, string)>();
        for (int y = 2; y <= 9; y++)
            for (int x = 4; x <= 52; x += 3)
                if (!(x >= 24 && x <= 32 && y >= 4 && y <= 6))
                    positions10.Add((x, y, GetRandomType(10)));
        _stages.Add(new StageData(10, 0.06f, positions10));
    }

    public StageData GetStage(int stageNumber)
    {
        return _stages[stageNumber - 1];
    }

    public int GetTotalStages()
    {
        return _stages.Count;
    }
}