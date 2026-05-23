using UnityEngine;

public sealed class FootballManagerMarketData
{
    private const double ImportantStatUpgradePriceDivisor = 6d;
    private const double NormalStatUpgradePriceDivisor = 12d;

    public readonly string Name;
    public readonly int Age;
    public readonly ManagerStats Stats;

    public double Price { get; set; }
    public string Class => Stats.managerClass.ToString();

    public FootballManagerMarketData(string name, string managerClass, int price, int age)
        : this(name, ParseManagerClass(managerClass), age)
    {
    }

    private FootballManagerMarketData(string name, ManagerClass managerClass, int age)
    {
        Name = name;
        Stats = ManagerStatGenerator.GenerateStats(managerClass, age);
        Price = ManagerPriceGenerator.GeneratePrice(Stats.managerClass);
        Age = Stats.age;
    }

    public FootballManagerMarketData WithGeneratedClass(ManagerClass managerClass)
    {
        return new FootballManagerMarketData(Name, managerClass, Age);
    }

    public double GetStatUpgradeCost(ManagerStat stat)
    {
        if (!CanUpgradeStat(stat))
        {
            return 0d;
        }

        double divisor = ManagerStatGenerator.IsImportantStat(stat)
            ? ImportantStatUpgradePriceDivisor
            : NormalStatUpgradePriceDivisor;

        return Price / divisor;
    }

    public bool TryApplyPaidStatUpgrade(ManagerStat stat, out double upgradeCost)
    {
        upgradeCost = GetStatUpgradeCost(stat);
        if (upgradeCost <= 0d)
        {
            return false;
        }

        ManagerClass oldClass = Stats.managerClass;
        ApplyStatUpgrade(stat);
        Price += upgradeCost;
        CorrectPriceForClassIfChanged(oldClass);
        return true;
    }

    public bool CanUpgradeStat(ManagerStat stat)
    {
        switch (stat)
        {
            case ManagerStat.ManagerLevel:
                return Stats.managerLevel < ManagerStatGenerator.MaxManagerLevel;
            case ManagerStat.TrainingBoost:
                return Stats.trainingBoost < ManagerStatGenerator.MaxTrainingBoost;
            case ManagerStat.TacticBoost:
                return Stats.tacticBoost < ManagerStatGenerator.MaxTacticBoost;
            case ManagerStat.Experience:
                return Stats.experience < ManagerStatGenerator.MaxExperience;
            case ManagerStat.FootballIQ:
                return Stats.footballIQ < ManagerStatGenerator.MaxFootballIQ;
            default:
                return false;
        }
    }

    public void UpgradeStat(ManagerStat stat)
    {
        if (!CanUpgradeStat(stat))
        {
            return;
        }

        ManagerClass oldClass = Stats.managerClass;
        ApplyStatUpgrade(stat);
        CorrectPriceForClassIfChanged(oldClass);
    }

    private void ApplyStatUpgrade(ManagerStat stat)
    {
        switch (stat)
        {
            case ManagerStat.ManagerLevel:
                Stats.managerLevel++;
                Stats.UpdateManagerClass();
                break;
            case ManagerStat.TrainingBoost:
                Stats.trainingBoost = Mathf.Min(Stats.trainingBoost + 1, ManagerStatGenerator.MaxTrainingBoost);
                break;
            case ManagerStat.TacticBoost:
                Stats.tacticBoost = Mathf.Min(Stats.tacticBoost + 1, ManagerStatGenerator.MaxTacticBoost);
                break;
            case ManagerStat.Experience:
                Stats.experience = Mathf.Min(Stats.experience + 1, ManagerStatGenerator.MaxExperience);
                break;
            case ManagerStat.FootballIQ:
                Stats.footballIQ = Mathf.Min(Stats.footballIQ + 1, ManagerStatGenerator.MaxFootballIQ);
                break;
        }
    }

    private void CorrectPriceForClassIfChanged(ManagerClass oldClass)
    {
        ManagerClass newClass = Stats.managerClass;
        if (newClass == oldClass)
        {
            return;
        }

        double minimumClassPrice = ManagerPriceGenerator.GetMinimumPrice(newClass);
        if (Price < minimumClassPrice)
        {
            Price += ManagerPriceGenerator.GetMiddlePrice(newClass);
        }
    }

    private static ManagerClass ParseManagerClass(string managerClass)
    {
        if (ManagerStatGenerator.TryParseClass(managerClass, out ManagerClass parsedClass))
        {
            return parsedClass;
        }

        Debug.LogWarning($"[MarketPanel] Bilinmeyen menajer sinifi '{managerClass}'. D kullanildi.");
        return ManagerClass.D;
    }
}
