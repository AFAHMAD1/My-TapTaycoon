using UnityEngine;

/// <summary>
/// Tum satin alinabilir ve gelistirilebilir nesnelerin State (Durum) tabani.
/// Binalar, ozellikler bu sinifi miras alarak kendi ScriptableObject verilerini referans gosterir.
/// </summary>
[System.Serializable]
public abstract class UpgradableEntity
{
    public int currentLevel = 0;

    // Alt siniflar kendi SO data'larini dondurecek
    public abstract UpgradableEntityData BaseData { get; }
    
    public bool IsMaxLevel => BaseData != null && BaseData.maxLevel > 0 && currentLevel >= BaseData.maxLevel;

    public double CurrentCost()
    {
        if (BaseData == null) return 0d;
        return BaseData.EvaluateCost(currentLevel);
    }

    public bool CanAffordUpgrade()
    {
        return CurrencyManager.Instance != null && CurrencyManager.Instance.currentMoney >= CurrentCost();
    }

    public double GetTotalCostForUpgrades(int amount)
    {
        if (BaseData == null) return 0d;
        
        int safeAmount = Mathf.Max(0, amount);
        double totalCost = 0d;

        for (int i = 0; i < safeAmount; i++)
        {
            totalCost += BaseData.EvaluateCost(currentLevel + i);
        }

        return totalCost;
    }

    public int GetMaxAffordableUpgradeCount(double availableMoney)
    {
        if (BaseData == null) return 0;

        double remainingMoney = availableMoney;
        int affordableCount = 0;

        while (true)
        {
            double nextCost = BaseData.EvaluateCost(currentLevel + affordableCount);
            if (remainingMoney < nextCost)
            {
                break;
            }

            remainingMoney -= nextCost;
            affordableCount++;

            if (affordableCount >= 100000)
            {
                break;
            }
        }

        return affordableCount;
    }

    public bool CanAffordUpgradeAmount(int amount)
    {
        if (CurrencyManager.Instance == null || BaseData == null)
        {
            return false;
        }

        if (amount >= int.MaxValue)
        {
            return GetMaxAffordableUpgradeCount(CurrencyManager.Instance.currentMoney) > 0;
        }

        int safeAmount = Mathf.Max(1, amount);
        return CurrencyManager.Instance.currentMoney >= GetTotalCostForUpgrades(safeAmount);
    }

    public virtual bool TryUpgrade()
    {
        if (BaseData == null || IsMaxLevel) return false;

        double cost = CurrentCost();

        if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendMoney(cost))
        {
            currentLevel++;
            OnUpgraded();
            return true;
        }

        return false;
    }

    protected abstract void OnUpgraded();
}
