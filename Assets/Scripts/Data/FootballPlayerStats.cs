using System;
using System.Collections.Generic;
using UnityEngine;

public enum FootballPlayerType
{
    Forward,
    Midfielder,
    Defender,
    Goalkeeper
}

public enum FootballPlayerClass
{
    G,
    F,
    E,
    D,
    C,
    B,
    A
}

public enum FootballStat
{
    Speed,
    ShootPower,
    Strength,
    PassAccuracy,
    DefensiveAbility,
    BallControl
}

public enum FootballStatImportance
{
    Main,
    Secondary,
    Weak
}

[Serializable]
public struct FootballPlayerStats
{
    [Range(1, 100)] public int speed;
    [Range(1, 100)] public int shootPower;
    [Range(1, 100)] public int strength;
    [Range(1, 100)] public int passAccuracy;
    [Range(1, 100)] public int defensiveAbility;
    [Range(1, 100)] public int ballControl;

    public FootballPlayerStats(
        int speed,
        int shootPower,
        int strength,
        int passAccuracy,
        int defensiveAbility)
        : this(speed, shootPower, strength, passAccuracy, defensiveAbility, Mathf.RoundToInt((speed + passAccuracy) / 2f))
    {
    }

    public FootballPlayerStats(
        int speed,
        int shootPower,
        int strength,
        int passAccuracy,
        int defensiveAbility,
        int ballControl)
    {
        this.speed = ClampStat(speed);
        this.shootPower = ClampStat(shootPower);
        this.strength = ClampStat(strength);
        this.passAccuracy = ClampStat(passAccuracy);
        this.defensiveAbility = ClampStat(defensiveAbility);
        this.ballControl = ClampStat(ballControl);
    }

    public int GetValue(FootballStat stat)
    {
        switch (stat)
        {
            case FootballStat.Speed:
                return speed;
            case FootballStat.ShootPower:
                return shootPower;
            case FootballStat.Strength:
                return strength;
            case FootballStat.PassAccuracy:
                return passAccuracy;
            case FootballStat.DefensiveAbility:
                return defensiveAbility;
            case FootballStat.BallControl:
                return ballControl;
            default:
                return 1;
        }
    }

    public bool HasStatBelow(int maximumValue)
    {
        int safeMaximum = ClampStat(maximumValue);
        return speed < safeMaximum ||
               shootPower < safeMaximum ||
               strength < safeMaximum ||
               passAccuracy < safeMaximum ||
               defensiveAbility < safeMaximum ||
               ballControl < safeMaximum;
    }

    public bool IsStatBelow(FootballStat stat, int maximumValue)
    {
        int safeMaximum = ClampStat(maximumValue);
        switch (stat)
        {
            case FootballStat.Speed:
                return speed < safeMaximum;
            case FootballStat.ShootPower:
                return shootPower < safeMaximum;
            case FootballStat.Strength:
                return strength < safeMaximum;
            case FootballStat.PassAccuracy:
                return passAccuracy < safeMaximum;
            case FootballStat.DefensiveAbility:
                return defensiveAbility < safeMaximum;
            case FootballStat.BallControl:
                return ballControl < safeMaximum;
            default:
                return false;
        }
    }

    public FootballPlayerStats IncreaseStat(FootballStat stat, int maximumValue)
    {
        int safeMaximum = ClampStat(maximumValue);
        return new FootballPlayerStats(
            stat == FootballStat.Speed ? IncreaseStat(speed, safeMaximum) : speed,
            stat == FootballStat.ShootPower ? IncreaseStat(shootPower, safeMaximum) : shootPower,
            stat == FootballStat.Strength ? IncreaseStat(strength, safeMaximum) : strength,
            stat == FootballStat.PassAccuracy ? IncreaseStat(passAccuracy, safeMaximum) : passAccuracy,
            stat == FootballStat.DefensiveAbility ? IncreaseStat(defensiveAbility, safeMaximum) : defensiveAbility,
            stat == FootballStat.BallControl ? IncreaseStat(ballControl, safeMaximum) : ballControl);
    }

