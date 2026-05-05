using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Oyunun temel para yönetim sistemidir. 
/// Paranın artması, harcanması ve UI üzerindeki güncel durumunu yönetir.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    // Singleton Pattern: Diğer tüm scriptlerden 'CurrencyManager.Instance' ile ulaşılabilir.
    public static CurrencyManager Instance { get; private set; }

    [Header("Para Verileri")]
    public double currentMoney = 0; // Mevcut para miktarı (double kullanarak çok yüksek sayılara destek veriyoruz)
    public TextMeshProUGUI moneyText; // Ekranda parayı gösteren yazı bileşeni

    // Son 1 saniye içindeki kazancı hesaplamak için kullanılan veri yapısı
    private readonly Queue<IncomeSample> recentActiveIncomeSamples = new Queue<IncomeSample>();
    private const float ActiveIncomeWindowSeconds = 1f;

    private struct IncomeSample
    {
        public double amount;
        public float time;
    }

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Oyuna para ekler. Varsayılan olarak aktif kazanç (DPS) hesabına dahil edilir.
    /// </summary>
    public void AddMoney(double amount)
    {
        AddMoney(amount, true);
    }

    /// <summary>
    /// Oyuna para ekler. 
    /// countTowardActiveProfit: true ise bu kazanç 'Saniyedeki Kazanç' (DPS) hesabına dahil edilir.
    /// </summary>
    public void AddMoney(double amount, bool countTowardActiveProfit)
    {
        if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
        {
            return;
        }

        currentMoney += amount;

        // Kazanç istatistiklerini güncelle (UI'daki saniyelik kazanç göstergesi için)
        if (countTowardActiveProfit && amount > 0d)
        {
            recentActiveIncomeSamples.Enqueue(new IncomeSample
            {
                amount = amount,
                time = Time.unscaledTime
            });
        }

        UpdateUI();
    }

    /// <summary>
    /// Para harcama fonksiyonu. Para yetiyorsa true döner ve parayı düşer.
    /// </summary>
    public bool SpendMoney(double amount)
    {
        if (amount <= 0d || double.IsNaN(amount) || double.IsInfinity(amount))
        {
            return false;
        }

        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }

        return false; // Bakiye yetersiz
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    public void SetMoney(double amount)
    {
        currentMoney = System.Math.Max(0d, amount);
        recentActiveIncomeSamples.Clear();
        UpdateUI();
    }

    /// <summary>
    /// Son 1 saniye içindeki ortalama kazancı (Active Profit) hesaplar.
    /// </summary>
    public double GetRecentActiveIncomePerSecond()
    {
        TrimExpiredIncomeSamples(); // Eski örnekleri temizle

        double totalIncome = 0d;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (IncomeSample sample in recentActiveIncomeSamples)
        {
            totalIncome += sample.amount;
        }

        return totalIncome / ActiveIncomeWindowSeconds;
    }

    // Süresi dolan (1 saniyeden eski) kazanç verilerini listeden çıkarır.
    private void TrimExpiredIncomeSamples()
    {
        float cutoffTime = Time.unscaledTime - ActiveIncomeWindowSeconds;

        // Bu dongu kosul dogru kaldigi surece calisir; kosul bozulunca durur.
        while (recentActiveIncomeSamples.Count > 0 && recentActiveIncomeSamples.Peek().time < cutoffTime)
        {
            recentActiveIncomeSamples.Dequeue();
        }
    }

    /// <summary>
    /// Para miktarını NumberFormatter kullanarak kısaltılmış (1.5K, 2M gibi) şekilde UI'a yazar.
    /// </summary>
    private void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = NumberFormatter.Format(currentMoney);
        }
    }
}
