using UnityEngine;

public class SkillCardAdapter : ICardDataProvider
{
    private SkillEntity skill;
    private CardDisplayConfig config;

    public SkillCardAdapter(SkillEntity skill)
    {
        this.skill = skill;
        this.config = new CardDisplayConfig
        {
            showIcon = true,
            showProgressBar = true, // Cooldown için kullanılacak
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
            // "{0} Saniyeliğine {1}x işletme kârı" gibi bir metni formatlıyoruz
            float duration = (float)skill.data.effectDuration.Evaluate(skill.currentLevel);
            float power = (float)skill.data.effectPower.Evaluate(skill.currentLevel);
            return string.Format(skill.data.descriptionTemplate, duration, power);
        }
    }

    public Sprite Icon => skill.data.icon;
    public Color CardColor => skill.data.cardBackgroundColor;
    public Color ButtonColor => Color.white; // Sağ taraf olmayacağı için önemsiz
    public Color IconTintColor => Color.white;
    public Color IncomeColor => Color.white;

    public CardDisplayConfig DisplayConfig => config;

    // Sağdaki butonları (Satın Al vs) UI'dan sildiğimiz için bu kısımlar tetiklenmeyecek
    // Ama kartın kendi üstüne tıklandığında OnProgressClick'i kullanabiliriz
    public double GetCost(int amount) => skill.CurrentCost();
    public double GetIncomePerCycle() => 0d;
    public bool CanAfford(int amount) => skill.CanAffordUpgradeAmount(amount);
    public void Purchase(int amount) { } 
    public string GetBuyButtonText(int amount) => "";

    public bool HasProgressBar() => true;

    public float GetProgressNormalized()
    {
        if (skill.currentLevel == 0) return 0f;
        
        float maxCooldown = (float)skill.data.cooldownTime.Evaluate(skill.currentLevel);
        if (maxCooldown <= 0) return 1f;

        // Bar doluluğu: (Max - Güncel) / Max. Bekleme bitince bar tam dolu görünür.
        return (maxCooldown - skill.currentCooldownTimer) / maxCooldown;
    }

    public string GetProgressText()
    {
        if (skill.currentLevel == 0) 
        {
            // Resimdeki gibi Lv.0 olsa bile bekleme süresini (Level 1 halini) gösteriyoruz.
            float previewCooldown = (float)skill.data.cooldownTime.Evaluate(1);
            return $"Bekleme: {Mathf.CeilToInt(previewCooldown / 60f)}m"; 
        }

        if (skill.IsActive) return $"AKTİF: {Mathf.CeilToInt(skill.currentActiveTimer)}s";
        if (!skill.IsReady) return $"Bekleme: {Mathf.CeilToInt(skill.currentCooldownTimer / 60f)}m"; // Dakika olarak gösterelim
        
        return "KULLANIMA HAZIR!";
    }

    public void OnProgressClick()
    {
        // Satın alma / level atlama işlemleri artık Kit panelinden yapılacağı için
        // burada SADECE eğer beceri açıksa (Level > 0) ve hazırsa kullanma işlemi yapıyoruz.
        if (skill.IsReady && skill.currentLevel > 0)
        {
            skill.UseSkill();
        }
    }
}
