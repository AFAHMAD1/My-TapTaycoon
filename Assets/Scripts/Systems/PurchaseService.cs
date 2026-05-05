using UnityEngine;

public static class PurchaseService
{
    public static System.Action OnAnyPurchaseCompleted;

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public static int ResolvePurchaseCount(UpgradableEntity entity, int requestedAmount)
    {
        if (entity == null)
        {
            return 0;
        }

        if (requestedAmount >= int.MaxValue)
        {
            double availableMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentMoney : 0d;
            return entity.GetMaxAffordableUpgradeCount(availableMoney);
        }

        return Mathf.Max(1, requestedAmount);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public static PurchaseResult TryPurchase(UpgradableEntity entity, int requestedAmount)
    {
        PurchaseResult result = new PurchaseResult();

        if (entity == null)
        {
            return result;
        }

        int purchaseCount = ResolvePurchaseCount(entity, requestedAmount);
        if (purchaseCount <= 0)
        {
            return result;
        }

        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < purchaseCount; i++)
        {
            double levelCost = entity.CurrentCost();
            if (!entity.TryUpgrade())
            {
                break;
            }

            result.PurchasedCount++;
            result.TotalSpent += levelCost;
        }

        if (result.HasPurchased)
        {
            OnAnyPurchaseCompleted?.Invoke();
        }

        return result;
    }
}

public struct PurchaseResult
{
    public int PurchasedCount;
    public double TotalSpent;

    public bool HasPurchased => PurchasedCount > 0;
}
