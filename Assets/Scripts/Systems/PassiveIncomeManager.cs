using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Binaların belirli seviyelerde üretim sürelerinin nasıl değişeceğini tutan yapı.
/// Örneğin: 25. seviyede üretim süresini 1 saniyeye düşür gibi.
/// </summary>
[System.Serializable]
public class DurationLevelStep
{
    [Min(1)]
    public int requiredLevel = 1;

    [Min(0.2f)]
    public float durationSeconds = 5f;
}

/// <summary>
/// Her bir binanın çalışma mantığını, gelirini ve görsel durumunu tutan sınıf.
/// </summary>
[System.Serializable]
public class IncomeBuilding : UpgradableEntity
{
    public IncomeBuildingData data; // Binanın ScriptableObject verisi (fiyat, başlangıç geliri vb.)
    
    public override UpgradableEntityData BaseData => data;

    [HideInInspector] public float timer = 0f; // Mevcut üretim süresindeki ilerleme
    [HideInInspector] public bool isReadyToCollect = false; // Manuel toplama gerekiyorsa toplama hazır mı?
    [HideInInspector] public GameObject spawnedVisualInstance; // Sahnede oluşturulan bina modeli

    /// <summary>
    /// Binanın şu anki seviyesine ve aktif boostlara göre tur başına kazancını hesaplar.
    /// </summary>
    public double CurrentIncome()
    {
        if (currentLevel == 0 || data == null) return 0d;
        
        // Seviyeye göre temel gelir (ScriptableObject içindeki eğriden gelir)
        double baseIncome = data.incomePerCycle.Evaluate(currentLevel - 1);
        
        // Aktif çarpanları (UpgradeManager) uygula
        double multiplier = 1d;
        if (UpgradeManager.Instance != null)
        {
            multiplier = UpgradeManager.Instance.GetBuildingMultiplier(data);
        }
        
        return baseIncome * multiplier;
    }

    /// <summary>
    /// Binanın saniye başına düşen ortalama kazancını hesaplar.
    /// </summary>
    public double CurrentIncomePerSecond()
    {
        return CurrentIncome() / CurrentDuration();
    }

    /// <summary>
    /// Binanın mevcut seviyesine göre bir üretimi kaç saniyede tamamladığını döner.
    /// </summary>
    public float CurrentDuration()
    {
        if (data == null) return 5f;
        return data.GetDurationForLevel(Mathf.Max(1, currentLevel));
    }

    // Seviye atlandığında yapılacak işlemler (UpgradableEntity'den override edildi)
    protected override void OnUpgraded()
    {
        if (currentLevel == 1) EnsureVisualCreated(); // İlk kez satın alındıysa modeli oluştur
        SyncVisualState(); // Modeli görünür yap

        // Seviye atlayınca görsel efekt oynat
        if (EffectManager.Instance != null && spawnedVisualInstance != null)
        {
            EffectManager.Instance.PlayLevelUpEffect(spawnedVisualInstance.transform.position);
        }
    }

    /// <summary>
    /// Oyun başladığında binanın durumunu (görsel ve zamanlayıcı) başlatır.
    /// </summary>
    public void InitializeVisualState()
    {
        isReadyToCollect = currentLevel > 0 && timer >= CurrentDuration();
        if (currentLevel > 0) EnsureVisualCreated();
        SyncVisualState();
    }

    // Binanın görsel modelini sahnede oluşturur.
    private void EnsureVisualCreated()
    {
        if (spawnedVisualInstance != null || data == null) return;

        if (data.buildingPrefab != null)
        {
            spawnedVisualInstance = Object.Instantiate(data.buildingPrefab, data.spawnOffset, Quaternion.identity);
            spawnedVisualInstance.name = $"{data.entityName}_Visual";
        }
        else if (data.buildingVisualObject != null)
        {
            spawnedVisualInstance = data.buildingVisualObject;
        }
    }

    // Binanın satın alınıp alınmadığına göre modelini açar veya kapatır.
    private void SyncVisualState()
    {
        if (spawnedVisualInstance != null) spawnedVisualInstance.SetActive(currentLevel > 0);
    }

