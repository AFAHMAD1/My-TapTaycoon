using UnityEngine;

[System.Serializable]
public class BoostUpgrade : UpgradableEntity
{
    public BoostUpgradeData data;
    public override UpgradableEntityData BaseData => data;

    public bool IsUnlocked => true; 

    protected override void OnUpgraded()
    {
    }
}
