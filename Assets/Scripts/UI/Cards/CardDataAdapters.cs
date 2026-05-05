using UnityEngine;

public class BuildingCardAdapter : ICardDataProvider
{
    private IncomeBuilding building;
    private int index;
    private CardDisplayConfig config;

    public BuildingCardAdapter(IncomeBuilding building, int index)
    {
        this.building = building;
        this.index = index;
        this.config = CardDisplayConfig.DefaultBuildingConfig;
    }

    public string DisplayName => building.data != null ? building.data.entityName : "Bilinmiyor";
    public int CurrentLevel => building.currentLevel;
    public string Description => "";

    public Sprite Icon => building.data != null ? building.data.cardIconSprite : null;
    public Color CardColor => building.data != null ? building.data.cardBackgroundColor : Color.white;
    public Color ButtonColor => building.data != null ? building.data.buttonColor : Color.white;
    public Color IconTintColor => building.data != null ? building.data.iconTintColor : Color.white;
    public Color IncomeColor => building.data != null ? building.data.incomeColor : Color.white;

    public CardDisplayConfig DisplayConfig => config;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetCost(int amount)
    {
        if (amount >= int.MaxValue)
        {
            double money = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentMoney : 0d;
            int maxAffordable = building.GetMaxAffordableUpgradeCount(money);
            return maxAffordable > 0 ? building.GetTotalCostForUpgrades(maxAffordable) : building.CurrentCost();
        }
        return building.GetTotalCostForUpgrades(amount);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetIncomePerCycle()
    {
        if (building.data == null) return 0d;
        return building.currentLevel == 0 ? building.data.incomePerCycle.baseValue : building.CurrentIncome();
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool CanAfford(int amount)
    {
        return building.CanAffordUpgradeAmount(amount);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Purchase(int amount)
    {
        if (PassiveIncomeManager.Instance != null)
        {
            PassiveIncomeManager.Instance.BuyUpgrade(index, amount);
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool HasProgressBar() => true;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public float GetProgressNormalized()
    {
        if (building.currentLevel <= 0) return 0f;
        float duration = Mathf.Max(0.2f, building.CurrentDuration());
        return Mathf.Clamp(building.timer, 0f, duration) / duration;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetProgressText()
    {
        float duration = Mathf.Max(0.2f, building.CurrentDuration());
        if (building.currentLevel <= 0) return FormatRemainingTime(duration);
        
        if (building.data != null && building.data.requireManualCollection && building.isReadyToCollect)
            return "Toplamaya Hazir!";
            
        float remainingTime = Mathf.Max(0f, duration - building.timer);
        return FormatRemainingTime(remainingTime);
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void OnProgressClick()
    {
        if (PassiveIncomeManager.Instance != null)
        {
            PassiveIncomeManager.Instance.CollectIncome(index);
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetBuyButtonText(int amount)
    {
        string baseText = building.currentLevel == 0 ? "Satin Al" : "Yukselt";
        if (amount > 1 && amount < int.MaxValue) return $"{baseText} x{amount}";
        if (amount == int.MaxValue) return $"{baseText} MAX";
        return baseText;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    private string FormatRemainingTime(float seconds)
    {
        int roundedSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
        System.TimeSpan remaining = System.TimeSpan.FromSeconds(roundedSeconds);
        return $"{remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
    }
}

public class CollectorCardAdapter : ICardDataProvider
{
    private ClickUpgradeEntity collector;
    private CardDisplayConfig config;

    public CollectorCardAdapter(ClickUpgradeEntity collector)
    {
        this.collector = collector;
        this.config = CardDisplayConfig.DefaultCollectorConfig;
    }

    public string DisplayName => collector.data != null ? collector.data.entityName : "Tiklama";
    public int CurrentLevel => collector.currentLevel;
    public string Description => "";

    public Sprite Icon => null; // Varsayılan collector ikonu yok, varsa eklenecek
    public Color CardColor => new Color32(236, 217, 196, 255);
    public Color ButtonColor => new Color32(32, 151, 220, 255);
    public Color IconTintColor => Color.white;
    public Color IncomeColor => new Color32(78, 154, 67, 255);

    public CardDisplayConfig DisplayConfig => config;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetCost(int amount)
    {
        if (amount >= int.MaxValue)
        {
            double money = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentMoney : 0d;
            int maxAffordable = collector.GetMaxAffordableUpgradeCount(money);
            return maxAffordable > 0 ? collector.GetTotalCostForUpgrades(maxAffordable) : collector.CurrentCost();
        }
        return collector.GetTotalCostForUpgrades(amount);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetIncomePerCycle()
    {
        if (collector.data == null) return 0d;
        return collector.currentLevel == 0 ? collector.data.rewardPerLevel.baseValue : collector.CurrentReward();
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool CanAfford(int amount) => collector.CanAffordUpgradeAmount(amount);

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Purchase(int amount)
    {
        PurchaseService.TryPurchase(collector, amount);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool HasProgressBar() => false;
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public float GetProgressNormalized() => 0f;
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetProgressText() => "";
    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void OnProgressClick() { }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetBuyButtonText(int amount)
    {
        string baseText = "Yükselt";
        if (amount > 1 && amount < int.MaxValue) return $"{baseText} x{amount}";
        if (amount == int.MaxValue) return $"{baseText} MAX";
        return baseText;
    }
    
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetSecondaryButtonText() => "Prestij!";
    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void OnSecondaryButtonClick()
    {
        Debug.Log("Prestij butonuna tiklandi! (Ileride Prestij sistemi buraya baglanacak)");
    }
}

public class BoostCardAdapter : ICardDataProvider
{
    private BoostUpgrade boost;
    private CardDisplayConfig config;

    public BoostCardAdapter(BoostUpgrade boost)
    {
        this.boost = boost;
        this.config = CardDisplayConfig.DefaultFeatureConfig;
    }

    public string DisplayName => boost.data != null ? boost.data.entityName : "Boost";
    public int CurrentLevel => boost.currentLevel;
    public string Description => boost.data != null ? boost.data.description : "";

    public Sprite Icon => null;
    public Color CardColor => boost.data != null ? boost.data.cardBackgroundColor : Color.white;
    public Color ButtonColor => boost.data != null ? boost.data.buttonColor : Color.white;
    public Color IconTintColor => Color.white;
    public Color IncomeColor => Color.white;

    public CardDisplayConfig DisplayConfig => config;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetCost(int amount)
    {
        if (amount >= int.MaxValue)
        {
            double money = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentMoney : 0d;
            int maxAffordable = boost.GetMaxAffordableUpgradeCount(money);
            return maxAffordable > 0 ? boost.GetTotalCostForUpgrades(maxAffordable) : boost.CurrentCost();
        }
        return boost.GetTotalCostForUpgrades(amount);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetIncomePerCycle() => 0d;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool CanAfford(int amount) => boost.CanAffordUpgradeAmount(amount);

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Purchase(int amount)
    {
        PurchaseService.TryPurchase(boost, amount);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool HasProgressBar() => false;
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public float GetProgressNormalized() => 0f;
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetProgressText() => "";
    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void OnProgressClick() { }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetBuyButtonText(int amount)
    {
        string baseText = boost.currentLevel == 0 ? "Satın Al" : "Yükselt";
        if (amount > 1 && amount < int.MaxValue) return $"{baseText} x{amount}";
        if (amount == int.MaxValue) return $"{baseText} MAX";
        return baseText;
    }
}

public class StoreCardAdapter : ICardDataProvider
{
    private StoreItemData data;
    private CardDisplayConfig config;

    public StoreCardAdapter(StoreItemData itemData)
    {
        this.data = itemData;
        
        // Magaza karti (sadece aciklama ve ikon gosterilir, level/income gosterilmez)
        this.config = new CardDisplayConfig
        {
            showIcon = true,
            showProgressBar = false,
            showDescription = true,
            showIncome = false,
            showLevel = false,
            showSecondaryButton = false
        };
    }

    public string DisplayName => data.itemName;
    public int CurrentLevel => 0; // Magaza esyalarinin level'i yok
    public string Description => data.description;

    public Sprite Icon => data.icon;
    public Color CardColor => data.cardBackgroundColor;
    public Color ButtonColor => data.buttonColor;
    public Color IconTintColor => data.iconTintColor;
    public Color IncomeColor => Color.white;

    public CardDisplayConfig DisplayConfig => config;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetCost(int amount) => data.costAmount; // Elmas veya TL miktarini dondurur (arayuz icin)
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetIncomePerCycle() => 0d;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool CanAfford(int amount)
    {
        // TL veya Ucretsiz ise her zaman basilabilir (odeme ekrani acilir)
        if (data.costType == StoreCostType.RealMoneyTRY || data.costType == StoreCostType.Free) return true;
        
        // TODO: Elmas sistemi eklendiginde buradan kontrol edilecek
        // return CurrencyManager.Instance.currentDiamonds >= data.costAmount;
        return true; 
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Purchase(int amount)
    {
        Debug.Log($"[{data.itemName}] satin alindi/tiklandi!");
        
        // Odul mantigi
        switch (data.rewardType)
        {
            case StoreRewardType.TimeSkipMoney:
                if (PassiveIncomeManager.Instance != null && CurrencyManager.Instance != null)
                {
                    double totalIncomePerSec = PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond();
                    double skipReward = totalIncomePerSec * (data.rewardAmount * 3600); // Saat -> Saniye
                    CurrencyManager.Instance.AddMoney(skipReward, true);
                    Debug.Log($"{data.rewardAmount} saatlik kazanc eklendi: {skipReward}");
                }
                break;
                
            case StoreRewardType.SocialMediaReward:
                Application.OpenURL("https://instagram.com/oyunumuz");
                // TODO: Elmas ekle
                break;
                
            case StoreRewardType.CooldownReset:
                Debug.Log("Beceriler sifirlandi!");
                break;
                
            case StoreRewardType.DiamondPack:
                Debug.Log("Google Play / App Store odeme ekrani acilacak.");
                break;
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool HasProgressBar() => false;
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public float GetProgressNormalized() => 0f;
    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetProgressText() => "";
    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void OnProgressClick() { }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetBuyButtonText(int amount)
    {
        if (!string.IsNullOrEmpty(data.customButtonText)) return data.customButtonText;
        
        return data.costType switch
        {
            StoreCostType.Free => "Ücretsiz",
            StoreCostType.RealMoneyTRY => $"{data.costAmount} TL",
            StoreCostType.Diamond => $"{data.costAmount} Elmas",
            _ => "Al"
        };
    }
}
