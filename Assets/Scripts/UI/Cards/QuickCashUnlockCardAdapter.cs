using UnityEngine;

public class QuickCashUnlockCardAdapter : ICardDataProvider
{
    private readonly SkillData skillData;
    private readonly CardDisplayConfig config;

    public QuickCashUnlockCardAdapter(SkillData skillData)
    {
        this.skillData = skillData;
        config = new CardDisplayConfig
        {
            showIcon = true,
            showProgressBar = false,
            showDescription = true,
            showIncome = false,
            showLevel = true,
            showSecondaryButton = false
        };
    }

    public string DisplayName => skillData != null ? skillData.entityName : "Quick Cash";
    public int CurrentLevel => QuickCashButton.CurrentLevel;
    public string Description => skillData != null && !string.IsNullOrWhiteSpace(skillData.descriptionTemplate)
        ? skillData.descriptionTemplate
        : "Hemen para al";

    public Sprite Icon => skillData != null ? skillData.icon : null;
    public Color CardColor => skillData != null ? skillData.cardBackgroundColor : new Color32(245, 240, 225, 255);
    public Color ButtonColor => new Color32(32, 151, 220, 255);
    public Color IconTintColor => Color.white;
    public Color IncomeColor => Color.white;
    public CardDisplayConfig DisplayConfig => config;

    public double GetCost(int amount)
    {
        return IsMaxLevel() ? 0d : GetNextLevelCost();
    }

    public double GetIncomePerCycle() => 0d;

    public bool CanAfford(int amount)
    {
        return !IsMaxLevel()
            && CurrencyManager.Instance != null
            && CurrencyManager.Instance.currentMoney >= GetNextLevelCost();
    }

    public void Purchase(int amount)
    {
        if (IsMaxLevel() || CurrencyManager.Instance == null)
        {
            return;
        }

        double cost = GetNextLevelCost();
        if (CurrencyManager.Instance.SpendMoney(cost))
        {
            QuickCashButton.UpgradeLevel();
            Debug.Log($"[QuickCash] Level {QuickCashButton.CurrentLevel} unlocked.");
        }
    }

    public bool HasProgressBar() => false;
    public float GetProgressNormalized() => 0f;
    public string GetProgressText() => string.Empty;
    public void OnProgressClick() { }

    public string GetBuyButtonText(int amount)
    {
        if (IsMaxLevel())
        {
            return "Max Level";
        }

        return QuickCashButton.IsUnlocked ? "Yukselt" : "Satin Al";
    }

    private double GetNextLevelCost()
    {
        return skillData != null ? skillData.EvaluateCost(QuickCashButton.CurrentLevel) : 0d;
    }

    private bool IsMaxLevel()
    {
        int maxLevel = skillData != null && skillData.maxLevel > 0 ? skillData.maxLevel : QuickCashButton.MaxLevel;
        return QuickCashButton.CurrentLevel >= maxLevel;
    }
}
