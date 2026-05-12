using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyun içindeki genel yükseltmeleri ve çarpanları (boost) yöneten merkezi sınıftır.
/// </summary>
public class UpgradeManager : MonoBehaviour, ISaveable
{
    public static UpgradeManager Instance;

    [Header("Collector Upgrade (Tıklama Gücü)")]
    public ClickUpgradeEntity collectorUpgrade = new ClickUpgradeEntity();

    [Header("Boost Geliştirmeleri")]
    [Tooltip("Binaların gelirini veya robotun hızını artıran özel geliştirmeler.")]
    public List<BoostUpgrade> boostUpgrades = new List<BoostUpgrade>();

    [Header("Player Profit Upgrades")]
    [Tooltip("Player panel cards. Bought players multiply the profit of purchased buildings.")]
    public List<PlayerProfitUpgrade> playerProfitUpgrades = new List<PlayerProfitUpgrade>();

    // Beceri sistemi tarafından geçici olarak ayarlanan çarpanlar (BUG-07 FIX)
    private double _skillBuildingMultiplier = 1d;
    private double _skillClickMultiplier = 1d;

    // Mevcut tıklama değeri — beceri çarpanını da içerir
    public double CurrentClickValue =>
        (collectorUpgrade != null ? collectorUpgrade.CurrentReward() : 1d) * _skillClickMultiplier;

