using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Income Building", menuName = "IdleGame/Income Building")]
public class IncomeBuildingData : UpgradableEntityData
{
    [Header("Bina Uretim Ayarlari")]
    public ScaledValue incomePerCycle = new ScaledValue { baseValue = 5d, multiplierPerLevel = 1.5f };

    [Min(0.2f)]
    public float baseDuration = 5f;

    [Tooltip("Belirli level'larda sureyi dogrudan degistirir. Ornek: Lv.1 = 50 sn, Lv.5 = 40 sn")]
    public List<DurationLevelStep> durationSteps = new List<DurationLevelStep>();

    [Tooltip("Sure dolunca para otomatik verilmez. Oyuncu slider veya hazir ikonundan toplar.")]
    public bool requireManualCollection = false;

    [Header("Sahne Gorseli")]
    public GameObject buildingPrefab;
    public Vector3 spawnOffset;

    [Header("Eski Sahne Objesi Destegi")]
    public GameObject buildingVisualObject;

    [Header("Kart Temasi")]
    public Sprite cardIconSprite;
    public Color cardBackgroundColor = new Color32(231, 217, 181, 255);
    public Color iconTintColor = new Color32(194, 48, 48, 255);
    public Color buttonColor = new Color32(32, 151, 220, 255);
    public Color incomeColor = new Color32(78, 154, 67, 255);

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public float GetDurationForLevel(int level)
    {
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'duration' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        float duration = Mathf.Max(0.2f, baseDuration);
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'safeLevel' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        int safeLevel = Mathf.Max(1, level);

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (DurationLevelStep step in durationSteps)
        {
            if (step != null && step.requiredLevel <= safeLevel)
            {
                // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'duration' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
                duration = Mathf.Max(0.2f, step.durationSeconds);
            }
        }

        return duration;
    }
}
