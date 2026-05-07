/// <summary>
/// Kaydetme/yukleme sisteminin sozlesmesini (contract) tanimlar.
/// Bu arayuzu kullanan kodlar, SaveManager'in somut implementasyonuna
/// bagimli kalmaz — farkli bir kaydetme stratejisiyle (Cloud Save vb.)
/// kolayca degistirilebilir.
/// </summary>
public interface ISaveManager
{
    /// <summary>
    /// Mevcut oyun durumunu kalici olarak kaydeder.
    /// Para, bina seviyeleri, gelistirmeler ve zaman damgasi dahildir.
    /// </summary>
    void SaveGame();

    /// <summary>
    /// Kayitli verileri yukler ve ilgili manager siniflarına dagitir.
    /// Kayit bulunamazsa sessizce devam eder.
    /// </summary>
    void LoadGame();

    /// <summary>
    /// Tum kayitli ilerlemeyi siler.
    /// Oyunu yeniden baslatmadan once cagrilmalidir.
    /// </summary>
    void ResetProgress();
}
