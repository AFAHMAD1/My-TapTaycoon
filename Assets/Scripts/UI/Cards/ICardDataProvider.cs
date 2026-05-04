using UnityEngine;

public interface ICardDataProvider
{
    string DisplayName { get; }
    int CurrentLevel { get; }
    string Description { get; }
    
    Sprite Icon { get; }
    Color CardColor { get; }
    Color ButtonColor { get; }
    Color IconTintColor { get; }
    Color IncomeColor { get; }

    CardDisplayConfig DisplayConfig { get; }

    double GetCost(int amount);
    double GetIncomePerCycle();
    bool CanAfford(int amount);
    void Purchase(int amount);
    
    bool HasProgressBar();
    float GetProgressNormalized();
    string GetProgressText();
    void OnProgressClick();

    string GetBuyButtonText(int amount);
    
    // Ikincil buton (Orn: Prestij)
    string GetSecondaryButtonText() => "Prestij!";
    void OnSecondaryButtonClick() { }
}