    public FootballPlayerStats IncreaseAllBelow(int maximumValue)
    {
        int safeMaximum = ClampStat(maximumValue);
        return new FootballPlayerStats(
            IncreaseStat(speed, safeMaximum),
            IncreaseStat(shootPower, safeMaximum),
            IncreaseStat(strength, safeMaximum),
            IncreaseStat(passAccuracy, safeMaximum),
            IncreaseStat(defensiveAbility, safeMaximum),
            IncreaseStat(ballControl, safeMaximum));
    }

    private static int IncreaseStat(int currentValue, int maximumValue)
    {
        int increasedValue = currentValue < maximumValue ? currentValue + 1 : currentValue;
        return Mathf.Clamp(increasedValue, FootballPlayerStatGenerator.MinStatValue, maximumValue);
    }

    private static int ClampStat(int value)
    {
        return Mathf.Clamp(value, FootballPlayerStatGenerator.MinStatValue, FootballPlayerStatGenerator.MaxStatValue);
    }
}

public static class FootballPlayerStatGenerator
{
    public const int MinStatValue = 1;
    public const int MaxStatValue = 100;

    private const int SecondaryStatOffset = 10;
    private const int WeakStatOffset = 25;

    private static readonly Dictionary<FootballPlayerClass, Vector2Int> ClassRanges =
        new Dictionary<FootballPlayerClass, Vector2Int>
        {
            { FootballPlayerClass.G, new Vector2Int(20, 40) },
            { FootballPlayerClass.F, new Vector2Int(40, 50) },
            { FootballPlayerClass.E, new Vector2Int(50, 60) },
            { FootballPlayerClass.D, new Vector2Int(60, 65) },
            { FootballPlayerClass.C, new Vector2Int(65, 75) },
            { FootballPlayerClass.B, new Vector2Int(75, 85) },
            { FootballPlayerClass.A, new Vector2Int(85, 100) }
        };

    private static readonly Dictionary<FootballPlayerType, Dictionary<FootballStat, FootballStatImportance>> StatImportanceByType =
        new Dictionary<FootballPlayerType, Dictionary<FootballStat, FootballStatImportance>>
        {
            {
                FootballPlayerType.Forward,
                CreateImportanceMap(
                    FootballStatImportance.Main,
                    FootballStatImportance.Main,
                    FootballStatImportance.Main,
                    FootballStatImportance.Secondary,
                    FootballStatImportance.Weak,
                    FootballStatImportance.Secondary)
            },
            {
                FootballPlayerType.Midfielder,
                CreateImportanceMap(
                    FootballStatImportance.Main,
                    FootballStatImportance.Secondary,
                    FootballStatImportance.Main,
                    FootballStatImportance.Main,
                    FootballStatImportance.Weak,
                    FootballStatImportance.Main)
            },
            {
                FootballPlayerType.Defender,
                CreateImportanceMap(
                    FootballStatImportance.Main,
                    FootballStatImportance.Weak,
                    FootballStatImportance.Main,
                    FootballStatImportance.Secondary,
                    FootballStatImportance.Main,
                    FootballStatImportance.Secondary)
            },
            {
                FootballPlayerType.Goalkeeper,
                CreateImportanceMap(
                    FootballStatImportance.Weak,
                    FootballStatImportance.Weak,
                    FootballStatImportance.Main,
                    FootballStatImportance.Secondary,
                    FootballStatImportance.Main,
                    FootballStatImportance.Weak)
            }
        };

    public static FootballPlayerStats GenerateStats(FootballPlayerType playerType, FootballPlayerClass playerClass)
    {
        return new FootballPlayerStats(
            GenerateStat(playerType, playerClass, FootballStat.Speed),
            GenerateStat(playerType, playerClass, FootballStat.ShootPower),
            GenerateStat(playerType, playerClass, FootballStat.Strength),
            GenerateStat(playerType, playerClass, FootballStat.PassAccuracy),
            GenerateStat(playerType, playerClass, FootballStat.DefensiveAbility),
            GenerateStat(playerType, playerClass, FootballStat.BallControl));
    }

    public static Vector2Int GetClassRange(FootballPlayerClass playerClass)
    {
        return ClassRanges[playerClass];
    }

