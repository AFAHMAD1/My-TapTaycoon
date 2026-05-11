using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages click upgrades, boost upgrades, skill multipliers, and player profit multipliers.
/// </summary>
public class UpgradeManager : MonoBehaviour, ISaveable
{
    public static UpgradeManager Instance;

    [Header("Collector Upgrade")]
    public ClickUpgradeEntity collectorUpgrade = new ClickUpgradeEntity();

    [Header("Boost Upgrades")]
    [Tooltip("Special upgrades that increase building income or collector behavior.")]
    public List<BoostUpgrade> boostUpgrades = new List<BoostUpgrade>();

    [Header("Player Profit Upgrades")]
    [Tooltip("Player panel cards. Bought players multiply the profit of purchased buildings.")]
    public List<PlayerProfitUpgrade> playerProfitUpgrades = new List<PlayerProfitUpgrade>();

    private double _skillBuildingMultiplier = 1d;
    private double _skillClickMultiplier = 1d;

    public double CurrentClickValue =>
        (collectorUpgrade != null ? collectorUpgrade.CurrentReward() : 1d) * _skillClickMultiplier;

    public double NextUpgradeCost => collectorUpgrade != null ? collectorUpgrade.CurrentCost() : 0d;
    public int ClickPowerLevel => collectorUpgrade != null ? collectorUpgrade.currentLevel : 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

        EnsureDefaultPlayerProfitUpgrades();
    }

    private void OnValidate() => ValidateSetup();

    private void ValidateSetup()
    {
        if (collectorUpgrade == null || collectorUpgrade.data == null)
            Debug.LogWarning("[UpgradeManager] Collector Upgrade data is missing. Assign it in the Inspector.");

        if (boostUpgrades == null || boostUpgrades.Count == 0)
            Debug.LogWarning("[UpgradeManager] Boost list is empty. Assign boost upgrades in the Inspector.");
    }

    public double GetBuildingMultiplier(IncomeBuildingData buildingData)
    {
        double multiplier = _skillBuildingMultiplier;

        if (boostUpgrades != null && buildingData != null)
        {
            foreach (var boost in boostUpgrades)
            {
                if (boost.currentLevel > 0 && boost.data != null)
                {
                    if (boost.data.targetType == BoostTargetType.AllBuildings ||
                        (boost.data.targetType == BoostTargetType.SpecificBuilding &&
                         boost.data.targetBuilding == buildingData))
                    {
                        multiplier *= boost.data.boostMultiplier;
                    }
                }
            }
        }

        return multiplier * GetPlayerProfitMultiplier(buildingData);
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

    public double GetPlayerProfitMultiplier(IncomeBuildingData buildingData)
    {
        EnsureDefaultPlayerProfitUpgrades();

        double multiplier = 1d;
        foreach (PlayerProfitUpgrade playerUpgrade in playerProfitUpgrades)
        {
            if (playerUpgrade != null && playerUpgrade.AppliesToBuilding(buildingData))
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

        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null &&
                boost.data.targetType == BoostTargetType.CollectorSpeed)
            {
                multiplier *= boost.data.boostMultiplier;
            }
        }

        return multiplier;
    }

    public int GetCollectorCapacityBonus()
    {
        int bonus = 0;
        if (boostUpgrades == null) return bonus;

        foreach (var boost in boostUpgrades)
        {
            if (boost.currentLevel > 0 && boost.data != null &&
                boost.data.targetType == BoostTargetType.CollectorCapacity)
            {
                bonus += Mathf.RoundToInt(boost.data.boostMultiplier);
            }
        }

        return bonus;
    }

    public void BuyClickPower()
    {
        if (collectorUpgrade != null && PurchaseService.TryPurchase(collectorUpgrade, 1).HasPurchased)
        {
            return;
        }

        Debug.Log("Yetersiz Bakiye!");
    }

    public void SetSkillBuildingMultiplier(double multiplier) => _skillBuildingMultiplier = multiplier;
    public void SetSkillClickMultiplier(double multiplier) => _skillClickMultiplier = multiplier;
    public void ResetSkillBuildingMultiplier() => _skillBuildingMultiplier = 1d;
    public void ResetSkillClickMultiplier() => _skillClickMultiplier = 1d;

    public void OnSave(SaveData data)
    {
        data.collectorLevel = collectorUpgrade != null ? collectorUpgrade.currentLevel : 0;

        data.boostLevels.Clear();
        if (boostUpgrades != null)
        {
            foreach (var boost in boostUpgrades)
            {
                data.boostLevels.Add(boost != null ? boost.currentLevel : 0);
            }
        }

        data.playerProfitLevels.Clear();
        EnsureDefaultPlayerProfitUpgrades();
        foreach (var playerUpgrade in playerProfitUpgrades)
        {
            data.playerProfitLevels.Add(playerUpgrade != null ? playerUpgrade.currentLevel : 0);
        }
    }

    public void OnLoad(SaveData data)
    {
        if (collectorUpgrade != null)
            collectorUpgrade.currentLevel = data.collectorLevel;

        if (data.boostLevels != null && boostUpgrades != null)
        {
            for (int i = 0; i < boostUpgrades.Count; i++)
            {
                if (i < data.boostLevels.Count && boostUpgrades[i] != null)
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
            3.00f,
            3.50f,
            4.00f,
            4.50f,
            5.00f,
            5.50f,
            6.00f,
            7.00f,
            8.00f,
            10.00f
        };

        bool[] affectsAllBuildings =
        {
            false,
            true,
            true,
            false,
            false,
            true,
            true,
            false,
            true,
            false
        };

        for (int i = 0; i < names.Length; i++)
        {
            PlayerProfitUpgrade playerUpgrade;
            if (i < playerProfitUpgrades.Count && playerProfitUpgrades[i] != null)
            {
                playerUpgrade = playerProfitUpgrades[i];
            }
            else
            {
                playerUpgrade = new PlayerProfitUpgrade
                {
                    playerName = names[i],
                    description = "Multiplies bought building profit.",
                    baseCost = costs[i],
                    costMultiplierPerLevel = 2f,
                    profitMultiplierPerLevel = multipliers[i],
                    maxLevel = 1
                };
                playerProfitUpgrades.Add(playerUpgrade);
            }

            playerUpgrade.affectsAllBuildings = affectsAllBuildings[i];
            playerUpgrade.targetBuildingIndex = affectsAllBuildings[i] ? -1 : i;
            playerUpgrade.profitMultiplierPerLevel = multipliers[i];

            if (string.IsNullOrWhiteSpace(playerUpgrade.description))
            {
                playerUpgrade.description = "Multiplies bought building profit.";
            }
        }
    }
}
