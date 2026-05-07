using UnityEngine;

/// <summary>
/// Satin alma islemlerinin merkezi servisi.
/// 
/// Tasarim Notu:
/// Bu sinif eskiden 'static class' idi. Ancak static siniflar C#'ta arayuz uygulayamaz.
/// Bu nedenle 'static class' -> normal 'class' + 'static Instance' singleton seklinde
/// yeniden duzenlendi. Mevcut tum cagri noktalari (PurchaseService.TryPurchase vb.)
/// degistirilmeden calismayi surdurmektedir.
/// </summary>
public class PurchaseService : IPurchaseService
{
    // --- Singleton ---
    // Arayuz uzerinden kullanim icin: PurchaseService.Instance.TryPurchase(...)
    // Dogrudan static kullanim icin: PurchaseService.TryPurchase(...)  (geri uyumluluk)
    public static readonly PurchaseService Instance = new PurchaseService();

    // Her satın alma tamamlandığında tüm UI panelleri bu event'i dinler
    public static System.Action OnAnyPurchaseCompleted;

    // --- Mevcut Static API (Geri Uyumluluk) ---
    // Bu metodlar degistirilmeden kaldi — mevcut hicbir cagri noktasi bozulmaz.

    public static int ResolvePurchaseCount(UpgradableEntity entity, int requestedAmount)
    {
        if (entity == null) return 0;

        if (requestedAmount >= int.MaxValue)
        {
            double availableMoney = CurrencyManager.Instance != null
                ? CurrencyManager.Instance.currentMoney
                : 0d;
            return entity.GetMaxAffordableUpgradeCount(availableMoney);
        }

        return Mathf.Max(1, requestedAmount);
    }

    public static PurchaseResult TryPurchase(UpgradableEntity entity, int requestedAmount)
    {
        PurchaseResult result = new PurchaseResult();

        if (entity == null) return result;

        int purchaseCount = ResolvePurchaseCount(entity, requestedAmount);
        if (purchaseCount <= 0) return result;

        for (int i = 0; i < purchaseCount; i++)
        {
            double levelCost = entity.CurrentCost();
            if (!entity.TryUpgrade()) break;

            result.PurchasedCount++;
            result.TotalSpent += levelCost;
        }

        if (result.HasPurchased)
        {
            OnAnyPurchaseCompleted?.Invoke();
        }

        return result;
    }

    // --- IPurchaseService Uygulamasi ---
    // Explicit interface implementation: dogrudan instance uzerinden erisimde kullanilir.
    // Static metodlara delege ederek kod tekrari onlenir.

    PurchaseResult IPurchaseService.TryPurchase(UpgradableEntity entity, int requestedAmount)
        => TryPurchase(entity, requestedAmount);

    int IPurchaseService.ResolvePurchaseCount(UpgradableEntity entity, int requestedAmount)
        => ResolvePurchaseCount(entity, requestedAmount);
}

public struct PurchaseResult
{
    public int PurchasedCount;
    public double TotalSpent;

    public bool HasPurchased => PurchasedCount > 0;
}