    public static Vector2Int GetStatRange(FootballPlayerClass playerClass, FootballStatImportance importance)
    {
        Vector2Int classRange = GetClassRange(playerClass);
        int offset = GetImportanceOffset(importance);
        int minValue = Mathf.Clamp(classRange.x - offset, MinStatValue, MaxStatValue);
        int maxValue = Mathf.Clamp(classRange.y - offset, MinStatValue, MaxStatValue);

        return new Vector2Int(minValue, maxValue);
    }

    public static FootballStatImportance GetStatImportance(FootballPlayerType playerType, FootballStat stat)
    {
        return StatImportanceByType[playerType][stat];
    }

    public static bool IsImportantStat(FootballPlayerType playerType, FootballStat stat)
    {
        switch (playerType)
        {
            case FootballPlayerType.Forward:
                return stat == FootballStat.Speed ||
                       stat == FootballStat.ShootPower ||
                       stat == FootballStat.Strength;
            case FootballPlayerType.Midfielder:
                return stat == FootballStat.PassAccuracy ||
                       stat == FootballStat.Strength ||
                       stat == FootballStat.Speed;
            case FootballPlayerType.Defender:
                return stat == FootballStat.DefensiveAbility ||
                       stat == FootballStat.Strength ||
                       stat == FootballStat.Speed;
            case FootballPlayerType.Goalkeeper:
                return stat == FootballStat.DefensiveAbility ||
                       stat == FootballStat.Strength;
            default:
                return false;
        }
    }

    public static float CalculateOverallScore(FootballPlayerType playerType, FootballPlayerStats stats)
    {
        switch (playerType)
        {
            case FootballPlayerType.Forward:
                return Average(stats.speed, stats.shootPower, stats.strength);
            case FootballPlayerType.Midfielder:
                return Average(stats.passAccuracy, stats.strength, stats.speed);
            case FootballPlayerType.Defender:
                return Average(stats.defensiveAbility, stats.strength, stats.speed);
            case FootballPlayerType.Goalkeeper:
                return Average(stats.defensiveAbility, stats.strength);
            default:
                return MinStatValue;
        }
    }

    public static FootballPlayerClass CalculateClass(FootballPlayerType playerType, FootballPlayerStats stats)
    {
        return CalculateClassFromScore(CalculateOverallScore(playerType, stats));
    }

    public static FootballPlayerClass CalculateClassFromScore(float overallScore)
    {
        float safeScore = Mathf.Clamp(overallScore, MinStatValue, MaxStatValue);

        if (safeScore >= 85f)
        {
            return FootballPlayerClass.A;
        }

        if (safeScore >= 75f)
        {
            return FootballPlayerClass.B;
        }

        if (safeScore >= 65f)
        {
            return FootballPlayerClass.C;
        }

        if (safeScore >= 60f)
        {
            return FootballPlayerClass.D;
        }

        if (safeScore >= 50f)
        {
            return FootballPlayerClass.E;
        }

        if (safeScore >= 40f)
        {
            return FootballPlayerClass.F;
        }

        return FootballPlayerClass.G;
    }

    private static int GenerateStat(FootballPlayerType playerType, FootballPlayerClass playerClass, FootballStat stat)
    {
        Vector2Int range = GetStatRange(playerClass, GetStatImportance(playerType, stat));
        return UnityEngine.Random.Range(range.x, range.y + 1);
    }

    private static int GetImportanceOffset(FootballStatImportance importance)
    {
        switch (importance)
        {
            case FootballStatImportance.Secondary:
                return SecondaryStatOffset;
            case FootballStatImportance.Weak:
                return WeakStatOffset;
            default:
                return 0;
        }
    }

    private static Dictionary<FootballStat, FootballStatImportance> CreateImportanceMap(
        FootballStatImportance speed,
        FootballStatImportance shootPower,
        FootballStatImportance strength,
        FootballStatImportance passAccuracy,
        FootballStatImportance defensiveAbility,
        FootballStatImportance ballControl)
    {
        return new Dictionary<FootballStat, FootballStatImportance>
        {
            { FootballStat.Speed, speed },
            { FootballStat.ShootPower, shootPower },
            { FootballStat.Strength, strength },
            { FootballStat.PassAccuracy, passAccuracy },
            { FootballStat.DefensiveAbility, defensiveAbility },
            { FootballStat.BallControl, ballControl }
        };
    }

