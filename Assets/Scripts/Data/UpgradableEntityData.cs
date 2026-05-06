using UnityEngine;

public abstract class UpgradableEntityData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string entityName;

    [Header("Ekonomi")]
    public ScaledValue upgradeCost = new ScaledValue();
    public int maxLevel = 0; // 0 ise sinirsiz

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double EvaluateCost(int level)
    {
        // Bu satir: 'upgradeCost' objesi uzerindeki 'Evaluate' metodunu cagirir; ScaledValue ayarlarina gore level bazli sayisal deger hesaplar.
        return upgradeCost.Evaluate(level);
    }
}