    /// <summary>
    /// Manuel toplama modu açıksa geliri toplar.
    /// </summary>
    public bool TryCollectIncome()
    {
        if (!isReadyToCollect || CurrencyManager.Instance == null) return false;

        CurrencyManager.Instance.AddMoney(CurrentIncome(), false);
        timer = 0f;
        isReadyToCollect = false;
        return true;
    }
}

/// <summary>
/// Tüm binaların pasif gelir üretim sürecini yöneten merkezi yönetici.
/// </summary>
public class PassiveIncomeManager : MonoBehaviour, IPassiveIncomeManager, ISaveable
{
    public static PassiveIncomeManager Instance { get; private set; }

    [Header("Binalar ve Tesisler")]
    public List<IncomeBuilding> buildings = new List<IncomeBuilding>();

    /// <summary>
    /// IPassiveIncomeManager arayüzü gerekliliği.
    /// Mevcut 'buildings' alanını salt-okunur olarak dışarıya sunar.
    /// Dış kodlar listeyi okuyabilir ancak doğrudan değiştiremez.
    /// </summary>
    public IReadOnlyList<IncomeBuilding> Buildings => buildings;

    [Header("Gelir Kontrolu")]
    public bool enablePassiveIncome = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Başlangıçta tüm binaları görsel olarak hazırla
        foreach (var building in buildings) building.InitializeVisualState();
    }

    private void Update()
    {
        if (!enablePassiveIncome || CurrencyManager.Instance == null) return;

        // Her karede binaların üretim sürelerini ilerlet
        foreach (var building in buildings)
        {
            // Bina henüz satın alınmadıysa veya manuel toplama bekliyorsa geç
            if (building.currentLevel <= 0 || building.isReadyToCollect) continue;

            building.timer += Time.deltaTime;
            float duration = building.CurrentDuration();

            // Üretim süresi tamamlandı mı?
            if (building.timer >= duration)
            {
                building.timer = duration;

                // Eğer manuel tıklama gerekiyorsa beklemeye al
                if (building.data != null && building.data.requireManualCollection)
                {
                    building.isReadyToCollect = true;
                    continue;
                }

                // Otomatik toplama: Parayı ekle ve zamanlayıcıyı sıfırla
                // [BUG-05 FIX] Was 'building.timer = 0f' which loses any overshoot time.
                // On a slow frame (e.g. app resume spike), the timer could overshoot by more
                // than one full duration, silently skipping an entire income cycle.
                // Subtracting 'duration' instead preserves the remainder so no time is lost.
                building.timer -= duration;
                CurrencyManager.Instance.AddMoney(building.CurrentIncome(), false);
            }
        }
    }

    /// <summary>
    /// Tüm binaların toplam saniyelik pasif kazancını döner.
    /// </summary>
    public double GetTotalPassiveIncomePerSecond()
    {
        double total = 0d;
        foreach (var b in buildings) if (b.currentLevel > 0) total += b.CurrentIncomePerSecond();
        return total;
    }

    // Bina satın alma/seviye atlatma köprüsü
    public void BuyUpgrade(int index) => BuyUpgrade(index, 1);
    
    public void BuyUpgrade(int index, int amount)
    {
        if (index >= 0 && index < buildings.Count)
            PurchaseService.TryPurchase(buildings[index], amount);
    }

    // Manuel gelir toplama köprüsü
    public bool CollectIncome(int index)
    {
        if (index >= 0 && index < buildings.Count)
            return buildings[index].TryCollectIncome();
        return false;
    }

    // --- ISaveable ---

    public void OnSave(SaveData data)
    {
        data.buildingLevels.Clear();
        foreach (var building in buildings)
            data.buildingLevels.Add(building.currentLevel);
    }

    public void OnLoad(SaveData data)
    {
        if (data.buildingLevels == null || data.buildingLevels.Count == 0) return;

        for (int i = 0; i < buildings.Count; i++)
        {
            if (i < data.buildingLevels.Count)
                buildings[i].currentLevel = data.buildingLevels[i];
        }

        // Seviyeleri yukledikten sonra gorselleri guncelle
        foreach (var building in buildings)
            building.InitializeVisualState();
    }
}
