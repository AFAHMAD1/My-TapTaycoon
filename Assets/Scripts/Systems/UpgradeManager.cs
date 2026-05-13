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

    [Header("Player Panel Skill Queue")]
    [Min(1)] public int visiblePlayerSkillCount = 10;
    [Min(1)] public int generatedPlayerSkillCount = 1000;
    public int playerSkillCursor;
    public List<PlayerPanelSkillState> playerSkillDeck = new List<PlayerPanelSkillState>();

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

        if (boostUpgrades == null || buildingData == null)
            return multiplier * GetPlayerProfitMultiplier() * GetPlayerPanelBuildingMultiplier(buildingData);

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
        return multiplier * GetPlayerProfitMultiplier() * GetPlayerPanelBuildingMultiplier(buildingData);
    }

    public double GetPlayerPanelBuildingMultiplier(IncomeBuildingData buildingData)
    {
        if (buildingData == null)
        {
            return 1d;
        }

        EnsurePlayerSkillDeck();

        double multiplier = 1d;
        foreach (PlayerPanelSkillState skill in playerSkillDeck)
        {
            if (skill == null || !skill.purchased)
            {
                continue;
            }

            switch (skill.kind)
            {
                case PlayerPanelSkillKind.BuildingProfitMultiplier:
                    if (IsSkillTargetingBuilding(skill, buildingData))
                    {
                        multiplier *= Mathf.Max(1f, skill.multiplier);
                    }
                    break;
                case PlayerPanelSkillKind.AllBuildingsProfitMultiplier:
                case PlayerPanelSkillKind.SuperBusiness:
                    multiplier *= Mathf.Max(1f, skill.multiplier);
                    break;
            }
        }

        return multiplier;
    }

    public bool IsPlayerPanelAutoCollectEnabled(IncomeBuildingData buildingData)
    {
        if (buildingData == null)
        {
            return false;
        }

        EnsurePlayerSkillDeck();

        foreach (PlayerPanelSkillState skill in playerSkillDeck)
        {
            if (skill != null &&
                skill.purchased &&
                skill.kind == PlayerPanelSkillKind.BuildingAutoCollect &&
                IsSkillTargetingBuilding(skill, buildingData))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsSkillTargetingBuilding(PlayerPanelSkillState skill, IncomeBuildingData buildingData)
    {
        if (skill == null ||
            buildingData == null ||
            PassiveIncomeManager.Instance == null ||
            PassiveIncomeManager.Instance.buildings == null ||
            skill.targetBuildingIndex < 0 ||
            skill.targetBuildingIndex >= PassiveIncomeManager.Instance.buildings.Count)
        {
            return false;
        }

        IncomeBuilding targetBuilding = PassiveIncomeManager.Instance.buildings[skill.targetBuildingIndex];
        return targetBuilding != null && targetBuilding.data == buildingData;
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

    public List<PlayerPanelSkillState> GetVisiblePlayerPanelSkills()
    {
        EnsurePlayerSkillDeck();

        List<PlayerPanelSkillState> visibleSkills = new List<PlayerPanelSkillState>();
        int safeCursor = Mathf.Clamp(playerSkillCursor, 0, playerSkillDeck.Count);

        for (int i = safeCursor; i < playerSkillDeck.Count && visibleSkills.Count < visiblePlayerSkillCount; i++)
        {
            PlayerPanelSkillState skill = playerSkillDeck[i];
            if (skill != null && !skill.purchased)
            {
                visibleSkills.Add(skill);
            }
        }

        return visibleSkills;
    }

    public bool TryPurchasePlayerPanelSkill(PlayerPanelSkillState skill)
    {
        if (skill == null || skill.purchased || CurrencyManager.Instance == null)
        {
            return false;
        }

        if (!CurrencyManager.Instance.SpendMoney(skill.cost))
        {
            return false;
        }

        skill.purchased = true;
        while (playerSkillCursor < playerSkillDeck.Count &&
               playerSkillDeck[playerSkillCursor] != null &&
               playerSkillDeck[playerSkillCursor].purchased)
        {
            playerSkillCursor++;
        }

        PurchaseService.OnAnyPurchaseCompleted?.Invoke();
        return true;
    }

    private void EnsurePlayerSkillDeck()
    {
        if (playerSkillDeck == null)
        {
            playerSkillDeck = new List<PlayerPanelSkillState>();
        }

        if (playerSkillDeck.Count == generatedPlayerSkillCount)
        {
            return;
        }

        playerSkillDeck.Clear();

        for (int i = 0; i < generatedPlayerSkillCount; i++)
        {
            playerSkillDeck.Add(CreatePlayerPanelSkill(i));
        }

        playerSkillCursor = Mathf.Clamp(playerSkillCursor, 0, playerSkillDeck.Count);
    }

    private PlayerPanelSkillState CreatePlayerPanelSkill(int index)
    {
        int buildingCount = GetBuildingCountForSkillDeck();
        int cycle = Mathf.Max(0, index / Mathf.Max(1, buildingCount));
        int buildingIndex = GetSortedBuildingIndex(index % Mathf.Max(1, buildingCount));

        PlayerPanelSkillState skill = new PlayerPanelSkillState
        {
            deckIndex = index,
            targetBuildingIndex = buildingIndex,
            durationSeconds = 30f,
            cashMinutes = 2f,
            multiplier = 2f + (cycle % 5),
            cost = CalculatePlayerPanelSkillCost(index, buildingIndex, cycle)
        };

        switch (index)
        {
            case 0:
                skill.kind = PlayerPanelSkillKind.BuildingProfitMultiplier;
                skill.targetBuildingIndex = GetSortedBuildingIndex(0);
                skill.multiplier = 6f;
                break;
            case 1:
                skill.kind = PlayerPanelSkillKind.BuildingAutoCollect;
                skill.targetBuildingIndex = GetSortedBuildingIndex(0);
                break;
            case 2:
                skill.kind = PlayerPanelSkillKind.BuildingProfitMultiplier;
                skill.targetBuildingIndex = GetSortedBuildingIndex(Mathf.Min(1, buildingCount - 1));
                skill.multiplier = 3f;
                break;
            case 3:
                skill.kind = PlayerPanelSkillKind.ClickProfitMultiplier;
                skill.multiplier = 6f;
                break;
            case 4:
                skill.kind = PlayerPanelSkillKind.AllBuildingsProfitMultiplier;
                skill.multiplier = 3f;
                break;
            case 5:
                skill.kind = PlayerPanelSkillKind.BuildingAutoCollect;
                skill.targetBuildingIndex = GetSortedBuildingIndex(Mathf.Min(1, buildingCount - 1));
                break;
            case 6:
                skill.kind = PlayerPanelSkillKind.FastCash;
                break;
            case 7:
                skill.kind = PlayerPanelSkillKind.BuildingProfitMultiplier;
                skill.targetBuildingIndex = GetSortedBuildingIndex(Mathf.Min(2, buildingCount - 1));
                skill.multiplier = 4f;
                break;
            case 8:
                skill.kind = PlayerPanelSkillKind.BuildingProfitMultiplier;
                skill.targetBuildingIndex = GetSortedBuildingIndex(Mathf.Min(3, buildingCount - 1));
                skill.multiplier = 5f;
                break;
            case 9:
                skill.kind = PlayerPanelSkillKind.BuildingAutoCollect;
                skill.targetBuildingIndex = GetSortedBuildingIndex(Mathf.Min(2, buildingCount - 1));
                break;
            default:
                ApplyGeneratedPlayerSkillPattern(skill, index, buildingCount, cycle);
                break;
        }

        return skill;
    }

    private void ApplyGeneratedPlayerSkillPattern(PlayerPanelSkillState skill, int index, int buildingCount, int cycle)
    {
        int pattern = (index - 10) % 10;
        int buildingIndex = GetSortedBuildingIndex((index + cycle) % Mathf.Max(1, buildingCount));
        skill.targetBuildingIndex = buildingIndex;

        switch (pattern)
        {
            case 0:
                skill.kind = PlayerPanelSkillKind.FastCash;
                break;
            case 1:
                skill.kind = PlayerPanelSkillKind.SuperBusiness;
                skill.multiplier = 2f + (cycle % 4);
                break;
            case 2:
            case 7:
                skill.kind = PlayerPanelSkillKind.BuildingAutoCollect;
                break;
            case 3:
                skill.kind = PlayerPanelSkillKind.CashPileChance;
                skill.multiplier = 2f + (cycle % 3);
                break;
            case 4:
                skill.kind = PlayerPanelSkillKind.MidasTouch;
                skill.multiplier = 10f + (cycle % 5);
                break;
            case 5:
            case 8:
                skill.kind = PlayerPanelSkillKind.BuildingProfitMultiplier;
                skill.multiplier = 3f + (cycle % 6);
                break;
            case 6:
                skill.kind = PlayerPanelSkillKind.ClickProfitMultiplier;
                skill.multiplier = 4f + (cycle % 6);
                break;
            case 9:
                skill.kind = PlayerPanelSkillKind.AllBuildingsProfitMultiplier;
                skill.multiplier = 2f + (cycle % 4);
                break;
        }
    }

    private int GetBuildingCountForSkillDeck()
    {
        if (PassiveIncomeManager.Instance != null &&
            PassiveIncomeManager.Instance.buildings != null &&
            PassiveIncomeManager.Instance.buildings.Count > 0)
        {
            return PassiveIncomeManager.Instance.buildings.Count;
        }

        return 10;
    }

    private int GetSortedBuildingIndex(int rank)
    {
        if (PassiveIncomeManager.Instance == null ||
            PassiveIncomeManager.Instance.buildings == null ||
            PassiveIncomeManager.Instance.buildings.Count == 0)
        {
            return Mathf.Clamp(rank, 0, 9);
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < PassiveIncomeManager.Instance.buildings.Count; i++)
        {
            indices.Add(i);
        }

        indices.Sort((a, b) =>
        {
            double costA = PassiveIncomeManager.Instance.buildings[a]?.data != null
                ? PassiveIncomeManager.Instance.buildings[a].data.EvaluateCost(0)
                : double.MaxValue;
            double costB = PassiveIncomeManager.Instance.buildings[b]?.data != null
                ? PassiveIncomeManager.Instance.buildings[b].data.EvaluateCost(0)
                : double.MaxValue;
            return costA.CompareTo(costB);
        });

        int safeRank = Mathf.Clamp(rank, 0, indices.Count - 1);
        return indices[safeRank];
    }

    private double CalculatePlayerPanelSkillCost(int index, int buildingIndex, int cycle)
    {
        double baseCost = 100d * System.Math.Pow(1.18d, index);

        if (PassiveIncomeManager.Instance != null &&
            PassiveIncomeManager.Instance.buildings != null &&
            buildingIndex >= 0 &&
            buildingIndex < PassiveIncomeManager.Instance.buildings.Count &&
            PassiveIncomeManager.Instance.buildings[buildingIndex]?.data != null)
        {
            baseCost = PassiveIncomeManager.Instance.buildings[buildingIndex].data.EvaluateCost(0) * System.Math.Pow(1.28d, cycle + 1);
        }

        return System.Math.Max(10d, baseCost);
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

        EnsurePlayerSkillDeck();
        data.playerSkillCursor = playerSkillCursor;
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

        EnsurePlayerSkillDeck();
        playerSkillCursor = Mathf.Clamp(data.playerSkillCursor, 0, playerSkillDeck.Count);
        for (int i = 0; i < playerSkillDeck.Count; i++)
        {
            if (playerSkillDeck[i] != null)
            {
                playerSkillDeck[i].purchased = i < playerSkillCursor;
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
