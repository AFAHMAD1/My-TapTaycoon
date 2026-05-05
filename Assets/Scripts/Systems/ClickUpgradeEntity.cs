using UnityEngine;

/// <summary>
/// Tıklama (veya Collector) gücü yükseltme verilerini tutan sınıf.
/// </summary>
[System.Serializable]
public class ClickUpgradeEntity : UpgradableEntity
{
    public ClickUpgradeData data;
    public override UpgradableEntityData BaseData => data;

    /// <summary>
    /// Mevcut seviyeye göre her toplamada verilecek para miktarını döner.
    /// </summary>
    public double CurrentReward()
    {
        return data != null ? data.rewardPerLevel.Evaluate(currentLevel) : 1d;
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    protected override void OnUpgraded()
    {
        // Seviye atlayınca görsel efekt oynat (Karakterin bulunduğu yer veya ekranın ortası)
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayLevelUpEffect(Vector3.zero);
        }
    }
}
