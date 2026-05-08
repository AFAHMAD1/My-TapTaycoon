using System.Collections.Generic;

/// <summary>
/// Pasif gelir sisteminin sozlesmesini (contract) tanimlar.
/// Binalarin uretim sureci, seviye atlama ve gelir toplama islemlerini kapsar.
/// </summary>
public interface IPassiveIncomeManager
{
    /// <summary>
    /// Tum satin alinmis binalarin saniyedeki toplam pasif kazancini dondurur.
    /// SaveManager'in offline kazanc hesaplamasi bu degeri kullanir.
    /// </summary>
    double GetTotalPassiveIncomePerSecond();

    /// <summary>
    /// Verilen indeksteki binayi 1 seviye yukselmeye calisir.
    /// </summary>
    void BuyUpgrade(int index);

    /// <summary>
    /// Verilen indeksteki binayi belirtilen miktar kadar yukseltmeye calisir.
    /// amount == int.MaxValue ise MAX mod devreye girer.
    /// </summary>
    void BuyUpgrade(int index, int amount);

    /// <summary>
    /// Manuel toplama modundaki bir binadan geliri toplar.
    /// Basarili olursa true, bina hazir degilse false dondurur.
    /// </summary>
    bool CollectIncome(int index);

    /// <summary>
    /// Tum binalara salt-okunur erisim saglar.
    /// UI ve SaveManager bina verilerini bu property uzerinden okur.
    /// </summary>
    IReadOnlyList<IncomeBuilding> Buildings { get; }
}
