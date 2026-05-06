using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyun içindeki genel yükseltmeleri ve çarpanları (boost) yöneten merkezi sınıftır.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    // Singleton: Sahnedeki tek UpgradeManager örneğine her yerden erişim sağlar.
    public static UpgradeManager Instance;

    [Header("Collector Upgrade (Tıklama Gücü)")]
    public ClickUpgradeEntity collectorUpgrade = new ClickUpgradeEntity();

    [Header("Boost Geliştirmeleri")]
    [Tooltip("Binaların gelirini veya robotun hızını artıran özel geliştirmeler.")]
    public List<BoostUpgrade> boostUpgrades = new List<BoostUpgrade>();

    // Mevcut tıklama değerini (para üretimini) döner.
    public double CurrentClickValue => collectorUpgrade != null ? collectorUpgrade.CurrentReward() : 1d;
    
    // Tıklama gücünü artırmanın maliyetini döner.
    public double NextUpgradeCost => collectorUpgrade != null ? collectorUpgrade.CurrentCost() : 0d;
    
    // Tıklama gücünün mevcut seviyesini döner.
    public int ClickPowerLevel => collectorUpgrade != null ? collectorUpgrade.currentLevel : 0;

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        Instance = this;
        ValidateSetup(); // Ayarların tam olup olmadığını kontrol et
    }

    // Unity Editor bu fonksiyonu Inspector degerleri degisince calistirir; eksik ayarlari yakalamaya yarar.
    private void OnValidate()
    {
        ValidateSetup();
    }

    // Geliştirme sırasında eksik referansları tespit eder ve konsola uyarı basar.
    private void ValidateSetup()
    {
        if (collectorUpgrade == null || collectorUpgrade.data == null)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogWarning' metodunu cagirir; Unity Console'a uyari mesaji yazar; oyun durmaz ama ayar eksigi olabilir.
            Debug.LogWarning("[UpgradeManager] Collector Upgrade (Tıklama) data'sı eksik! Lütfen Inspector'dan atayın.");
        }

        if (boostUpgrades == null || boostUpgrades.Count == 0)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogWarning' metodunu cagirir; Unity Console'a uyari mesaji yazar; oyun durmaz ama ayar eksigi olabilir.
            Debug.LogWarning("[UpgradeManager] Boost listesi boş. Lütfen Boost objelerini atayın.");
        }
    }

    /// <summary>
    /// Belirli bir bina için tüm aktif çarpanları (boost) hesaplar.
    /// Örneğin: 2 tane 5x boost varsa, bina 25 kat daha fazla kazanır.
    /// </summary>
    public double GetBuildingMultiplier(IncomeBuildingData buildingData)
    {
        double multiplier = 1d;
        
        if (boostUpgrades == null || buildingData == null) return multiplier;

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null)
            {
                // Boost tüm binaları mı etkiliyor yoksa sadece bu binayı mı?
                if (boost.data.targetType == BoostTargetType.AllBuildings || 
                   (boost.data.targetType == BoostTargetType.SpecificBuilding && boost.data.targetBuilding == buildingData))
                {
                    multiplier *= boost.data.boostMultiplier; 
                }
            }
        }
        return multiplier;
    }

    /// <summary>
    /// Toplayıcı robotun hız çarpanını hesaplar.
    /// </summary>
    public float GetCollectorSpeedMultiplier()
    {
        float multiplier = 1f;
        if (boostUpgrades == null) return multiplier;

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null && boost.data.targetType == BoostTargetType.CollectorSpeed)
            {
                multiplier *= boost.data.boostMultiplier;
            }
        }
        return multiplier;
    }

    /// <summary>
    /// Toplayıcı robotun kapasite bonusunu hesaplar.
    /// </summary>
    public int GetCollectorCapacityBonus()
    {
        int bonus = 0;
        if (boostUpgrades == null) return bonus;

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null && boost.data.targetType == BoostTargetType.CollectorCapacity)
            {
                bonus += Mathf.RoundToInt(boost.data.boostMultiplier);
            }
        }
        return bonus;
    }

    /// <summary>
    /// Tıklama gücünü satın alma fonksiyonu.
    /// </summary>
    public void BuyClickPower()
    {
        if (collectorUpgrade != null && PurchaseService.TryPurchase(collectorUpgrade, 1).HasPurchased)
        {
            // Başarılı satın alma durumunda yapılacak ek işlemler buraya gelebilir.
        }
        else
        {
            // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
            Debug.Log("Yetersiz Bakiye!");
        }
    }
}
