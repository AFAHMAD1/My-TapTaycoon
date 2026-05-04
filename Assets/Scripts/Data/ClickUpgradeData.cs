using UnityEngine;

[CreateAssetMenu(fileName = "New Click Upgrade", menuName = "IdleGame/Click Upgrade")]
public class ClickUpgradeData : UpgradableEntityData
{
    [Header("Tiklama Ayarlari")]
    public ScaledValue rewardPerLevel = new ScaledValue();
}
