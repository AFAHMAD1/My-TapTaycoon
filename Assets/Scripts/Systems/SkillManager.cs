using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("Oyundaki Tum Beceriler")]
    public List<SkillData> allSkillDatas;
    
    // Oyuncunun sahip olduğu/ilerlettiği beceri durumları
    public List<SkillEntity> unlockedSkills = new List<SkillEntity>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        InitializeSkills();
    }

    private void InitializeSkills()
    {
        // Tüm datalardan birer Entity oluştur (Save sistemi eklenince buradan yüklenecek)
        foreach (var data in allSkillDatas)
        {
            unlockedSkills.Add(new SkillEntity(data));
        }
    }

    private void Update()
    {
        // Becerilerin bekleme (cooldown) ve aktiflik sürelerini düşür
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
        Debug.Log($"BECERİ KULLANILDI: {skill.data.entityName}");
        // Burada becerinin tipine göre geliri 2'ye katlama vb. kodlar eklenecek.
    }

    public void DeactivateSkillEffect(SkillEntity skill)
    {
        Debug.Log($"BECERİ ETKİSİ BİTTİ: {skill.data.entityName}");
        // Çarpanları geri alma kodları buraya gelecek.
    }
}
