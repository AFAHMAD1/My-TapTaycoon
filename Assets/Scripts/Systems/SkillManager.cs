using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour, ISaveable
{
    public static SkillManager Instance;

    [Header("Oyundaki Tum Beceriler")]
    public List<SkillData> allSkillDatas;

    public List<SkillEntity> unlockedSkills = new List<SkillEntity>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        InitializeSkills();
    }

    private void InitializeSkills()
    {
        unlockedSkills.Clear();
        if (allSkillDatas == null) return;

        foreach (var data in allSkillDatas)
        {
            if (data != null)
            {
                unlockedSkills.Add(new SkillEntity(data));
            }
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        foreach (var skill in unlockedSkills)
        {
            if (skill.currentCooldownTimer > 0)
            {
                skill.currentCooldownTimer -= dt;
            }

            if (skill.currentActiveTimer > 0)
            {
                skill.currentActiveTimer -= dt;
                if (skill.currentActiveTimer <= 0)
                {
                    DeactivateSkillEffect(skill);
                }
            }
        }
    }

    public void ActivateSkillEffect(SkillEntity skill)
    {
        if (skill?.data == null) return;

        float power = (float)skill.data.effectPower.Evaluate(skill.currentLevel);
        float duration = (float)skill.data.effectDuration.Evaluate(skill.currentLevel);

        switch (skill.data.effectType)
        {
            case SkillEffectType.ProfitBoost:
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.SetSkillBuildingMultiplier(power);
                Debug.Log($"[Skill] {skill.data.entityName}: building income {power}x for {duration}s.");
                break;

            case SkillEffectType.ClickPowerBoost:
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.SetSkillClickMultiplier(power);
                Debug.Log($"[Skill] {skill.data.entityName}: click power {power}x for {duration}s.");
                break;

            case SkillEffectType.InstantCash:
                if (PassiveIncomeManager.Instance != null && CurrencyManager.Instance != null)
                {
                    double perSecond = PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond();
                    double reward = perSecond * power * 60f;
                    CurrencyManager.Instance.AddMoney(reward, false);
                    Debug.Log($"[Skill] {skill.data.entityName}: added {NumberFormatter.Format(reward)} instantly.");
                }
                break;

            case SkillEffectType.AutoClicker:
                StartCoroutine(AutoClickerRoutine(skill, duration, power));
                Debug.Log($"[Skill] {skill.data.entityName}: auto clicker started for {duration}s.");
                break;
        }
    }

    public void DeactivateSkillEffect(SkillEntity skill)
    {
        if (skill?.data == null) return;

        switch (skill.data.effectType)
        {
            case SkillEffectType.ProfitBoost:
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.ResetSkillBuildingMultiplier();
                Debug.Log($"[Skill] {skill.data.entityName}: building multiplier reset.");
                break;

            case SkillEffectType.ClickPowerBoost:
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.ResetSkillClickMultiplier();
                Debug.Log($"[Skill] {skill.data.entityName}: click multiplier reset.");
                break;

            case SkillEffectType.InstantCash:
            case SkillEffectType.AutoClicker:
                break;
        }
    }

    private IEnumerator AutoClickerRoutine(SkillEntity skill, float duration, float powerMultiplier)
    {
        float elapsed = 0f;
        const float clickInterval = 0.5f;

        while (elapsed < duration && skill.IsActive)
        {
            yield return new WaitForSeconds(clickInterval);
            elapsed += clickInterval;

            if (UpgradeManager.Instance != null && CurrencyManager.Instance != null)
            {
                double clickValue = UpgradeManager.Instance.CurrentClickValue * powerMultiplier;
                CurrencyManager.Instance.AddMoney(clickValue, true);
            }
        }
    }

    public void OnSave(SaveData data)
    {
        data.skillLevels.Clear();
        foreach (var skill in unlockedSkills)
        {
            data.skillLevels.Add(skill.currentLevel);
        }
    }

    public void OnLoad(SaveData data)
    {
        if (data.skillLevels == null || data.skillLevels.Count == 0) return;

        for (int i = 0; i < unlockedSkills.Count; i++)
        {
            if (i < data.skillLevels.Count)
            {
                unlockedSkills[i].currentLevel = data.skillLevels[i];
            }
        }
    }
}
