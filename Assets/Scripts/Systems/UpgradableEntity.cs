using UnityEngine;

/// <summary>
/// Tum satin alinabilir ve gelistirilebilir nesnelerin State (Durum) tabani.
/// Binalar, ozellikler bu sinifi miras alarak kendi ScriptableObject verilerini referans gosterir.
/// </summary>
[System.Serializable]
public abstract class UpgradableEntity
{
    public int currentLevel = 0;

    // Alt siniflar kendi SO data'larini dondurecek
    public abstract UpgradableEntityData BaseData { get; }
    
    public bool IsMaxLevel => BaseData != null && BaseData.maxLevel > 0 && currentLevel >= BaseData.maxLevel;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double CurrentCost()
    {
        if (BaseData == null) return 0d;
        // Bu satir: 'BaseData' objesi uzerindeki 'EvaluateCost' metodunu cagirir; verilen level icin upgrade maliyetini hesaplar.
        return BaseData.EvaluateCost(currentLevel);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool CanAffordUpgrade()
    {
        return CurrencyManager.Instance != null && CurrencyManager.Instance.currentMoney >= CurrentCost();
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetTotalCostForUpgrades(int amount)
    {
        if (BaseData == null) return 0d;
        
        // Bu satir: 'Mathf' uzerindeki 'Min' metodunu cagirir ve sonucu 'safeAmount' degiskenine koyar; iki degerden kucuk olani secer; burada genelde ust sinir koymak icin kullanilir.
        int safeAmount = Mathf.Min(Mathf.Max(0, amount), GetRemainingUpgradeCount());
        double totalCost = 0d;

        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < safeAmount; i++)
        {
            totalCost += BaseData.EvaluateCost(currentLevel + i);
        }

        return totalCost;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public int GetMaxAffordableUpgradeCount(double availableMoney)
    {
        if (BaseData == null) return 0;

        double remainingMoney = availableMoney;
        int affordableCount = 0;
        int remainingUpgradeCount = GetRemainingUpgradeCount();

        // Bu dongu kosul dogru kaldigi surece calisir; kosul bozulunca durur.
        while (affordableCount < remainingUpgradeCount)
        {
            // Bu satir: 'BaseData' uzerindeki 'EvaluateCost' metodunu cagirir ve sonucu 'nextCost' degiskenine koyar; verilen level icin maliyet egrisini kullanarak fiyat hesaplar.
            double nextCost = BaseData.EvaluateCost(currentLevel + affordableCount);
            if (remainingMoney < nextCost)
            {
                break;
            }

            remainingMoney -= nextCost;
            affordableCount++;

            if (affordableCount >= 100000)
            {
                break;
            }
        }

        return affordableCount;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public int GetRemainingUpgradeCount()
    {
        if (BaseData == null)
        {
            return 0;
        }

        if (BaseData.maxLevel <= 0)
        {
            return int.MaxValue;
        }

        // Bu satir: 'Mathf' objesi uzerindeki 'Max' metodunu cagirir; verilen degerlerden buyuk olani secer; minimum sinir koymak icin kullanilir.
        return Mathf.Max(0, BaseData.maxLevel - currentLevel);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool CanAffordUpgradeAmount(int amount)
    {
        if (CurrencyManager.Instance == null || BaseData == null)
        {
            return false;
        }

        if (amount >= int.MaxValue)
        {
            return GetMaxAffordableUpgradeCount(CurrencyManager.Instance.currentMoney) > 0;
        }

        // Bu satir: 'Mathf' uzerindeki 'Min' metodunu cagirir ve sonucu 'safeAmount' degiskenine koyar; iki degerden kucuk olani secer; burada genelde ust sinir koymak icin kullanilir.
        int safeAmount = Mathf.Min(Mathf.Max(1, amount), GetRemainingUpgradeCount());
        if (safeAmount <= 0)
        {
            return false;
        }

        return CurrencyManager.Instance.currentMoney >= GetTotalCostForUpgrades(safeAmount);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public virtual bool TryUpgrade()
    {
        if (BaseData == null || IsMaxLevel) return false;

        double cost = CurrentCost();

        if (CurrencyManager.Instance != null && CurrencyManager.Instance.SpendMoney(cost))
        {
            currentLevel++;
            OnUpgraded();
            return true;
        }

        return false;
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    protected abstract void OnUpgraded();
}
