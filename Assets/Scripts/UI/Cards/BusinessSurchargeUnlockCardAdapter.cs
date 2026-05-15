using UnityEngine;

public class BusinessSurchargeUnlockCardAdapter : ICardDataProvider
{
    private readonly SkillData skillData;
    private readonly CardDisplayConfig config;

    public BusinessSurchargeUnlockCardAdapter(SkillData skillData)
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

    public string DisplayName => skillData != null ? skillData.entityName : "Business Surcharge";
    public int CurrentLevel => BusinessSurchargeButton.CurrentLevel;
    public string Description => skillData != null && !string.IsNullOrWhiteSpace(skillData.descriptionTemplate)
        ? skillData.descriptionTemplate
        : "Varlığını kat kat";

    public Sprite Icon => null;
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

        if (CurrencyManager.Instance.SpendMoney(GetNextLevelCost()))
        {
            BusinessSurchargeButton.UpgradeLevel();
            Debug.Log($"[Business Surcharge] Level {BusinessSurchargeButton.CurrentLevel} unlocked.");
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

        return BusinessSurchargeButton.IsUnlocked ? "Yukselt" : "Satin Al";
    }

    private double GetNextLevelCost()
    {
        return skillData != null ? skillData.EvaluateCost(BusinessSurchargeButton.CurrentLevel) : 1000d;
    }

    private bool IsMaxLevel()
    {
        int maxLevel = skillData != null && skillData.maxLevel > 0 ? skillData.maxLevel : BusinessSurchargeButton.MaxLevel;
        return BusinessSurchargeButton.CurrentLevel >= maxLevel;
    }
}
