using UnityEngine;

[System.Serializable]
public class SkillEntity : UpgradableEntity
{
    public SkillData data;
    public override UpgradableEntityData BaseData => data;

    public float currentCooldownTimer = 0f;
    public float currentActiveTimer = 0f;

    public bool IsReady => currentCooldownTimer <= 0f;
    public bool IsActive => currentActiveTimer > 0f;

    public SkillEntity(SkillData data)
    {
        this.data = data;
        this.currentLevel = 0; // Başlangıçta seviye 0 (kilitli)
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    protected override void OnUpgraded()
    {
        // Seviye atladığında yapılacak ekstra bir şey varsa buraya yazılır.
        Debug.Log($"{data.entityName} seviye atladı! Yeni Seviye: {currentLevel}");
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void UseSkill()
    {
        if (data != null && SkillManager.Instance != null && IsReady && currentLevel > 0)
        {
            currentCooldownTimer = (float)data.cooldownTime.Evaluate(currentLevel);
            currentActiveTimer = (float)data.effectDuration.Evaluate(currentLevel);
            
            // SkillManager üzerinden gerçek etkiyi başlatacağız.
            SkillManager.Instance.ActivateSkillEffect(this);
        }
    }
}
