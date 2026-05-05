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
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
            for (int i = 1; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath += "/" + folders[i];
            }
        }

        CreateBuilding("Kafeterya", 50, 1.08f, 10, 10f, ScaleMode.Linear, 10f, new Color32(247, 224, 179, 255), new Color32(222, 119, 55, 255), new Color32(47, 125, 211, 255), new Color32(98, 156, 64, 255), true);
        CreateBuilding("Bilet Gisesi", 100, 1.15f, 5, 1.35f, ScaleMode.Exponential, 5f, new Color32(231, 217, 181, 255), new Color32(194, 48, 48, 255), new Color32(32, 151, 220, 255), new Color32(78, 154, 67, 255), false);
        CreateBuilding("Kulup Muzesi", 750, 1.15f, 42, 1.38f, ScaleMode.Exponential, 7f, new Color32(226, 220, 196, 255), new Color32(139, 93, 46, 255), new Color32(43, 129, 210, 255), new Color32(91, 160, 84, 255), false);
        CreateBuilding("Antrenman Tesisi", 25000, 1.15f, 1500, 1.42f, ScaleMode.Exponential, 10f, new Color32(217, 230, 205, 255), new Color32(70, 140, 74, 255), new Color32(40, 127, 201, 255), new Color32(66, 150, 72, 255), false);
        CreateBuilding("Taraftar Magazasi", 5000, 1.15f, 300, 1.4f, ScaleMode.Exponential, 8f, new Color32(228, 218, 189, 255), new Color32(154, 58, 136, 255), new Color32(46, 115, 201, 255), new Color32(72, 145, 72, 255), false);
        CreateBuilding("Otopark", 12000, 1.15f, 820, 1.4f, ScaleMode.Exponential, 9f, new Color32(219, 225, 210, 255), new Color32(94, 114, 126, 255), new Color32(55, 132, 206, 255), new Color32(74, 147, 77, 255), false);
        CreateBuilding("Stadyum", 1000000, 1.15f, 80000, 1.38f, ScaleMode.Exponential, 15f, new Color32(209, 217, 193, 255), new Color32(50, 128, 105, 255), new Color32(33, 122, 197, 255), new Color32(65, 149, 76, 255), false);
        CreateBuilding("Medya Merkezi", 18000000, 1.15f, 1400000, 1.36f, ScaleMode.Exponential, 18f, new Color32(220, 213, 233, 255), new Color32(103, 77, 165, 255), new Color32(73, 127, 226, 255), new Color32(74, 163, 93, 255), false);
        CreateBuilding("Alisveris Merkezi", 75000000, 1.15f, 6200000, 1.34f, ScaleMode.Exponential, 20f, new Color32(243, 222, 206, 255), new Color32(207, 115, 66, 255), new Color32(48, 153, 214, 255), new Color32(83, 166, 89, 255), false);
        CreateBuilding("Mega Arena", 4000000000, 1.15f, 420000000, 1.32f, ScaleMode.Exponential, 28f, new Color32(236, 217, 196, 255), new Color32(133, 88, 56, 255), new Color32(24, 105, 191, 255), new Color32(99, 181, 75, 255), false);

        CreateCollectorData();
        CreateBoosts();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
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
            AssetDatabase.CreateAsset(data, boost1Path);
        }

        string boost2Path = "Assets/Scripts/Data/Defaults/Boost_OtoTopla.asset";
        if (AssetDatabase.LoadAssetAtPath<BoostUpgradeData>(boost2Path) == null)
        {
            BoostUpgradeData data = ScriptableObject.CreateInstance<BoostUpgradeData>();
            data.entityName = "Otomatik Topla";
            data.description = "Restoranin karini oto topla";
            data.boostMultiplier = 0f;
            data.upgradeCost = new ScaledValue { mode = ScaleMode.Exponential, baseValue = 50000d, multiplierPerLevel = 1f };
            AssetDatabase.CreateAsset(data, boost2Path);
        }
    }
}
