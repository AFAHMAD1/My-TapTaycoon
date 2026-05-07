using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Oyunun temel para yönetim sistemidir. 
/// Paranın artması, harcanması ve UI üzerindeki güncel durumunu yönetir.
/// </summary>
public class CurrencyManager : MonoBehaviour, ICurrencyManager, ISaveable
{
    // Singleton Pattern: Diğer tüm scriptlerden 'CurrencyManager.Instance' ile ulaşılabilir.
    public static CurrencyManager Instance;

    [Header("Para Verileri")]
    public double currentMoney = 0; // Mevcut para miktarı (double kullanarak çok yüksek sayılara destek veriyoruz)
    public TextMeshProUGUI moneyText; // Ekranda parayı gösteren yazı bileşeni

    /// <summary>
    /// ICurrencyManager arayüzü gerekliliği.
    /// Mevcut 'currentMoney' alanını dışarıya property olarak sunar.
    /// Eski kodlar currentMoney alanını kullanmaya devam edebilir.
    /// </summary>
    public double CurrentMoney => currentMoney;

    // Son 1 saniye içindeki kazancı hesaplamak için kullanılan veri yapısı
    private readonly Queue<IncomeSample> recentActiveIncomeSamples = new Queue<IncomeSample>();
    private const float ActiveIncomeWindowSeconds = 1f;

    private struct IncomeSample
    {
        public double amount;
        public float time;
    }

    private void Awake()
    {
        // [BUG-01 FIX] Added proper singleton guard to prevent duplicate instances
        // from overwriting the existing one (e.g. on scene reload).
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }

        return false; // Bakiye yetersiz
    }

    /// <summary>
    /// Son 1 saniye içindeki ortalama kazancı (Active Profit) hesaplar.
    /// </summary>
    public double GetRecentActiveIncomePerSecond()
    {
        TrimExpiredIncomeSamples(); // Eski örnekleri temizle

        double totalIncome = 0d;
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

    // --- ISaveable ---

    public void OnSave(SaveData data)
    {
        data.currentMoney = currentMoney;
    }

    public void OnLoad(SaveData data)
    {
        currentMoney = data.currentMoney;
        UpdateUI();
    }
}
