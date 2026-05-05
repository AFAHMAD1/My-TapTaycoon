using UnityEngine;

[System.Serializable]
public class BoostUpgrade : UpgradableEntity
{
    public BoostUpgradeData data;
    public override UpgradableEntityData BaseData => data;

    public bool IsUnlocked => true; 

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    protected override void OnUpgraded()
    {
    }
}
