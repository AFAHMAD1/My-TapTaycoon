using UnityEngine;

[System.Serializable]
public class PlayerProfitUpgrade
{
    public string playerName = "Player";
    [TextArea] public string description = "Bought players multiply building profit.";
    public Sprite icon;

    [Min(0)]
    public int currentLevel;

    [Tooltip("0 means unlimited.")]
    [Min(0)]
    public int maxLevel = 1;

    [Header("Economy")]
    public double baseCost = 100d;
    public float costMultiplierPerLevel = 1.5f;

    [Tooltip("1.25 means each level gives x1.25 building profit.")]
    [Min(1f)]
    public float profitMultiplierPerLevel = 1.25f;

    [Header("Colors")]
    public Color cardBackgroundColor = new Color32(236, 217, 196, 255);
    public Color buttonColor = new Color32(32, 151, 220, 255);
    public Color iconTintColor = Color.white;
    public Color incomeColor = new Color32(78, 154, 67, 255);

    public bool IsMaxLevel => maxLevel > 0 && currentLevel >= maxLevel;

    public double CurrentMultiplier()
    {
        if (currentLevel <= 0)
        {
            return 1d;
        }

        return System.Math.Pow(Mathf.Max(1f, profitMultiplierPerLevel), currentLevel);
    }

    public double PreviewNextMultiplier()
    {
        return System.Math.Pow(Mathf.Max(1f, profitMultiplierPerLevel), currentLevel + 1);
    }

    public double CostAtLevel(int level)
    {
        int safeLevel = Mathf.Max(0, level);
        double cost = baseCost * System.Math.Pow(Mathf.Max(1f, costMultiplierPerLevel), safeLevel);
        return double.IsNaN(cost) || double.IsInfinity(cost) ? 0d : cost;
    }

    public double GetTotalCost(int amount)
    {
        int purchaseCount = ResolvePurchaseCount(amount);
        if (purchaseCount <= 0)
        {
            return CostAtLevel(currentLevel);
        }

        double total = 0d;
        for (int i = 0; i < purchaseCount; i++)
        {
            total += CostAtLevel(currentLevel + i);
        }

        return total;
    }

    public int ResolvePurchaseCount(int requestedAmount)
    {
        if (IsMaxLevel)
        {
            return 0;
        }

        int remaining = maxLevel > 0 ? maxLevel - currentLevel : int.MaxValue;
        if (requestedAmount >= int.MaxValue)
        {
            return GetMaxAffordableCount(remaining);
        }

        return Mathf.Min(Mathf.Max(1, requestedAmount), remaining);
    }

    private int GetMaxAffordableCount(int remaining)
    {
        if (CurrencyManager.Instance == null)
        {
            return 0;
        }

        double money = CurrencyManager.Instance.currentMoney;
        int count = 0;

        while (count < remaining && count < 100000)
        {
            double nextCost = CostAtLevel(currentLevel + count);
            if (money < nextCost)
            {
                break;
            }

            money -= nextCost;
            count++;
        }

        return count;
    }

    public bool CanAfford(int amount)
    {
        int purchaseCount = ResolvePurchaseCount(amount);
        return CurrencyManager.Instance != null &&
               purchaseCount > 0 &&
               CurrencyManager.Instance.currentMoney >= GetTotalCost(purchaseCount);
    }

    public bool TryPurchase(int amount)
    {
        int purchaseCount = ResolvePurchaseCount(amount);
        if (CurrencyManager.Instance == null || purchaseCount <= 0)
        {
            return false;
        }

        double totalCost = GetTotalCost(purchaseCount);
        if (!CurrencyManager.Instance.SpendMoney(totalCost))
        {
            return false;
        }

        currentLevel += purchaseCount;
        PurchaseService.OnAnyPurchaseCompleted?.Invoke();
        return true;
    }
}