    private static float Average(int first, int second)
    {
        return (first + second) / 2f;
    }

    private static float Average(int first, int second, int third)
    {
        return (first + second + third) / 3f;
    }
}

public static class FootballMarketClassGenerator
{
    private static readonly FootballPlayerClass[] LowerMarketClasses =
    {
        FootballPlayerClass.G,
        FootballPlayerClass.F,
        FootballPlayerClass.E,
        FootballPlayerClass.D
    };

    public static List<FootballPlayerClass> CreateClassPlan(int footballPlayerCount)
    {
        return CreateClassPlan(footballPlayerCount, 2, 3, 5);
    }

    public static List<FootballPlayerClass> CreateGoalkeeperClassPlan(int goalkeeperCount)
    {
        return CreateClassPlan(goalkeeperCount, 1, 1, 2);
    }

    private static List<FootballPlayerClass> CreateClassPlan(
        int footballPlayerCount,
        int classACount,
        int classBCount,
        int classCCount)
    {
        int safeCount = Mathf.Max(0, footballPlayerCount);
        List<FootballPlayerClass> classPlan = new List<FootballPlayerClass>(safeCount);

        AddCopies(classPlan, safeCount, FootballPlayerClass.A, classACount);
        AddCopies(classPlan, safeCount, FootballPlayerClass.B, classBCount);
        AddCopies(classPlan, safeCount, FootballPlayerClass.C, classCCount);

        while (classPlan.Count < safeCount)
        {
            classPlan.Add(LowerMarketClasses[UnityEngine.Random.Range(0, LowerMarketClasses.Length)]);
        }

        Shuffle(classPlan);
        return classPlan;
    }

    private static void AddCopies(
        List<FootballPlayerClass> classPlan,
        int maxCount,
        FootballPlayerClass playerClass,
        int copyCount)
    {
        for (int i = 0; i < copyCount && classPlan.Count < maxCount; i++)
        {
            classPlan.Add(playerClass);
        }
    }

    private static void Shuffle(IList<FootballPlayerClass> classPlan)
    {
        for (int i = classPlan.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            FootballPlayerClass current = classPlan[i];
            classPlan[i] = classPlan[randomIndex];
            classPlan[randomIndex] = current;
        }
    }
}

public static class FootballPlayerPriceGenerator
{
    private static readonly Dictionary<FootballPlayerClass, PriceRange> PriceRanges =
        new Dictionary<FootballPlayerClass, PriceRange>
        {
            { FootballPlayerClass.G, new PriceRange(10000d, 50000d) },
            { FootballPlayerClass.F, new PriceRange(50000d, 150000d) },
            { FootballPlayerClass.E, new PriceRange(200000d, 1000000d) },
            { FootballPlayerClass.D, new PriceRange(1000000d, 100000000d) },
            { FootballPlayerClass.C, new PriceRange(100000000d, 1000000000d) },
            { FootballPlayerClass.B, new PriceRange(1000000000d, 1000000000000d) },
            { FootballPlayerClass.A, new PriceRange(1000000000000d, 1e21d) }
        };

    public static double GeneratePrice(FootballPlayerClass playerClass)
    {
        PriceRange priceRange = PriceRanges[playerClass];
        double randomPrice = priceRange.Min + ((priceRange.Max - priceRange.Min) * UnityEngine.Random.value);
        return Math.Floor(randomPrice);
    }

    public static double GetMinimumPrice(FootballPlayerClass playerClass)
    {
        return PriceRanges[playerClass].Min;
    }

    public static double GetMiddlePrice(FootballPlayerClass playerClass)
    {
        PriceRange priceRange = PriceRanges[playerClass];
        return (priceRange.Min + priceRange.Max) / 2d;
    }

    private readonly struct PriceRange
    {
        public readonly double Min;
        public readonly double Max;

        public PriceRange(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }
}
