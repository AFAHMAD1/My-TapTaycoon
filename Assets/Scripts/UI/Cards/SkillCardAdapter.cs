using UnityEngine;

public class SkillCardAdapter : ICardDataProvider
{
    private readonly SkillEntity skill;
    private readonly CardDisplayConfig config;

    public SkillCardAdapter(SkillEntity skill)
    {
        this.skill = skill;
        config = new CardDisplayConfig
        {
            showIcon = true,
            showProgressBar = false,
            showDescription = true,
            showIncome = false,
            showLevel = false,
            showSecondaryButton = false,
            useSkillCardLayout = true
        };
    }

    public string DisplayName => skill.data.entityName;
    public int CurrentLevel => GetEffectiveLevel();

    public string Description
    {
        get
        {
            int previewLevel = Mathf.Max(1, GetEffectiveLevel());
            int valueLevel = Mathf.Max(0, previewLevel - 1);
            float duration = (float)skill.data.effectDuration.Evaluate(valueLevel);
            float power = (float)skill.data.effectPower.Evaluate(valueLevel);
            string effectText = string.Format(skill.data.descriptionTemplate, duration, power);
            return effectText;
        }
    }

    public Sprite Icon => skill.data.icon;
    public Color CardColor => new Color32(235, 220, 176, 255);
    public Color ButtonColor => Color.white;
    public Color IconTintColor => Color.white;
    public Color IncomeColor => Color.white;

    public CardDisplayConfig DisplayConfig => config;

    public double GetCost(int amount) => skill.CurrentCost();
    public double GetIncomePerCycle() => 0d;
    public bool CanAfford(int amount) => skill.CanAffordUpgradeAmount(amount);
    public void Purchase(int amount) { }
    public string GetBuyButtonText(int amount)
    {
        return IsUnlocked() ? "Satin Alindi" : "Satin Alinmadi";
    }

    public bool HasProgressBar() => false;
    public float GetProgressNormalized() => 0f;
    public string GetProgressText()
    {
        if (skill.currentCooldownTimer > 0f)
        {
            return $"Bekleme Suresi: {FormatSeconds(skill.currentCooldownTimer)}";
        }

        if (skill.data != null && skill.data.skillId == SkillId.OtomaticBas)
        {
            return $"Sure: {GetDurationText()}";
        }

        return $"Bekleme Suresi: {GetCooldownText()}";
    }

    public void OnProgressClick()
    {
        if (skill.IsReady && skill.currentLevel > 0)
        {
            skill.UseSkill();
        }
    }

    private string GetCooldownText()
    {
        int previewLevel = Mathf.Max(1, GetEffectiveLevel());
        int valueLevel = Mathf.Max(0, previewLevel - 1);
        float cooldownSeconds = (float)skill.data.cooldownTime.Evaluate(valueLevel);
        return FormatSeconds(cooldownSeconds);
    }

    private string GetDurationText()
    {
        int previewLevel = Mathf.Max(1, GetEffectiveLevel());
        int valueLevel = Mathf.Max(0, previewLevel - 1);
        float durationSeconds = (float)skill.data.effectDuration.Evaluate(valueLevel);
        return FormatSeconds(durationSeconds);
    }

    private bool IsUnlocked()
    {
        if (skill?.data == null)
        {
            return false;
        }

        switch (skill.data.skillId)
        {
            case SkillId.QuickCash:
                return QuickCashButton.IsUnlocked;
            case SkillId.BusinessSurcharge:
                return BusinessSurchargeButton.IsUnlocked;
            case SkillId.OtomaticBas:
                return OtomaticBasButton.IsUnlocked;
            case SkillId.HandOfMidas:
                return HandOfMidasButton.IsUnlocked;
            default:
                return skill.currentLevel > 0;
        }
    }

    private int GetEffectiveLevel()
    {
        if (skill?.data == null)
        {
            return 0;
        }

        switch (skill.data.skillId)
        {
            case SkillId.QuickCash:
                return QuickCashButton.CurrentLevel;
            case SkillId.BusinessSurcharge:
                return BusinessSurchargeButton.CurrentLevel;
            case SkillId.OtomaticBas:
                return OtomaticBasButton.CurrentLevel;
            case SkillId.HandOfMidas:
                return HandOfMidasButton.CurrentLevel;
            default:
                return skill.currentLevel;
        }
    }

    private string FormatSeconds(float secondsValue)
    {
        if (secondsValue < 60f)
        {
            return $"{Mathf.CeilToInt(secondsValue)}s";
        }

        int minutes = Mathf.FloorToInt(secondsValue / 60f);
        int seconds = Mathf.CeilToInt(secondsValue % 60f);
        return seconds > 0 ? $"{minutes}m {seconds}s" : $"{minutes}m";
    }
}
