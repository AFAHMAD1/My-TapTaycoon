using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCustomCardData
{
    public string cardName = "New Card";
    public string description;
    public Sprite icon;

    [Min(0)]
    public int currentLevel;

    [Tooltip("0 means unlimited.")]
    [Min(0)]
    public int maxLevel;

    [Header("Economy")]
    public double baseCost = 100d;
    public float costMultiplierPerLevel = 1.15f;
    public double incomePerLevel = 10d;

    [Header("Colors")]
    public Color cardBackgroundColor = new Color32(236, 217, 196, 255);
    public Color buttonColor = new Color32(32, 151, 220, 255);
    public Color iconTintColor = Color.white;
    public Color incomeColor = new Color32(78, 154, 67, 255);
}

public class PlayerCustomCardPanel : BaseUpgradePanel
{
    [Header("Player Panel Custom Cards")]
    public List<PlayerCustomCardData> cards = new List<PlayerCustomCardData>();

    protected override List<ICardDataProvider> GetDataProviders()
    {
        List<ICardDataProvider> providers = new List<ICardDataProvider>();

        foreach (PlayerCustomCardData card in cards)
        {
            if (card != null)
            {
                providers.Add(new PlayerCustomCardAdapter(card));
            }
        }

        return providers;
    }
}

public class PlayerCustomCardAdapter : ICardDataProvider
{
    private readonly PlayerCustomCardData card;
    private readonly CardDisplayConfig displayConfig;

    public PlayerCustomCardAdapter(PlayerCustomCardData card)
    {
        this.card = card;
        displayConfig = new CardDisplayConfig
        {
            showIcon = true,
            showProgressBar = false,
            showDescription = !string.IsNullOrWhiteSpace(card.description),
            showIncome = true,
            showLevel = true,
            showSecondaryButton = false
        };
    }

    public string DisplayName => string.IsNullOrWhiteSpace(card.cardName) ? "New Card" : card.cardName;
    public int CurrentLevel => card.currentLevel;
    public string Description => card.description;

    public Sprite Icon => card.icon;
    public Color CardColor => card.cardBackgroundColor;
    public Color ButtonColor => card.buttonColor;
    public Color IconTintColor => card.iconTintColor;
    public Color IncomeColor => card.incomeColor;
    public CardDisplayConfig DisplayConfig => displayConfig;

    public double GetCost(int amount)
    {
        int purchaseCount = ResolvePurchaseCount(amount);
        if (purchaseCount <= 0)
        {
            return CurrentCost();
        }

        double total = 0d;
        for (int i = 0; i < purchaseCount; i++)
        {
            total += CostAtLevel(card.currentLevel + i);
        }

        return total;
    }

    public double GetIncomePerCycle()
    {
        int previewLevel = Mathf.Max(1, card.currentLevel);
        return card.incomePerLevel * previewLevel;
    }

    public bool CanAfford(int amount)
    {
        if (CurrencyManager.Instance == null || IsMaxLevel)
        {
            return false;
        }

        int purchaseCount = ResolvePurchaseCount(amount);
        return purchaseCount > 0 && CurrencyManager.Instance.currentMoney >= GetCost(purchaseCount);
    }

    public void Purchase(int amount)
    {
        if (CurrencyManager.Instance == null || IsMaxLevel)
        {
            return;
        }

        int purchaseCount = ResolvePurchaseCount(amount);
        if (purchaseCount <= 0)
        {
            return;
        }

        double totalCost = GetCost(purchaseCount);
        if (CurrencyManager.Instance.SpendMoney(totalCost))
        {
            card.currentLevel += purchaseCount;
        }
    }

    public bool HasProgressBar() => false;
    public float GetProgressNormalized() => 0f;
    public string GetProgressText() => string.Empty;
    public void OnProgressClick() { }

    public string GetBuyButtonText(int amount)
    {
        if (IsMaxLevel)
        {
            return "MAX";
        }

        if (amount >= int.MaxValue)
        {
            return "Satin Al MAX";
        }

        return amount > 1 ? $"Satin Al x{amount}" : "Satin Al";
    }

    private bool IsMaxLevel => card.maxLevel > 0 && card.currentLevel >= card.maxLevel;

    private double CurrentCost()
    {
        return CostAtLevel(card.currentLevel);
    }

    private double CostAtLevel(int level)
    {
        int safeLevel = Mathf.Max(0, level);
        float multiplier = Mathf.Max(1f, card.costMultiplierPerLevel);
        double cost = card.baseCost * System.Math.Pow(multiplier, safeLevel);
        return double.IsNaN(cost) || double.IsInfinity(cost) ? 0d : cost;
    }

    private int ResolvePurchaseCount(int requestedAmount)
    {
        if (IsMaxLevel)
        {
            return 0;
        }

        int remaining = card.maxLevel > 0 ? card.maxLevel - card.currentLevel : int.MaxValue;

        if (requestedAmount >= int.MaxValue)
        {
            return GetMaxAffordableCount(remaining);
        }

        return Mathf.Min(Mathf.Max(1, requestedAmount), remaining);
    }

    private int GetMaxAffordableCount(int remaining)
    {
        if (CurrencyManager.Instance == null)
        {
            return 0;
        }

        double money = CurrencyManager.Instance.currentMoney;
        int count = 0;

        while (count < remaining && count < 100000)
        {
            double nextCost = CostAtLevel(card.currentLevel + count);
            if (money < nextCost)
            {
                break;
            }

            money -= nextCost;
            count++;
        }

        return count;
    }
}
