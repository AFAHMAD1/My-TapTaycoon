/// <summary>
/// Kendini kaydetmek ve yuklemek isteyen her sinifin sozlesmesini tanimlar.
/// SaveManager bu arayuzu uygulayan tum bilesenlerle otomatik calisir.
/// Yeni bir sistem eklendiginde sadece bu arayuzu uygulamak yeterlidir —
/// SaveManager.cs degistirmeye gerek kalmaz.
/// </summary>
public interface ISaveable
{
    /// <summary>SaveManager kaydederken cagirir. Sinif kendi verisini 'data'ya yazar.</summary>
    void OnSave(SaveData data);

    /// <summary>SaveManager yuklerken cagirir. Sinif kendi verisini 'data'dan okur.</summary>
    void OnLoad(SaveData data);
}
