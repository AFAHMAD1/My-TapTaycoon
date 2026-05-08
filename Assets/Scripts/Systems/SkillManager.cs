using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour, ISaveable
{
    public static SkillManager Instance;

    [Header("Oyundaki Tum Beceriler")]
    public List<SkillData> allSkillDatas;

    public List<SkillEntity> unlockedSkills = new List<SkillEntity>();

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        // [BUG-02 FIX] Singleton duplicate guard eklendi.
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        InitializeSkills();
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void InitializeSkills()
    {
        // [BUG-17 FIX] Liste temizlenerek mükerrer girdi oluşması önlendi.
        // Bu satir: 'unlockedSkills' objesi uzerindeki 'Clear' metodunu cagirir; listenin icindeki tum elemanlari siler; liste bos hale gelir.
        unlockedSkills.Clear();
        if (allSkillDatas == null) return;

        // Tüm datalardan birer Entity oluştur (Save sistemi eklenince buradan yüklenecek)
        foreach (var data in allSkillDatas)
        {
            if (data != null)
                // Bu satir: 'unlockedSkills' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
                unlockedSkills.Add(new SkillEntity(data));
        }
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        float dt = Time.deltaTime;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var skill in unlockedSkills)
        {
            if (skill.currentCooldownTimer > 0)
                skill.currentCooldownTimer -= dt;

            if (skill.currentActiveTimer > 0)
            {
                skill.currentActiveTimer -= dt;
                if (skill.currentActiveTimer <= 0)
                    DeactivateSkillEffect(skill);
            }
        }
    }

    // --- Skill Effects (BUG-07 FIX) ---

    /// <summary>
    /// Becerinin tipine göre gerçek oyun etkisini uygular.
    /// </summary>
    public void ActivateSkillEffect(SkillEntity skill)
    {
        if (skill?.data == null) return;

        float power    = (float)skill.data.effectPower.Evaluate(skill.currentLevel);
        float duration = (float)skill.data.effectDuration.Evaluate(skill.currentLevel);

        switch (skill.data.effectType)
        {
            case SkillEffectType.ProfitBoost:
                // Tüm binaların gelirini 'power' katına çıkar
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.SetSkillBuildingMultiplier(power);
                Debug.Log($"[Skill] {skill.data.entityName}: {duration}s boyunca bina geliri {power}x!");
                break;

            case SkillEffectType.ClickPowerBoost:
                // Tıklama gücünü 'power' katına çıkar
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.SetSkillClickMultiplier(power);
                Debug.Log($"[Skill] {skill.data.entityName}: {duration}s boyunca tıklama gücü {power}x!");
                break;

            case SkillEffectType.InstantCash:
                // 'power' dakikalık pasif geliri anında ekle
                if (PassiveIncomeManager.Instance != null && CurrencyManager.Instance != null)
                {
                    double perSec  = PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond();
                    double reward  = perSec * power * 60f; // power = dakika
                    CurrencyManager.Instance.AddMoney(reward, false);
                    Debug.Log($"[Skill] {skill.data.entityName}: {NumberFormatter.Format(reward)} anında eklendi!");
                }
                break;

            case SkillEffectType.AutoClicker:
                // 'duration' saniye boyunca otomatik tıklama coroutine'i başlat
                StartCoroutine(AutoClickerRoutine(skill, duration, power));
                Debug.Log($"[Skill] {skill.data.entityName}: {duration}s boyunca otomatik tıklama başladı!");
                break;
        }
    }

    /// <summary>
    /// Beceri süresi bittiğinde uygulanan etkileri geri alır.
    /// </summary>
    public void DeactivateSkillEffect(SkillEntity skill)
    {
        if (skill?.data == null) return;

        switch (skill.data.effectType)
        {
            case SkillEffectType.ProfitBoost:
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.ResetSkillBuildingMultiplier();
                Debug.Log($"[Skill] {skill.data.entityName}: Bina geliri çarpanı sıfırlandı.");
                break;

            case SkillEffectType.ClickPowerBoost:
                if (UpgradeManager.Instance != null)
                    UpgradeManager.Instance.ResetSkillClickMultiplier();
                Debug.Log($"[Skill] {skill.data.entityName}: Tıklama çarpanı sıfırlandı.");
                break;

            case SkillEffectType.InstantCash:
            case SkillEffectType.AutoClicker:
                // Anlık veya coroutine-tabanlı etkiler — deactivation gerekmez.
                break;
        }
    }

    /// <summary>
    /// AutoClicker becerisi: 'duration' saniye boyunca her 0.5 saniyede bir
    /// tıklama değeri kadar para ekler.
    /// </summary>
    private IEnumerator AutoClickerRoutine(SkillEntity skill, float duration, float powerMultiplier)
    {
        float elapsed       = 0f;
        float clickInterval = 0.5f; // saniyede 2 tıklama

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

    // --- ISaveable (BUG-06 FIX) ---

    public void OnSave(SaveData data)
    {
        data.skillLevels.Clear();
        foreach (var skill in unlockedSkills)
            data.skillLevels.Add(skill.currentLevel);
    }

    public void OnLoad(SaveData data)
    {
        if (data.skillLevels == null || data.skillLevels.Count == 0) return;

        for (int i = 0; i < unlockedSkills.Count; i++)
        {
            if (i < data.skillLevels.Count)
                unlockedSkills[i].currentLevel = data.skillLevels[i];
        }

    }
}
