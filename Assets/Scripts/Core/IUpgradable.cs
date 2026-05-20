/// <summary>
/// Seviye atlayabilen ve satın alınabilen her nesnenin sözleşmesini (contract) tanımlar.
/// IncomeBuilding, BoostUpgrade ve SkillEntity bu arayüzü
/// UpgradableEntity base sınıfı aracılığıyla dolaylı olarak karşılar.
/// </summary>
public interface IUpgradable
{
    /// <summary>Nesnenin mevcut seviyesi. 0 ise henüz satın alınmamıştır.</summary>
    int CurrentLevel { get; }

    /// <summary>Nesne maksimum seviyeye ulaştıysa true döner.</summary>
    bool IsMaxLevel { get; }

    /// <summary>Mevcut seviyeden bir sonraki seviyenin maliyetini döner.</summary>
    double CurrentCost();

    /// <summary>Belirtilen sayıda seviye atlamanın toplam maliyetini döner.</summary>
    double GetTotalCostForUpgrades(int amount);

    /// <summary>
    /// Oyuncunun mevcut parasıyla kaç seviye atlayabileceğini hesaplar.
    /// </summary>
    int GetMaxAffordableUpgradeCount(double availableMoney);

    /// <summary>Tek bir seviye atlama için para yeterliyse true döner.</summary>
    bool CanAffordUpgrade();

    /// <summary>
    /// Belirtilen sayıda seviye atlama için para yeterliyse true döner.
    /// amount == int.MaxValue ise "MAX" modunda en az 1 seviye karşılanabilir mi diye bakar.
    /// </summary>
    bool CanAffordUpgradeAmount(int amount);

    /// <summary>
    /// Bir seviye atlamayı dener. Para yeterliyse harcar, seviyeyi yükseltir ve true döner.
    /// Para yetersizse veya max seviyedeyse false döner.
    /// </summary>
    bool TryUpgrade();
}
