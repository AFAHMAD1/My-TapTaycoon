using System;
using System.Collections.Generic;
using UnityEngine;

public enum ManagerClass
{
    D,
    C,
    B,
    A
}

public enum ManagerStat
{
    ManagerLevel,
    TrainingBoost,
    TacticBoost,
    Experience,
    FootballIQ
}

[Serializable]
public class ManagerStats
{
    public ManagerClass managerClass;
    [Min(1)] public int managerLevel;
    [Min(1)] public int trainingBoost;
    [Min(1)] public int tacticBoost;
    [Range(1, 100)] public int experience;
    [Range(30, 70)] public int age;
    [Range(1, 100)] public int footballIQ;

    public void UpdateManagerClass()
    {
        managerLevel = Mathf.Max(1, managerLevel);
        managerClass = ManagerStatGenerator.CalculateClass(managerLevel);
    }

    public void UpgradeOneLevel()
    {
        managerLevel++;
        trainingBoost = Mathf.Min(trainingBoost + 1, ManagerStatGenerator.MaxTrainingBoost);
        tacticBoost = Mathf.Min(tacticBoost + 1, ManagerStatGenerator.MaxTacticBoost);
        experience = Mathf.Min(experience + 1, ManagerStatGenerator.MaxExperience);
        footballIQ = Mathf.Min(footballIQ + 1, ManagerStatGenerator.MaxFootballIQ);
        UpdateManagerClass();
    }
}

public static class ManagerStatGenerator
{
    public const int MaxManagerLevel = 100;
    public const int MaxTrainingBoost = 35;
    public const int MaxTacticBoost = 35;
    public const int MaxExperience = 100;
    public const int MaxFootballIQ = 100;

    private static readonly Dictionary<ManagerClass, ManagerStatRange> StatRanges =
        new Dictionary<ManagerClass, ManagerStatRange>
        {
            {
                ManagerClass.D,
                new ManagerStatRange(
                    new Vector2Int(1, 9),
                    new Vector2Int(1, 5),
                    new Vector2Int(1, 5),
                    new Vector2Int(5, 25),
                    new Vector2Int(30, 45),
                    new Vector2Int(20, 40))
            },
            {
                ManagerClass.C,
                new ManagerStatRange(
                    new Vector2Int(10, 29),
                    new Vector2Int(6, 12),
                    new Vector2Int(6, 12),
                    new Vector2Int(20, 45),
                    new Vector2Int(35, 55),
                    new Vector2Int(40, 60))
            },
            {
                ManagerClass.B,
                new ManagerStatRange(
                    new Vector2Int(30, 59),
                    new Vector2Int(13, 22),
                    new Vector2Int(13, 22),
                    new Vector2Int(40, 70),
                    new Vector2Int(40, 65),
                    new Vector2Int(60, 80))
            },
            {
                ManagerClass.A,
                new ManagerStatRange(
                    new Vector2Int(60, 100),
                    new Vector2Int(23, 35),
                    new Vector2Int(23, 35),
                    new Vector2Int(65, 100),
                    new Vector2Int(45, 70),
                    new Vector2Int(80, 100))
            }
        };

    public static ManagerStats GenerateStats(ManagerClass managerClass, int existingAge = 0)
    {
        ManagerStatRange range = StatRanges[managerClass];
        ManagerStats stats = new ManagerStats
        {
            managerLevel = RandomInRange(range.ManagerLevel),
            trainingBoost = RandomInRange(range.TrainingBoost),
            tacticBoost = RandomInRange(range.TacticBoost),
            experience = RandomInRange(range.Experience),
            age = existingAge > 0
                ? Mathf.Clamp(existingAge, range.Age.x, range.Age.y)
                : RandomInRange(range.Age),
            footballIQ = RandomInRange(range.FootballIQ)
        };

        stats.UpdateManagerClass();
        return stats;
    }

