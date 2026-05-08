/// <summary>
/// Para yönetim sisteminin sözleşmesini (contract) tanımlar.
/// Bu arayüzü kullanan kodlar, CurrencyManager'ın somut implementasyonuna
/// bağımlı kalmaz — test veya farklı bir implementasyonla kolayca değiştirilebilir.
/// </summary>
public interface ICurrencyManager
{
    /// <summary>Oyuncunun mevcut para miktarı.</summary>
    double CurrentMoney { get; }

    /// <summary>
    /// Oyuna belirtilen miktarda para ekler.
    /// Varsayılan olarak aktif kazanç (DPS) istatistiğine dahil edilir.
    /// </summary>
    void AddMoney(double amount);

    /// <summary>
    /// Oyuna belirtilen miktarda para ekler.
    /// countTowardActiveProfit: true ise bu kazanç saniyelik kazanç (DPS) hesabına dahil edilir.
    /// </summary>
    void AddMoney(double amount, bool countTowardActiveProfit);

    /// <summary>
    /// Belirtilen miktarda para harcamayı dener.
    /// Para yeterliyse parayı düşer ve true döner; yetersizse false döner.
    /// </summary>
    bool SpendMoney(double amount);

    /// <summary>
    /// Son 1 saniye içindeki ortalama aktif kazancı (tıklama vb.) döner.
    /// </summary>
    double GetRecentActiveIncomePerSecond();
}
