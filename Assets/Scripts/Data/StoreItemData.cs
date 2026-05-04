using UnityEngine;

public enum StoreCostType
{
    Diamond,
    RealMoneyTRY,
    Free
}

public enum StoreRewardType
{
    TimeSkipMoney,    // Zaman atlamasi (orn: 24 saatlik gelir)
    CooldownReset,    // Beceri sifirlama
    SocialMediaReward,// Elmas odulu (takip et)
    DiamondPack       // Gercek parayla elmas alimi
}

[CreateAssetMenu(fileName = "New Store Item", menuName = "IdleGame/Store Item (Kit)")]
public class StoreItemData : ScriptableObject
{
    [Header("Kart Bilgileri")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("Ucret Ayarlari")]
    public StoreCostType costType;
    public float costAmount; // Elmas miktari veya TL fiyati
    public string customButtonText; // Orn: "Bizi Takip Et" (Bos birakilirsa otomatik fiyat yazar)

    [Header("Odul Ayarlari")]
    public StoreRewardType rewardType;
    public float rewardAmount; // 24 (saat) veya 50 (elmas) vs.

    [Header("Gorsel Tema")]
    public Color cardBackgroundColor = new Color32(240, 240, 240, 255);
    public Color buttonColor = new Color32(32, 151, 220, 255);
    public Color iconTintColor = Color.white;
}
