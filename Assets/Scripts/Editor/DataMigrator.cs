using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DataMigrator : EditorWindow
{
    [MenuItem("IdleGame/Create Default Data Assets")]
    public static void CreateDefaultDataAssets()
    {
        string path = "Assets/Scripts/Data/Defaults";
        if (!AssetDatabase.IsValidFolder(path))
        {
            // Bu satir: 'path' uzerindeki 'Split' metodunu cagirir ve donen sonucu 'folders' degiskenine kaydeder.
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
            for (int i = 1; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                {
                    // Bu satir: 'AssetDatabase' objesi uzerindeki 'CreateFolder' metodunu cagirir; Unity proje klasorleri icinde yeni klasor olusturur.
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath += "/" + folders[i];
            }
        }

        CreateBuilding("Kafeterya", 50, 1.08f, 10, 10f, ScaleMode.Linear, 10f, new Color32(247, 224, 179, 255), new Color32(222, 119, 55, 255), new Color32(47, 125, 211, 255), new Color32(98, 156, 64, 255), true);
        CreateBuilding("Bilet Gisesi", 700, 1.15f, 60, 1.35f, ScaleMode.Exponential, 5f, new Color32(231, 217, 181, 255), new Color32(194, 48, 48, 255), new Color32(32, 151, 220, 255), new Color32(78, 154, 67, 255), false);
        CreateBuilding("Kulup Muzesi", 10500, 1.15f, 1000, 1.38f, ScaleMode.Exponential, 7f, new Color32(226, 220, 196, 255), new Color32(139, 93, 46, 255), new Color32(43, 129, 210, 255), new Color32(91, 160, 84, 255), false);
        CreateBuilding("Antrenman Tesisi", 189000, 1.15f, 10000, 1.42f, ScaleMode.Exponential, 10f, new Color32(217, 230, 205, 255), new Color32(70, 140, 74, 255), new Color32(40, 127, 201, 255), new Color32(66, 150, 72, 255), false);
        CreateBuilding("Taraftar Magazasi", 3780000, 1.15f, 175000, 1.4f, ScaleMode.Exponential, 8f, new Color32(228, 218, 189, 255), new Color32(154, 58, 136, 255), new Color32(46, 115, 201, 255), new Color32(72, 145, 72, 255), false);
        CreateBuilding("Otopark", 83200000, 1.15f, 1100000, 1.4f, ScaleMode.Exponential, 9f, new Color32(219, 225, 210, 255), new Color32(94, 114, 126, 255), new Color32(55, 132, 206, 255), new Color32(74, 147, 77, 255), false);
        CreateBuilding("Stadyum", 2080, 1.15f, 9370000, 1.38f, ScaleMode.Exponential, 15f, new Color32(209, 217, 193, 255), new Color32(50, 128, 105, 255), new Color32(33, 122, 197, 255), new Color32(65, 149, 76, 255), false);
        CreateBuilding("Medya Merkezi", 62400000000, 1.15f, 70000000, 1.36f, ScaleMode.Exponential, 18f, new Color32(220, 213, 233, 255), new Color32(103, 77, 165, 255), new Color32(73, 127, 226, 255), new Color32(74, 163, 93, 255), false);
        CreateBuilding("Alisveris Merkezi", 2490000000000, 1.15f, 581250000, 1.34f, ScaleMode.Exponential, 20f, new Color32(243, 222, 206, 255), new Color32(207, 115, 66, 255), new Color32(48, 153, 214, 255), new Color32(83, 166, 89, 255), false);
        CreateBuilding("Mega Arena", 125000000000000, 1.15f, 4240000000, 1.32f, ScaleMode.Exponential, 28f, new Color32(236, 217, 196, 255), new Color32(133, 88, 56, 255), new Color32(24, 105, 191, 255), new Color32(99, 181, 75, 255), false);

        CreateCollectorData();
        CreateBoosts();

        // Bu satir: 'AssetDatabase' objesi uzerindeki 'SaveAssets' metodunu cagirir; Editor'da olusturulan/degisen assetleri kaydeder.
        AssetDatabase.SaveAssets();
        // Bu satir: 'AssetDatabase' objesi uzerindeki 'Refresh' metodunu cagirir; gorunumu veya veriyi guncel hale getirir.
        AssetDatabase.Refresh();
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log("Tum default veriler ScriptableObject olarak Assets/Scripts/Data/Defaults klasorune eklendi!");
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private static void CreateBuilding(string name, double baseCost, float costMultiplier, double baseIncome, float incomeMultiplier, ScaleMode incomeMode, float duration, Color cardBackgroundColor, Color iconTintColor, Color buttonColor, Color incomeColor, bool requireManualCollection)
    {
        string assetPath = $"Assets/Scripts/Data/Defaults/Building_{name.Replace(" ", "")}.asset";
        if (AssetDatabase.LoadAssetAtPath<IncomeBuildingData>(assetPath) != null) return; // Zaten varsa pas gec

        IncomeBuildingData data = ScriptableObject.CreateInstance<IncomeBuildingData>();
        data.entityName = name;
        data.requireManualCollection = requireManualCollection;
        data.baseDuration = duration;
        data.cardBackgroundColor = cardBackgroundColor;
        data.iconTintColor = iconTintColor;
        data.buttonColor = buttonColor;
        data.incomeColor = incomeColor;

        data.durationSteps = new List<DurationLevelStep>
        {
            new DurationLevelStep { requiredLevel = 1, durationSeconds = duration },
            new DurationLevelStep { requiredLevel = 5, durationSeconds = Mathf.Max(0.2f, duration * 0.85f) },
            new DurationLevelStep { requiredLevel = 10, durationSeconds = Mathf.Max(0.2f, duration * 0.72f) },
            new DurationLevelStep { requiredLevel = 25, durationSeconds = Mathf.Max(0.2f, duration * 0.6f) }
        };

        data.incomePerCycle = new ScaledValue
        {
            mode = incomeMode,
            baseValue = baseIncome,
            multiplierPerLevel = incomeMultiplier
        };

        data.upgradeCost = new ScaledValue
        {
            mode = ScaleMode.Exponential,
            baseValue = baseCost,
            multiplierPerLevel = costMultiplier
        };

        // Bu satir: 'AssetDatabase' objesi uzerindeki 'CreateAsset' metodunu cagirir; Unity Editor icinde ScriptableObject asset dosyasi olusturur.
        AssetDatabase.CreateAsset(data, assetPath);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private static void CreateCollectorData()
    {
        string assetPath = "Assets/Scripts/Data/Defaults/ClickUpgrade_Collector.asset";
        if (AssetDatabase.LoadAssetAtPath<ClickUpgradeData>(assetPath) != null) return;

        ClickUpgradeData data = ScriptableObject.CreateInstance<ClickUpgradeData>();
        data.entityName = "Ekrana Tiklama";
        data.rewardPerLevel = new ScaledValue { mode = ScaleMode.Exponential, baseValue = 1d, multiplierPerLevel = 1.5f };
        data.upgradeCost = new ScaledValue { mode = ScaleMode.Exponential, baseValue = 25d, multiplierPerLevel = 1.8f };

        // Bu satir: 'AssetDatabase' objesi uzerindeki 'CreateAsset' metodunu cagirir; Unity Editor icinde ScriptableObject asset dosyasi olusturur.
        AssetDatabase.CreateAsset(data, assetPath);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private static void CreateBoosts()
    {
        string boost1Path = "Assets/Scripts/Data/Defaults/Boost_Restoran.asset";
        if (AssetDatabase.LoadAssetAtPath<BoostUpgradeData>(boost1Path) == null)
        {
            BoostUpgradeData data = ScriptableObject.CreateInstance<BoostUpgradeData>();
            data.entityName = "Restoran Yukseltmesi";
            data.description = "Restoran karini 5x arttir";
            data.boostMultiplier = 5f;
            data.upgradeCost = new ScaledValue { mode = ScaleMode.Exponential, baseValue = 50000d, multiplierPerLevel = 1f };
            // Bu satir: 'AssetDatabase' objesi uzerindeki 'CreateAsset' metodunu cagirir; Unity Editor icinde ScriptableObject asset dosyasi olusturur.
            AssetDatabase.CreateAsset(data, boost1Path);
        }

        string boost2Path = "Assets/Scripts/Data/Defaults/Boost_OtoTopla.asset";
        if (AssetDatabase.LoadAssetAtPath<BoostUpgradeData>(boost2Path) == null)
        {
            BoostUpgradeData data = ScriptableObject.CreateInstance<BoostUpgradeData>();
            data.entityName = "Otomatik Topla";
            data.description = "Restoranin karini oto topla";
            // [BUG-10 FIX] Was 0f — when UpgradeManager multiplies income by this value,
            // a 0x multiplier wipes ALL building income to zero the moment this boost is purchased.
            // Set to 1f (neutral) since this is a behavioural flag, not an income multiplier.
            data.boostMultiplier = 1f;
            data.upgradeCost = new ScaledValue { mode = ScaleMode.Exponential, baseValue = 50000d, multiplierPerLevel = 1f };
            // Bu satir: 'AssetDatabase' objesi uzerindeki 'CreateAsset' metodunu cagirir; Unity Editor icinde ScriptableObject asset dosyasi olusturur.
            AssetDatabase.CreateAsset(data, boost2Path);
        }
    }
}
