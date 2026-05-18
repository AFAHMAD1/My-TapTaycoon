using UnityEngine;

public class OtomaticBasUnlockCardAdapter : ICardDataProvider
{
    private readonly SkillData skillData;
    private readonly CardDisplayConfig config;

    public OtomaticBasUnlockCardAdapter(SkillData skillData)
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

    public string DisplayName => skillData != null ? skillData.entityName : "Otomatic Bas";
    public int CurrentLevel => OtomaticBasButton.CurrentLevel;
    public string Description => skillData != null && !string.IsNullOrWhiteSpace(skillData.descriptionTemplate)
        ? FormatDescription()
        : "Otomatik basar";

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

        if (CurrencyManager.Instance.SpendMoney(GetNextLevelCost()))
        {
            OtomaticBasButton.UpgradeLevel();
            Debug.Log($"[Otomatic Bas] Level {OtomaticBasButton.CurrentLevel} unlocked.");
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

        return OtomaticBasButton.IsUnlocked ? "Yukselt" : "Satin Al";
    }

    private string FormatDescription()
    {
        int previewLevel = Mathf.Max(1, OtomaticBasButton.CurrentLevel);
        float duration = skillData != null ? (float)skillData.effectDuration.Evaluate(previewLevel - 1) : 30f;
        float tapsPerSecond = skillData != null ? (float)skillData.effectPower.Evaluate(previewLevel - 1) : 2f;
        return string.Format(skillData.descriptionTemplate, duration, tapsPerSecond);
    }

    private double GetNextLevelCost()
    {
        return skillData != null ? skillData.EvaluateCost(OtomaticBasButton.CurrentLevel) : 10000d;
    }

    private bool IsMaxLevel()
    {
        int maxLevel = skillData != null && skillData.maxLevel > 0 ? skillData.maxLevel : OtomaticBasButton.MaxLevel;
        return OtomaticBasButton.CurrentLevel >= maxLevel;
    }
}
