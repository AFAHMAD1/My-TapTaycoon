using UnityEngine;

public enum StoreCostType
{
    // Ucret oyun ici elmasla odenir.
    Diamond,
    // Ucret gercek para/TL olarak gosterilir.
    RealMoneyTRY,
    // Ucretsiz odul veya reklam/takip gibi islemler icin kullanilir.
    Free
}

public enum StoreRewardType
{
    // Belirli saat kadar pasif gelir verir.
    TimeSkipMoney,    // Zaman atlamasi (orn: 24 saatlik gelir)
    // Skill bekleme surelerini sifirlamak icin kullanilir.
    CooldownReset,    // Beceri sifirlama
    // Sosyal medya gibi aksiyonlardan elmas odulu verir.
    SocialMediaReward,// Elmas odulu (takip et)
    // Gercek para karsiligi elmas paketi verir.
    DiamondPack       // Gercek parayla elmas alimi
}

public enum StoreCardVisualType
{
    SocialMedia,
    DiamondFeature,
    DiamondPackTRY
}

[CreateAssetMenu(fileName = "New Store Item", menuName = "IdleGame/Store Item (Kit)")]
public class StoreItemData : ScriptableObject
{
    [Header("Kart Bilgileri")]
    // Magaza kartinda gosterilecek isimdir.
    public string itemName;
    // Magaza kartinin oyuncuya gosterecegi aciklama metnidir.
    [TextArea] public string description;
    public Sprite icon;
    
    [Header("Ucret Ayarlari")]
    // Bu urunun hangi para/takas tipiyle alinacagini belirler.
    public StoreCostType costType;
    public float costAmount; // Elmas miktari veya TL fiyati
    public string customButtonText; // Orn: "Bizi Takip Et" (Bos birakilirsa otomatik fiyat yazar)

    [Header("Odul Ayarlari")]
    // Satin alma tamamlaninca oyuncuya hangi odul verilecek bunu belirler.
    public StoreRewardType rewardType;
    public float rewardAmount; // 24 (saat) veya 50 (elmas) vs.

    [Header("Gorsel Tema")]
    public StoreCardVisualType visualType = StoreCardVisualType.DiamondFeature;
    public bool showPriceRow = true;
    public string priceTextOverride;
    public Sprite priceIcon;
    public Vector2 normalButtonSize = Vector2.zero;
    public Vector2 socialButtonSize = Vector2.zero;
    public Color cardBackgroundColor = new Color32(240, 240, 240, 255);
    public Color buttonColor = new Color32(32, 151, 220, 255);
    public Color socialButtonColor = new Color32(51, 153, 255, 255);
    public Color iconTintColor = Color.white;
}
