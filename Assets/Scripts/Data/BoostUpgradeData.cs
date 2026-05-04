using UnityEngine;

public enum BoostTargetType
{
    AllBuildings,
    SpecificBuilding,
    CollectorSpeed,
    CollectorCapacity
}

[CreateAssetMenu(fileName = "New Boost Upgrade", menuName = "IdleGame/Boost Upgrade")]
public class BoostUpgradeData : UpgradableEntityData
{
    public string description;
    
    [Header("Boost Etkisi")]
    public BoostTargetType targetType = BoostTargetType.AllBuildings;
    
    [Tooltip("Sadece SpecificBuilding seciliyse gecerli olur.")]
    public IncomeBuildingData targetBuilding;

    [Tooltip("Orn: 5 (5 kat daha fazla kazanc)")]
    public float boostMultiplier = 5f;

    [Header("Gorunum")]
    public Color cardBackgroundColor = new Color32(236, 217, 196, 255);
    public Color buttonColor = new Color32(32, 151, 220, 255);
}
