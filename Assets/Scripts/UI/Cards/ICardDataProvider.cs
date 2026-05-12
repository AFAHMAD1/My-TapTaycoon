using UnityEngine;

public interface ICardDataProvider
{
    // UpgradeCard bu ozellikleri okuyarak kartin baslik, ikon, renk ve seviye yazilarini doldurur.
    string DisplayName { get; }
    int CurrentLevel { get; }
    string Description { get; }
    
    Sprite Icon { get; }
    Color CardColor { get; }
    Color ButtonColor { get; }
    Color IconTintColor { get; }
    Color IncomeColor { get; }

    CardDisplayConfig DisplayConfig { get; }

    // UpgradeCard satin alma butonu icin bu metotlarla fiyat, gelir ve para yetme durumunu sorar.
    double GetCost(int amount);
    double GetIncomePerCycle();
    bool CanAfford(int amount);
    void Purchase(int amount);
    
    // Progress bar gerekiyorsa kart bu metotlarla doluluk oranini ve tiklama davranisini alir.
    bool HasProgressBar();
    float GetProgressNormalized();
    string GetProgressText();
    void OnProgressClick();

    // Satin alma butonunda yazacak metni verinin turune gore uretir.
    string GetBuyButtonText(int amount);
    
    // Ikincil buton (Orn: Prestij)
    string GetSecondaryButtonText() => "Prestij!";
    void OnSecondaryButtonClick() { }
}

public interface IStoreCardVisualProvider
{
    StoreCardVisualType VisualType { get; }
    bool ShowPriceRow { get; }
    string PriceText { get; }
    Sprite PriceIcon { get; }
    string LeftActionText { get; }
    Vector2 ButtonSize { get; }
}
