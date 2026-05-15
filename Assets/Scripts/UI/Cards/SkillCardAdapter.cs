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
            showLevel = true,
            showSecondaryButton = false,
            useSkillCardLayout = true
        };
    }

    public string DisplayName => skill.data.entityName;
    public int CurrentLevel => skill.currentLevel;

    public string Description
    {
        get
        {
            float duration = (float)skill.data.effectDuration.Evaluate(skill.currentLevel);
            float power = (float)skill.data.effectPower.Evaluate(skill.currentLevel);
            string effectText = string.Format(skill.data.descriptionTemplate, duration, power);
            return $"<b>Cooldown:</b> <color=#E8464A>{GetCooldownText()}</color>\n<i>{effectText}</i>";
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
    public string GetBuyButtonText(int amount) => "";

    public bool HasProgressBar() => false;
    public float GetProgressNormalized() => 0f;
    public string GetProgressText() => "";

    public void OnProgressClick()
    {
        if (skill.IsReady && skill.currentLevel > 0)
        {
            skill.UseSkill();
        }
    }

    private string GetCooldownText()
    {
        int previewLevel = Mathf.Max(1, skill.currentLevel);
        float cooldownSeconds = (float)skill.data.cooldownTime.Evaluate(previewLevel);
        int minutes = Mathf.CeilToInt(cooldownSeconds / 60f);
        return $"{minutes}m";
    }
}
