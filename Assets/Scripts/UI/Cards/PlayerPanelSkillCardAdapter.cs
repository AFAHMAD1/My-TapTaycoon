using UnityEngine;

public class PlayerPanelSkillCardAdapter : ICardDataProvider
{
    private readonly PlayerPanelSkillState skill;
    private readonly CardDisplayConfig config;

    public PlayerPanelSkillCardAdapter(PlayerPanelSkillState skill)
    {
        this.skill = skill;
        config = new CardDisplayConfig
        {
            showIcon = true,
            showProgressBar = false,
            showDescription = true,
            showIncome = false,
            showLevel = false,
            showSecondaryButton = false
        };
    }

    public string DisplayName => skill != null ? skill.GetDisplayName() : "Skill";
    public int CurrentLevel => 0;
    public string Description => skill != null ? skill.GetDescription() : "";
    public Sprite Icon => null;
    public Color CardColor => new Color32(236, 217, 196, 255);
    public Color ButtonColor => new Color32(32, 151, 220, 255);
    public Color IconTintColor => Color.white;
    public Color IncomeColor => Color.white;
    public CardDisplayConfig DisplayConfig => config;

    public double GetCost(int amount) => skill != null ? skill.cost : 0d;
    public double GetIncomePerCycle() => 0d;

    public bool CanAfford(int amount)
    {
        return skill != null &&
               !skill.purchased &&
               CurrencyManager.Instance != null &&
               CurrencyManager.Instance.currentMoney >= skill.cost;
    }

    public void Purchase(int amount)
    {
        if (UpgradeManager.Instance != null && skill != null)
        {
            UpgradeManager.Instance.TryPurchasePlayerPanelSkill(skill);
        }
    }

    public bool HasProgressBar() => false;
    public float GetProgressNormalized() => 0f;
    public string GetProgressText() => "";
    public void OnProgressClick() { }
    public string GetBuyButtonText(int amount) => "Satin Al";
}
