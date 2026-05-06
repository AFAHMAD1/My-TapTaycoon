using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("Oyundaki Tum Beceriler")]
    public List<SkillData> allSkillDatas;
    
    // Oyuncunun sahip olduğu/ilerlettiği beceri durumları
    public List<SkillEntity> unlockedSkills = new List<SkillEntity>();

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeSkills();
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void InitializeSkills()
    {
        // Bu satir: 'unlockedSkills' objesi uzerindeki 'Clear' metodunu cagirir; listenin icindeki tum elemanlari siler; liste bos hale gelir.
        unlockedSkills.Clear();
        if (allSkillDatas == null)
        {
            return;
        }

        // Tüm datalardan birer Entity oluştur (Save sistemi eklenince buradan yüklenecek)
        foreach (var data in allSkillDatas)
        {
            if (data != null)
            {
                // Bu satir: 'unlockedSkills' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
                unlockedSkills.Add(new SkillEntity(data));
            }
        }
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        // Becerilerin bekleme (cooldown) ve aktiflik sürelerini düşür
        float dt = Time.deltaTime;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
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

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void ActivateSkillEffect(SkillEntity skill)
    {
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log($"BECERİ KULLANILDI: {skill.data.entityName}");
        // Burada becerinin tipine göre geliri 2'ye katlama vb. kodlar eklenecek.
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void DeactivateSkillEffect(SkillEntity skill)
    {
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log($"BECERİ ETKİSİ BİTTİ: {skill.data.entityName}");
        // Çarpanları geri alma kodları buraya gelecek.
    }
}
