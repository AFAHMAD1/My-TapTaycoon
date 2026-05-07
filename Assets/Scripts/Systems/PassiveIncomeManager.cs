using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DurationLevelStep
{
    [Min(1)]
    public int requiredLevel = 1;

    [Min(0.2f)]
    public float durationSeconds = 5f;
}

[System.Serializable]
public class IncomeBuilding : UpgradableEntity
{
    public IncomeBuildingData data;

    public override UpgradableEntityData BaseData => data;

    [HideInInspector] public float timer = 0f;
    [HideInInspector] public bool isReadyToCollect = false;
    [HideInInspector] public GameObject spawnedVisualInstance;

    public double CurrentIncome()
    {
        if (currentLevel == 0 || data == null) return 0d;

        double baseIncome = data.incomePerCycle.Evaluate(currentLevel - 1);
        double multiplier = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.GetBuildingMultiplier(data)
            : 1d;

        return baseIncome * multiplier;
    }

    public double CurrentIncomePerSecond()
    {
        return CurrentIncome() / CurrentDuration();
    }

    public float CurrentDuration()
    {
        if (data == null) return 5f;
        return data.GetDurationForLevel(Mathf.Max(1, currentLevel));
    }

    protected override void OnUpgraded()
    {
        if (currentLevel == 1) EnsureVisualCreated();
        SyncVisualState();

        if (EffectManager.Instance != null && spawnedVisualInstance != null)
        {
            EffectManager.Instance.PlayLevelUpEffect(spawnedVisualInstance.transform.position);
        }
    }

    public void InitializeVisualState()
    {
        isReadyToCollect = currentLevel > 0 && timer >= CurrentDuration();
        if (currentLevel > 0) EnsureVisualCreated();
        SyncVisualState();
    }

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

    private void SyncVisualState()
    {
        if (spawnedVisualInstance != null)
        {
            spawnedVisualInstance.SetActive(currentLevel > 0);
        }
    }

    public bool TryCollectIncome()
    {
        if (!isReadyToCollect || CurrencyManager.Instance == null) return false;

        CurrencyManager.Instance.AddMoney(CurrentIncome(), false);
        timer = 0f;
        isReadyToCollect = false;
        return true;
    }
}

public class PassiveIncomeManager : MonoBehaviour, IPassiveIncomeManager, ISaveable
{
    public static PassiveIncomeManager Instance { get; private set; }

    [Header("Binalar ve Tesisler")]
    public List<IncomeBuilding> buildings = new List<IncomeBuilding>();

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
        foreach (var building in buildings)
        {
            building.InitializeVisualState();
        }
    }

    private void Update()
    {
        if (!enablePassiveIncome || CurrencyManager.Instance == null) return;

        foreach (var building in buildings)
        {
            if (building.currentLevel <= 0 || building.isReadyToCollect) continue;

            building.timer += Time.deltaTime;
            float duration = building.CurrentDuration();

            if (building.timer >= duration)
            {
                building.timer = duration;

                if (building.data != null && building.data.requireManualCollection)
                {
                    building.isReadyToCollect = true;
                    continue;
                }

                building.timer -= duration;
                CurrencyManager.Instance.AddMoney(building.CurrentIncome(), false);
            }
        }
    }

    public double GetTotalPassiveIncomePerSecond()
    {
        double total = 0d;
        foreach (var building in buildings)
        {
            if (building.currentLevel > 0)
            {
                total += building.CurrentIncomePerSecond();
            }
        }

        return total;
    }

    public void BuyUpgrade(int index) => BuyUpgrade(index, 1);

    public void BuyUpgrade(int index, int amount)
    {
        if (index >= 0 && index < buildings.Count)
        {
            PurchaseService.TryPurchase(buildings[index], amount);
        }
    }

    public bool CollectIncome(int index)
    {
        if (index >= 0 && index < buildings.Count)
        {
            return buildings[index].TryCollectIncome();
        }

        return false;
    }

    public void OnSave(SaveData data)
    {
        data.buildingLevels.Clear();
        foreach (var building in buildings)
        {
            data.buildingLevels.Add(building.currentLevel);
        }
    }

    public void OnLoad(SaveData data)
    {
        if (data.buildingLevels == null || data.buildingLevels.Count == 0) return;

        for (int i = 0; i < buildings.Count; i++)
        {
            if (i < data.buildingLevels.Count)
            {
                buildings[i].currentLevel = data.buildingLevels[i];
            }
        }

        foreach (var building in buildings)
        {
            building.InitializeVisualState();
        }
    }
}
