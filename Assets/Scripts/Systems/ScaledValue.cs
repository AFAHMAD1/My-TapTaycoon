using UnityEngine;

public enum ScaleMode
{
    Exponential,
    Linear
}

[System.Serializable]
public class ScaledValue
{
    [Tooltip("Hesaplama Yontemi. Linear dogrusal artar, Exponential bilesik artar.")]
    public ScaleMode mode = ScaleMode.Exponential;

    // Sadece Editor tarafinda CustomPropertyDrawer tarafindan kullanilacak
    public string baseValueString = "";

    public double baseValue = 1d;

    [Tooltip("Artis orani. Exponential icin carpandir (1.08 = %8 artis). Linear icin ise her seviyede uzerine eklenecek sabit degerdir (orn: 10).")]
    [Min(0f)]
    public float multiplierPerLevel = 1.15f;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double Evaluate(int level)
    {
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'safeLevel' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        int safeLevel = Mathf.Max(0, level);
        
        switch (mode)
        {
            case ScaleMode.Linear:
                return baseValue + (multiplierPerLevel * safeLevel);
            case ScaleMode.Exponential:
            default:
                return baseValue * System.Math.Pow(multiplierPerLevel, safeLevel);
        }
    }
}
