using UnityEngine;

[CreateAssetMenu(fileName = "New Click Upgrade", menuName = "IdleGame/Click Upgrade")]
public class ClickUpgradeData : UpgradableEntityData
{
    [Header("Tiklama Ayarlari")]
    // Her level'da tiklama basina ne kadar para kazanilacagini ScaledValue hesaplar.
    public ScaledValue rewardPerLevel = new ScaledValue();

    [Header("Collector Fiyat Egrisi")]
    public bool useCollectorCostCurve = true;
    public double collectorStartCost = 20d;

    public override double EvaluateCost(int level)
    {
        if (!useCollectorCostCurve)
        {
            return base.EvaluateCost(level);
        }

        return EvaluateCollectorCost(level);
    }

    private double EvaluateCollectorCost(int level)
    {
        int safeLevel = Mathf.Max(0, level);
        double cost = collectorStartCost;

        for (int i = 0; i < safeLevel; i++)
        {
            cost += GetCollectorCostIncrease(i, cost);
        }

        return System.Math.Round(cost);
    }

    private double GetCollectorCostIncrease(int completedUpgradeIndex, double currentCost)
    {
        int displayLevel = completedUpgradeIndex + 1;

        if (displayLevel < 9)
        {
            return 1d;
        }

        if (displayLevel == 9)
        {
            return 2d;
        }

        if (currentCost < 40d)
        {
            return displayLevel % 2 == 0 ? 1d : 2d;
        }

        int level = displayLevel;
        if (level < 35) return 2d;
        if (level < 70) return 3d;
        if (level < 120) return 4d;
        if (level < 200) return 5d;
        if (level < 320) return 7d;
        if (level < 500) return 10d;

        return 10d + Mathf.FloorToInt((level - 500) / 150f) * 5d;
    }
}
