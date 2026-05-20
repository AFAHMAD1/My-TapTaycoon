using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages click upgrades, boost upgrades, skill multipliers, and player profit multipliers.
/// </summary>
public class UpgradeManager : MonoBehaviour, ISaveable
{
    public static UpgradeManager Instance;

    [Header("Boost Upgrades")]
    [Tooltip("Special upgrades that increase building income or collector behavior.")]
    public List<BoostUpgrade> boostUpgrades = new List<BoostUpgrade>();

    [Header("Player Panel Skill Queue")]
    [Min(1)] public int visiblePlayerSkillCount = 10;
    [Min(1)] public int generatedPlayerSkillCount = 1000;
    public int playerSkillCursor;
    public List<PlayerPanelSkillState> playerSkillDeck = new List<PlayerPanelSkillState>();

    private double _skillBuildingMultiplier = 1d;
    private double _businessSurchargeMultiplier = 1d;
    private double _skillClickMultiplier = 1d;

    private const double TapLevelRiserBaseCost = 30d;
    private const double TapLevelRiserCostMultiplier = 3d;
    private const double TapLevelRiserValueMultiplier = 2d;

    public double CurrentClickValue => TapLevelRiserMultiplier * _skillClickMultiplier;

    public int TapLevelRiserLevel { get; private set; }
    public double TapLevelRiserMultiplier => SafePow(TapLevelRiserValueMultiplier, TapLevelRiserLevel);
    public double TapLevelRiserNextCost => TapLevelRiserBaseCost * SafePow(TapLevelRiserCostMultiplier, TapLevelRiserLevel);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void OnValidate() => ValidateSetup();

    private void ValidateSetup()
    {
        if (boostUpgrades == null || boostUpgrades.Count == 0)
            Debug.LogWarning("[UpgradeManager] Boost list is empty. Assign boost upgrades in the Inspector.");
    }

    public double GetBuildingMultiplier(IncomeBuildingData buildingData)
    {
        double multiplier = _skillBuildingMultiplier * _businessSurchargeMultiplier;

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

        return multiplier * GetPlayerPanelBuildingMultiplier(buildingData);
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

    public bool CanBuyTapLevelRiser()
    {
        return CurrencyManager.Instance != null &&
               CurrencyManager.Instance.currentMoney >= TapLevelRiserNextCost;
    }

    public bool TryBuyTapLevelRiser()
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[UpgradeManager] Tap level upgrade could not be bought because CurrencyManager is missing.");
            return false;
        }

        double cost = TapLevelRiserNextCost;
        if (!CurrencyManager.Instance.SpendMoney(cost))
        {
            Debug.Log("Yetersiz Bakiye!");
            return false;
        }

        TapLevelRiserLevel++;
        return true;
    }

    public void SetSkillBuildingMultiplier(double multiplier) => _skillBuildingMultiplier = multiplier;
    public void SetBusinessSurchargeMultiplier(double multiplier) => _businessSurchargeMultiplier = multiplier;
    public void SetSkillClickMultiplier(double multiplier) => _skillClickMultiplier = multiplier;
    public void ResetSkillBuildingMultiplier() => _skillBuildingMultiplier = 1d;
    public void ResetBusinessSurchargeMultiplier() => _businessSurchargeMultiplier = 1d;
    public void ResetSkillClickMultiplier() => _skillClickMultiplier = 1d;

    public void OnSave(SaveData data)
    {
        data.tapLevelRiserLevel = TapLevelRiserLevel;

        data.boostLevels.Clear();
        if (boostUpgrades != null)
        {
            foreach (var boost in boostUpgrades)
            {
                data.boostLevels.Add(boost != null ? boost.currentLevel : 0);
            }
        }

        EnsurePlayerSkillDeck();
        data.playerSkillCursor = playerSkillCursor;
    }

    public void OnLoad(SaveData data)
    {
        TapLevelRiserLevel = Mathf.Max(0, data.tapLevelRiserLevel);

        if (data.boostLevels != null && boostUpgrades != null)
        {
            for (int i = 0; i < boostUpgrades.Count; i++)
            {
                if (i < data.boostLevels.Count && boostUpgrades[i] != null)
                    boostUpgrades[i].currentLevel = data.boostLevels[i];
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

    private static double SafePow(double baseValue, int exponent)
    {
        double value = System.Math.Pow(baseValue, Mathf.Max(0, exponent));
        return double.IsNaN(value) || double.IsInfinity(value) ? double.MaxValue : value;
    }

}
