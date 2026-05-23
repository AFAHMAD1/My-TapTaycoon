using System;
using UnityEngine;

public sealed class FootballPlayerMarketData
{
    public const int MaxUpgradedStat = 99;
    private const double ImportantStatUpgradePriceDivisor = 6d;
    private const double NormalStatUpgradePriceDivisor = 12d;

    public readonly string Name;
    public readonly string Type;
    public readonly FootballPlayerType PlayerType;
    public readonly int Age;
    public readonly int Height;

    public FootballPlayerClass PlayerClass { get; private set; }
    public FootballPlayerStats Stats { get; private set; }
    public double Price { get; set; }
    public string Class => PlayerClass.ToString();
    public bool CanUpgradeStat(FootballStat stat)
    {
        return Stats.IsStatBelow(stat, MaxUpgradedStat);
    }

    public double GetStatUpgradeCost(FootballStat stat)
    {
        if (!CanUpgradeStat(stat))
        {
            return 0d;
        }

        double divisor = FootballPlayerStatGenerator.IsImportantStat(PlayerType, stat)
            ? ImportantStatUpgradePriceDivisor
            : NormalStatUpgradePriceDivisor;

        return Price / divisor;
    }

    public bool TryApplyPaidStatUpgrade(FootballStat stat, out double upgradeCost)
    {
        upgradeCost = GetStatUpgradeCost(stat);
        if (upgradeCost <= 0d)
        {
            return false;
        }

        Stats = Stats.IncreaseStat(stat, MaxUpgradedStat);
        Price += upgradeCost;
        RecalculateClassAndCorrectPriceIfChanged();
        return true;
    }

    public void UpgradeStat(FootballStat stat)
    {
        if (!CanUpgradeStat(stat))
        {
            return;
        }

        Stats = Stats.IncreaseStat(stat, MaxUpgradedStat);
        RecalculateClassAndCorrectPriceIfChanged();
    }
    public FootballPlayerMarketData(string name, string type, string playerClass, int price, int age, int height)
        : this(name, type, ParsePlayerType(type), ParsePlayerClass(playerClass), age, height)
    {
    }

    private FootballPlayerMarketData(
        string name,
        string type,
        FootballPlayerType footballPlayerType,
        FootballPlayerClass footballPlayerClass,
        int age,
        int height)
    {
        Name = name;
        Type = type;
        PlayerType = footballPlayerType;
        PlayerClass = footballPlayerClass;
        Stats = FootballPlayerStatGenerator.GenerateStats(footballPlayerType, footballPlayerClass);
        Price = FootballPlayerPriceGenerator.GeneratePrice(footballPlayerClass);
        Age = age;
        Height = height;
    }

    public FootballPlayerMarketData WithGeneratedClass(FootballPlayerClass footballPlayerClass)
    {
        return new FootballPlayerMarketData(Name, Type, PlayerType, footballPlayerClass, Age, Height);
    }

    private void RecalculateClassAndCorrectPriceIfChanged()
    {
        FootballPlayerClass oldClass = PlayerClass;
        FootballPlayerClass newClass = FootballPlayerStatGenerator.CalculateClass(PlayerType, Stats);
        PlayerClass = newClass;

        if (newClass != oldClass)
        {
            CorrectPriceForClass(newClass);
        }
    }

    private void CorrectPriceForClass(FootballPlayerClass playerClass)
    {
        double minimumClassPrice = FootballPlayerPriceGenerator.GetMinimumPrice(playerClass);
        if (Price < minimumClassPrice)
        {
            Price += FootballPlayerPriceGenerator.GetMiddlePrice(playerClass);
        }
    }

    private static FootballPlayerType ParsePlayerType(string type)
    {
        if (Enum.TryParse(type, true, out FootballPlayerType footballPlayerType))
        {
            return footballPlayerType;
        }

        Debug.LogWarning($"[MarketPanel] Bilinmeyen futbolcu tipi '{type}'. Forward kullanildi.");
        return FootballPlayerType.Forward;
    }

    private static FootballPlayerClass ParsePlayerClass(string playerClass)
    {
        if (Enum.TryParse(playerClass, true, out FootballPlayerClass footballPlayerClass))
        {
            return footballPlayerClass;
        }

        Debug.LogWarning($"[MarketPanel] Bilinmeyen futbolcu sinifi '{playerClass}'. G kullanildi.");
        return FootballPlayerClass.G;
    }
}
