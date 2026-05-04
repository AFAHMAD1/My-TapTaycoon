using UnityEngine;

public abstract class UpgradableEntityData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string entityName;

    [Header("Ekonomi")]
    public ScaledValue upgradeCost = new ScaledValue();
    public int maxLevel = 0; // 0 ise sinirsiz

    public double EvaluateCost(int level)
    {
        return upgradeCost.Evaluate(level);
    }
}
