using UnityEngine;

[CreateAssetMenu(fileName = "New Click Upgrade", menuName = "IdleGame/Click Upgrade")]
public class ClickUpgradeData : UpgradableEntityData
{
    [Header("Tiklama Ayarlari")]
    // Her level'da tiklama basina ne kadar para kazanilacagini ScaledValue hesaplar.
    public ScaledValue rewardPerLevel = new ScaledValue();
}
