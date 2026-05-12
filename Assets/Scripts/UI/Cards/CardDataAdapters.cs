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
            // Bu satir: 'building' uzerindeki 'GetMaxAffordableUpgradeCount' metodunu cagirir ve sonucu 'maxAffordable' degiskenine koyar; eldeki parayla en fazla kac upgrade alinabilecegini hesaplar.
            int maxAffordable = building.GetMaxAffordableUpgradeCount(money);
            return maxAffordable > 0 ? building.GetTotalCostForUpgrades(maxAffordable) : building.CurrentCost();
        }
        // Bu satir: 'building' objesi uzerindeki 'GetTotalCostForUpgrades' metodunu cagirir; birden fazla level satin almanin toplam maliyetini hesaplar.
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
        // Bu satir: 'building' objesi uzerindeki 'CanAffordUpgradeAmount' metodunu cagirir; istenen sayida upgrade icin para yetip yetmedigini kontrol eder.
        return building.CanAffordUpgradeAmount(amount);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Purchase(int amount)
    {
        if (PassiveIncomeManager.Instance != null)
        {
            // Bu satir: 'Instance' objesi uzerindeki 'BuyUpgrade' metodunu cagirir; belirli index'teki bina/upgrade icin satin alma islemini tetikler.
            PassiveIncomeManager.Instance.BuyUpgrade(index, amount);
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool HasProgressBar() => true;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public float GetProgressNormalized()
    {
        if (building.currentLevel <= 0) return 0f;
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'duration' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        float duration = Mathf.Max(0.2f, building.CurrentDuration());
        return Mathf.Clamp(building.timer, 0f, duration) / duration;
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public string GetProgressText()
    {
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'duration' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        float duration = Mathf.Max(0.2f, building.CurrentDuration());
        if (building.currentLevel <= 0) return FormatRemainingTime(duration);
        
        if (building.data != null && building.data.requireManualCollection && building.isReadyToCollect)
            return "Toplamaya Hazir!";
            
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'remainingTime' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        float remainingTime = Mathf.Max(0f, duration - building.timer);
        return FormatRemainingTime(remainingTime);
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public void OnProgressClick()
    {
        if (PassiveIncomeManager.Instance != null)
        {
            // Bu satir: 'Instance' objesi uzerindeki 'CollectIncome' metodunu cagirir; hazir olan bina gelirini toplamayi dener.
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
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'roundedSeconds' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        int roundedSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
        // Bu satir: 'TimeSpan' uzerindeki 'FromSeconds' metodunu cagirir ve sonucu 'remaining' degiskenine koyar; saniye degerini saat-dakika-saniye olarak kullanilabilecek TimeSpan yapisina cevirir.
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
            // Bu satir: 'collector' uzerindeki 'GetMaxAffordableUpgradeCount' metodunu cagirir ve sonucu 'maxAffordable' degiskenine koyar; eldeki parayla en fazla kac upgrade alinabilecegini hesaplar.
            int maxAffordable = collector.GetMaxAffordableUpgradeCount(money);
            return maxAffordable > 0 ? collector.GetTotalCostForUpgrades(maxAffordable) : collector.CurrentCost();
        }
        // Bu satir: 'collector' objesi uzerindeki 'GetTotalCostForUpgrades' metodunu cagirir; birden fazla level satin almanin toplam maliyetini hesaplar.
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
        // Bu satir: 'PurchaseService' objesi uzerindeki 'TryPurchase' metodunu cagirir; para yetiyorsa satin alma/seviye atlama islemini dener.
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
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
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
            // Bu satir: 'boost' uzerindeki 'GetMaxAffordableUpgradeCount' metodunu cagirir ve sonucu 'maxAffordable' degiskenine koyar; eldeki parayla en fazla kac upgrade alinabilecegini hesaplar.
            int maxAffordable = boost.GetMaxAffordableUpgradeCount(money);
            return maxAffordable > 0 ? boost.GetTotalCostForUpgrades(maxAffordable) : boost.CurrentCost();
        }
        // Bu satir: 'boost' objesi uzerindeki 'GetTotalCostForUpgrades' metodunu cagirir; birden fazla level satin almanin toplam maliyetini hesaplar.
        return boost.GetTotalCostForUpgrades(amount);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public double GetIncomePerCycle() => 0d;

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    public bool CanAfford(int amount) => boost.CanAffordUpgradeAmount(amount);

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Purchase(int amount)
    {
        // Bu satir: 'PurchaseService' objesi uzerindeki 'TryPurchase' metodunu cagirir; para yetiyorsa satin alma/seviye atlama islemini dener.
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

public class PlayerProfitCardAdapter : ICardDataProvider
{
    private readonly PlayerProfitUpgrade playerUpgrade;
    private readonly CardDisplayConfig config;

    public PlayerProfitCardAdapter(PlayerProfitUpgrade playerUpgrade)
    {
        this.playerUpgrade = playerUpgrade;
        config = new CardDisplayConfig
        {
            showIcon = true,
            showProgressBar = false,
            showDescription = true,
            showIncome = false,
            showLevel = true,
            showSecondaryButton = false
        };
    }

    public string DisplayName => playerUpgrade != null ? playerUpgrade.playerName : "Player";
    public int CurrentLevel => playerUpgrade != null ? playerUpgrade.currentLevel : 0;

    public string Description
    {
        get
        {
            if (playerUpgrade == null)
            {
                return "";
            }

            string current = playerUpgrade.CurrentMultiplier().ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            string next = playerUpgrade.PreviewNextMultiplier().ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            return playerUpgrade.currentLevel <= 0
                ? $"Building profit x{next} after bought"
                : $"Building profit x{current}";
        }
    }

    public Sprite Icon => playerUpgrade != null ? playerUpgrade.icon : null;
    public Color CardColor => playerUpgrade != null ? playerUpgrade.cardBackgroundColor : Color.white;
    public Color ButtonColor => playerUpgrade != null ? playerUpgrade.buttonColor : Color.white;
    public Color IconTintColor => playerUpgrade != null ? playerUpgrade.iconTintColor : Color.white;
    public Color IncomeColor => playerUpgrade != null ? playerUpgrade.incomeColor : Color.white;
    public CardDisplayConfig DisplayConfig => config;

    public double GetCost(int amount)
    {
        return playerUpgrade != null ? playerUpgrade.GetTotalCost(amount) : 0d;
    }

    public double GetIncomePerCycle()
    {
        return playerUpgrade != null ? playerUpgrade.CurrentMultiplier() : 1d;
    }

    public bool CanAfford(int amount)
    {
        return playerUpgrade != null && playerUpgrade.CanAfford(amount);
    }

    public void Purchase(int amount)
    {
        playerUpgrade?.TryPurchase(amount);
    }

    public bool HasProgressBar() => false;
    public float GetProgressNormalized() => 0f;
    public string GetProgressText() => "";
    public void OnProgressClick() { }

    public string GetBuyButtonText(int amount)
    {
        if (playerUpgrade == null)
        {
            return "Buy";
        }

        if (playerUpgrade.IsMaxLevel)
        {
            return "Bought";
        }

        if (amount >= int.MaxValue)
        {
            return "Buy MAX";
        }

        return amount > 1 ? $"Buy x{amount}" : "Buy";
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
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log($"[{data.itemName}] satin alindi/tiklandi!");
        
        // Odul mantigi
        switch (data.rewardType)
        {
            case StoreRewardType.TimeSkipMoney:
                if (PassiveIncomeManager.Instance != null && CurrencyManager.Instance != null)
                {
                    // Bu satir: 'Instance' uzerindeki 'GetTotalPassiveIncomePerSecond' metodunu cagirir ve sonucu 'totalIncomePerSec' degiskenine koyar; tum binalarin saniyelik toplam pasif gelirini hesaplar.
                    double totalIncomePerSec = PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond();
                    double skipReward = totalIncomePerSec * (data.rewardAmount * 3600); // Saat -> Saniye
                    // Bu satir: 'Instance' objesi uzerindeki 'AddMoney' metodunu cagirir; oyuncunun bakiyesine para ekler.
                    CurrencyManager.Instance.AddMoney(skipReward, true);
                    // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
                    Debug.Log($"{data.rewardAmount} saatlik kazanc eklendi: {skipReward}");
                }
                break;
                
            case StoreRewardType.SocialMediaReward:
                // Bu satir: 'Application' objesi uzerindeki 'OpenURL' metodunu cagirir; tarayici veya sistem uzerinden verilen linki acar.
                Application.OpenURL("https://instagram.com/oyunumuz");
                // TODO: Elmas ekle
                break;
                
            case StoreRewardType.CooldownReset:
                // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
                Debug.Log("Beceriler sifirlandi!");
                break;
                
            case StoreRewardType.DiamondPack:
                // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
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
