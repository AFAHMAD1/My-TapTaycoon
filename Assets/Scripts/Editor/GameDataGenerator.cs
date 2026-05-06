#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GameDataGenerator : EditorWindow
{
    [MenuItem("IdleGame/Otomatik Veri Olusturucu")]
    public static void GenerateData()
    {
        string dataFolder = "Assets/Scripts/Data/Defaults";
        if (!AssetDatabase.IsValidFolder("Assets/Scripts")) AssetDatabase.CreateFolder("Assets", "Scripts");
        if (!AssetDatabase.IsValidFolder("Assets/Scripts/Data")) AssetDatabase.CreateFolder("Assets/Scripts", "Data");
        if (!AssetDatabase.IsValidFolder(dataFolder)) AssetDatabase.CreateFolder("Assets/Scripts/Data", "Defaults");

        // 1. Kit (Magaza) Kartlari
        CreateStoreItem("Yagdir", "24 saat değerinde para düşür: 10.08M", StoreCostType.Diamond, 100f, StoreRewardType.TimeSkipMoney, 24f, "");
        CreateStoreItem("Beceri Sifirlama", "Beceri sıfırlama CD2", StoreCostType.Free, 0f, StoreRewardType.CooldownReset, 0f, "");
        CreateStoreItem("Sosyal Medya", "Bizi takip et, elmas kazan!", StoreCostType.Free, 0f, StoreRewardType.SocialMediaReward, 0f, "Bizi Takip Et");
        CreateStoreItem("Elmas Paketi", "Oyun ici elmas satin al", StoreCostType.RealMoneyTRY, 19.99f, StoreRewardType.DiamondPack, 0f, "");

        // 2. Oyuncu (Boost) Kartlari
        for (int i = 1; i <= 10; i++)
        {
            CreateBoostItem($"Bina Boost {i}", $"Secili binanin gelirini {i + 1} kat arttirir.", i + 1, 500 * i);
        }

        // Bu satir: 'AssetDatabase' objesi uzerindeki 'SaveAssets' metodunu cagirir; Editor'da olusturulan/degisen assetleri kaydeder.
        AssetDatabase.SaveAssets();
        // Bu satir: 'AssetDatabase' objesi uzerindeki 'Refresh' metodunu cagirir; gorunumu veya veriyi guncel hale getirir.
        AssetDatabase.Refresh();
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log("Tüm veriler 'Assets/Scripts/Data/Defaults' klasörüne otomatik oluşturuldu! Gidip listelere sürükleyebilirsin.");
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private static void CreateStoreItem(string name, string desc, StoreCostType cType, float cost, StoreRewardType rType, float reward, string btnText)
    {
        string path = $"Assets/Scripts/Data/Defaults/StoreItem_{name.Replace(" ", "")}.asset";
        if (AssetDatabase.LoadAssetAtPath<StoreItemData>(path) != null) return;

        StoreItemData asset = ScriptableObject.CreateInstance<StoreItemData>();
        asset.itemName = name;
        asset.description = desc;
        asset.costType = cType;
        asset.costAmount = cost;
        asset.rewardType = rType;
        asset.rewardAmount = reward;
        asset.customButtonText = btnText;

        // Bu satir: 'AssetDatabase' objesi uzerindeki 'CreateAsset' metodunu cagirir; Unity Editor icinde ScriptableObject asset dosyasi olusturur.
        AssetDatabase.CreateAsset(asset, path);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private static void CreateBoostItem(string name, string desc, float multiplier, float cost)
    {
        string path = $"Assets/Scripts/Data/Defaults/BoostItem_{name.Replace(" ", "")}.asset";
        if (AssetDatabase.LoadAssetAtPath<BoostUpgradeData>(path) != null) return;

        BoostUpgradeData asset = ScriptableObject.CreateInstance<BoostUpgradeData>();
        asset.entityName = name;
        asset.description = desc;
        asset.boostMultiplier = multiplier;
        asset.maxLevel = 1;
        asset.upgradeCost.baseValue = cost;

        // Bu satir: 'AssetDatabase' objesi uzerindeki 'CreateAsset' metodunu cagirir; Unity Editor icinde ScriptableObject asset dosyasi olusturur.
        AssetDatabase.CreateAsset(asset, path);
    }
}
#endif
