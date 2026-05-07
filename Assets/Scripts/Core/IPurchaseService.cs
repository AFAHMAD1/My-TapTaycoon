/// <summary>
/// Satin alma islemlerini gerceklestiren servisin sozlesmesini (contract) tanimlar.
/// Bu arayuzu kullanan kodlar, PurchaseService'in somut implementasyonuna
/// bagimli kalmaz — farkli bir satin alma stratejisiyle kolayca degistirilebilir.
/// </summary>
public interface IPurchaseService
{
    /// <summary>
    /// Verilen entity icin istenen miktarda seviye atlamayi dener.
    /// Sonucu PurchaseResult ile doner: kac seviye alindi ve ne kadar harcandi.
    /// </summary>
    PurchaseResult TryPurchase(UpgradableEntity entity, int requestedAmount);

    /// <summary>
    /// requestedAmount == int.MaxValue ise oyuncunun parasi yettigince
    /// kac seviye alinabilecegini hesaplar; diger durumlarda requestedAmount'u dogrudan dondurur.
    /// </summary>
    int ResolvePurchaseCount(UpgradableEntity entity, int requestedAmount);
}