    public double NextUpgradeCost => collectorUpgrade != null ? collectorUpgrade.CurrentCost() : 0d;
    public int ClickPowerLevel => collectorUpgrade != null ? collectorUpgrade.currentLevel : 0;

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        // [BUG-02 FIX] Singleton duplicate guard eklendi.
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        EnsureDefaultPlayerProfitUpgrades();
    }

    // Unity Editor bu fonksiyonu Inspector degerleri degisince calistirir; eksik ayarlari yakalamaya yarar.
    private void OnValidate() => ValidateSetup();

    private void ValidateSetup()
    {
        if (collectorUpgrade == null || collectorUpgrade.data == null)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogWarning' metodunu cagirir; Unity Console'a uyari mesaji yazar; oyun durmaz ama ayar eksigi olabilir.
            Debug.LogWarning("[UpgradeManager] Collector Upgrade (Tıklama) data'sı eksik! Lütfen Inspector'dan atayın.");
        }

        if (boostUpgrades == null || boostUpgrades.Count == 0)
            Debug.LogWarning("[UpgradeManager] Boost listesi boş. Lütfen Boost objelerini atayın.");
    }

    /// <summary>
    /// Belirli bir bina için tüm aktif çarpanları hesaplar (boost + geçici beceri çarpanı).
    /// </summary>
    public double GetBuildingMultiplier(IncomeBuildingData buildingData)
    {
        double multiplier = _skillBuildingMultiplier; // Beceri çarpanıyla başla

        if (boostUpgrades == null || buildingData == null) return multiplier * GetPlayerProfitMultiplier();

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null)
            {
                if (boost.data.targetType == BoostTargetType.AllBuildings ||
                   (boost.data.targetType == BoostTargetType.SpecificBuilding && boost.data.targetBuilding == buildingData))
                {
                    multiplier *= boost.data.boostMultiplier;
                }
            }
        }
        return multiplier * GetPlayerProfitMultiplier();
    }

    public double GetPlayerProfitMultiplier()
    {
        EnsureDefaultPlayerProfitUpgrades();

        double multiplier = 1d;
        foreach (PlayerProfitUpgrade playerUpgrade in playerProfitUpgrades)
        {
            if (playerUpgrade != null)
            {
                multiplier *= playerUpgrade.CurrentMultiplier();
            }
        }

        return multiplier;
    }

    public float GetCollectorSpeedMultiplier()
    {
        float multiplier = 1f;
        if (boostUpgrades == null) return multiplier;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null && boost.data.targetType == BoostTargetType.CollectorSpeed)
                multiplier *= boost.data.boostMultiplier;
        }
        return multiplier;
    }

    public int GetCollectorCapacityBonus()
    {
        int bonus = 0;
        if (boostUpgrades == null) return bonus;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null && boost.data.targetType == BoostTargetType.CollectorCapacity)
                bonus += Mathf.RoundToInt(boost.data.boostMultiplier);
        }
        return bonus;
    }

    public void BuyClickPower()
    {
        if (collectorUpgrade != null && PurchaseService.TryPurchase(collectorUpgrade, 1).HasPurchased)
        {
            // Başarılı satın alma sonrası ek işlemler buraya gelebilir.
        }
        else
        {
            // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
            Debug.Log("Yetersiz Bakiye!");
        }
    }

    // --- Skill Multiplier API (BUG-07 FIX) ---

    /// <summary>Tüm binalar için geçici bir beceri çarpanı uygular.</summary>
    public void SetSkillBuildingMultiplier(double multiplier) => _skillBuildingMultiplier = multiplier;

    /// <summary>Tıklama gücü için geçici bir beceri çarpanı uygular.</summary>
    public void SetSkillClickMultiplier(double multiplier) => _skillClickMultiplier = multiplier;

    /// <summary>Bina beceri çarpanını sıfırlar (beceri süresi bittiğinde çağrılır).</summary>
    public void ResetSkillBuildingMultiplier() => _skillBuildingMultiplier = 1d;

    /// <summary>Tıklama beceri çarpanını sıfırlar (beceri süresi bittiğinde çağrılır).</summary>
    public void ResetSkillClickMultiplier() => _skillClickMultiplier = 1d;

    // --- ISaveable ---

    public void OnSave(SaveData data)
    {
        data.collectorLevel = collectorUpgrade != null ? collectorUpgrade.currentLevel : 0;

        data.boostLevels.Clear();
        if (boostUpgrades != null)
            foreach (var boost in boostUpgrades)
                data.boostLevels.Add(boost.currentLevel);

        data.playerProfitLevels.Clear();
        EnsureDefaultPlayerProfitUpgrades();
        foreach (var playerUpgrade in playerProfitUpgrades)
            data.playerProfitLevels.Add(playerUpgrade != null ? playerUpgrade.currentLevel : 0);
    }

    public void OnLoad(SaveData data)
    {
        if (collectorUpgrade != null)
            collectorUpgrade.currentLevel = data.collectorLevel;

        if (data.boostLevels != null && boostUpgrades != null)
        {
            for (int i = 0; i < boostUpgrades.Count; i++)
            {
                if (i < data.boostLevels.Count)
                    boostUpgrades[i].currentLevel = data.boostLevels[i];
            }
        }

        EnsureDefaultPlayerProfitUpgrades();
        if (data.playerProfitLevels != null)
        {
            for (int i = 0; i < playerProfitUpgrades.Count; i++)
            {
                if (i < data.playerProfitLevels.Count && playerProfitUpgrades[i] != null)
                    playerProfitUpgrades[i].currentLevel = data.playerProfitLevels[i];
            }
        }
    }

    private void EnsureDefaultPlayerProfitUpgrades()
    {
        if (playerProfitUpgrades == null)
        {
            playerProfitUpgrades = new List<PlayerProfitUpgrade>();
        }

        if (playerProfitUpgrades.Count > 0)
        {
            return;
        }

        string[] names =
        {
            "Rookie Striker",
            "Wing Runner",
            "Midfield Engine",
            "Set Piece Ace",
            "Captain",
            "Star Forward",
            "Playmaker",
            "Clean Sheet Wall",
            "Golden Boot",
            "Legend"
        };

        double[] costs =
        {
            250d,
            1500d,
            10000d,
            75000d,
            500000d,
            3500000d,
            25000000d,
            180000000d,
            1250000000d,
            10000000000d
        };

        float[] multipliers =
        {
            1.10f,
            1.15f,
            1.20f,
            1.25f,
            1.35f,
            1.50f,
            1.75f,
            2.00f,
            2.50f,
            3.00f
        };

        for (int i = 0; i < names.Length; i++)
        {
            playerProfitUpgrades.Add(new PlayerProfitUpgrade
            {
                playerName = names[i],
                description = "Multiplies bought building profit.",
                baseCost = costs[i],
                costMultiplierPerLevel = 2f,
                profitMultiplierPerLevel = multipliers[i],
                maxLevel = 1
            });
        }
    }
}