    public static ManagerClass CalculateClass(int managerLevel)
    {
        if (managerLevel >= 60)
        {
            return ManagerClass.A;
        }

        if (managerLevel >= 30)
        {
            return ManagerClass.B;
        }

        if (managerLevel >= 10)
        {
            return ManagerClass.C;
        }

        return ManagerClass.D;
    }

    public static bool TryParseClass(string classText, out ManagerClass managerClass)
    {
        return Enum.TryParse(classText, true, out managerClass);
    }

    public static bool IsImportantStat(ManagerStat stat)
    {
        return stat == ManagerStat.ManagerLevel ||
               stat == ManagerStat.TacticBoost ||
               stat == ManagerStat.Experience;
    }

    private static int RandomInRange(Vector2Int range)
    {
        return UnityEngine.Random.Range(range.x, range.y + 1);
    }

    private readonly struct ManagerStatRange
    {
        public readonly Vector2Int ManagerLevel;
        public readonly Vector2Int TrainingBoost;
        public readonly Vector2Int TacticBoost;
        public readonly Vector2Int Experience;
        public readonly Vector2Int Age;
        public readonly Vector2Int FootballIQ;

        public ManagerStatRange(
            Vector2Int managerLevel,
            Vector2Int trainingBoost,
            Vector2Int tacticBoost,
            Vector2Int experience,
            Vector2Int age,
            Vector2Int footballIQ)
        {
            ManagerLevel = managerLevel;
            TrainingBoost = trainingBoost;
            TacticBoost = tacticBoost;
            Experience = experience;
            Age = age;
            FootballIQ = footballIQ;
        }
    }
}

public static class ManagerMarketClassGenerator
{
    private static readonly ManagerClass[] RegularMarketClasses =
    {
        ManagerClass.D,
        ManagerClass.C
    };

    public static List<ManagerClass> CreateClassPlan(int managerCount)
    {
        int safeCount = Mathf.Max(0, managerCount);
        List<ManagerClass> classPlan = new List<ManagerClass>(safeCount);

        AddCopies(classPlan, safeCount, ManagerClass.A, 1);
        AddCopies(classPlan, safeCount, ManagerClass.B, 2);

        while (classPlan.Count < safeCount)
        {
            classPlan.Add(RegularMarketClasses[UnityEngine.Random.Range(0, RegularMarketClasses.Length)]);
        }

        Shuffle(classPlan);
        return classPlan;
    }

    private static void AddCopies(List<ManagerClass> classPlan, int maxCount, ManagerClass managerClass, int copyCount)
    {
        for (int i = 0; i < copyCount && classPlan.Count < maxCount; i++)
        {
            classPlan.Add(managerClass);
        }
    }

    private static void Shuffle(IList<ManagerClass> classPlan)
    {
        for (int i = classPlan.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            ManagerClass current = classPlan[i];
            classPlan[i] = classPlan[randomIndex];
            classPlan[randomIndex] = current;
        }
    }
}

public static class ManagerPriceGenerator
{
    private static readonly Dictionary<ManagerClass, PriceRange> PriceRanges =
        new Dictionary<ManagerClass, PriceRange>
        {
            { ManagerClass.D, new PriceRange(100000d, 1000000d) },
            { ManagerClass.C, new PriceRange(1000000d, 1000000000d) },
            { ManagerClass.B, new PriceRange(1000000000d, 1e15d) },
            { ManagerClass.A, new PriceRange(1e15d, 1e21d) }
        };

    public static double GeneratePrice(ManagerClass managerClass)
    {
        PriceRange priceRange = PriceRanges[managerClass];
        double randomPrice = priceRange.Min + ((priceRange.Max - priceRange.Min) * UnityEngine.Random.value);
        return Math.Floor(randomPrice);
    }

    public static double GetMinimumPrice(ManagerClass managerClass)
    {
        return PriceRanges[managerClass].Min;
    }

    public static double GetMiddlePrice(ManagerClass managerClass)
    {
        PriceRange priceRange = PriceRanges[managerClass];
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
