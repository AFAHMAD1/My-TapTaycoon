using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kaydedilecek verilerin yapısını tutan sınıf.
/// </summary>
[Serializable]
public class SaveData
{
    public int saveVersion = 1;                             // Gelecekteki migrasyon için
    public double currentMoney;                             // Mevcut para
    public List<int> buildingLevels = new List<int>();      // Bina seviyeleri
    public int collectorLevel;                              // Tıklama/Karakter seviyesi
    public List<int> boostLevels = new List<int>();         // Boost seviyeleri
    public List<int> skillLevels = new List<int>();         // [BUG-06 FIX] Beceri seviyeleri
    public string lastSaveTime;                             // Offline kazanç için
}

/// <summary>
/// Oyunun kaydetme, yükleme ve offline kazanç hesaplama işlemlerini yönetir.
/// [ISaveable Pattern] Her manager kendi OnSave/OnLoad metodunu uygular.
/// SaveManager artık hiçbir concrete manager'ı doğrudan bilmek zorunda değil.
/// </summary>
public class SaveManager : MonoBehaviour, ISaveManager
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;

    // Sahnedeki tüm ISaveable bileşenlerini tutar
    private readonly List<ISaveable> _saveables = new List<ISaveable>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        // [BUG-03 FIX] LoadGame Awake'den Start'a taşındı.
        // Awake'de diğer manager Instance'ları henüz hazır değildir.
    }

    private void Start()
    {
        CollectSaveables();

        // [BUG-03 FIX] Start'ta çağrılır — tüm manager Awake() tamamlandıktan sonra.
        LoadGame();

        if (autoSave) InvokeRepeating(nameof(SaveGame), autoSaveInterval, autoSaveInterval);
    }

    /// <summary>
    /// Sahnedeki ISaveable uygulayan tüm MonoBehaviour bileşenlerini toplar.
    /// </summary>
    private void CollectSaveables()
    {
        _saveables.Clear();
        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in allBehaviours)
        {
            if (mb is ISaveable saveable)
                _saveables.Add(saveable);
        }
        Debug.Log($"[SaveManager] {_saveables.Count} ISaveable bileşeni bulundu.");
    }

    private void OnApplicationQuit() => SaveGame();
    private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }

    // [Android Safeguard] Bildirim çubuğu veya telefon araması gibi durumlarda kaydet.
    private void OnApplicationFocus(bool hasFocus) { if (!hasFocus) SaveGame(); }

    /// <summary>
    /// Mevcut oyun durumunu PlayerPrefs üzerine JSON olarak kaydeder.
    /// Her ISaveable kendi verisini SaveData'ya yazar.
    /// </summary>
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        SaveData data = new SaveData();

        foreach (var saveable in _saveables)
            saveable.OnSave(data);

        // [BUG-16 FIX] ISO 8601 "O" formatı — tüm Android cihazlarda locale-bağımsız çalışır.
        data.lastSaveTime = DateTime.UtcNow.ToString("O");

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GameSave", json);
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] Oyun Kaydedildi.");
    }

    /// <summary>
    /// Kayıtlı verileri yükler ve her ISaveable'a dağıtır.
    /// </summary>
    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("GameSave"))
        {
            Debug.Log("[SaveManager] Kayıtlı dosya bulunamadı.");
            return;
        }

        string json = PlayerPrefs.GetString("GameSave");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            Debug.LogWarning("[SaveManager] Kayıt dosyası bozuk veya okunamadı.");
            return;
        }

        foreach (var saveable in _saveables)
            saveable.OnLoad(data);

        CalculateOfflineEarnings(data.lastSaveTime);

        Debug.Log("[SaveManager] Veriler Yüklendi.");
    }

    /// <summary>
    /// Oyuncunun çevrimdışı kaldığı süreyi hesaplayıp parayı ekler.
    /// </summary>
    private void CalculateOfflineEarnings(string lastSaveTimeString)
    {
        if (string.IsNullOrEmpty(lastSaveTimeString)) return;

        // DateTimeStyles.RoundtripKind: ISO 8601 "O" formatını doğru ayrıştırır.
        if (DateTime.TryParse(lastSaveTimeString, null,
            System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastSaveTime))
        {
            double secondsOffline = (DateTime.UtcNow - lastSaveTime).TotalSeconds;

            if (secondsOffline > 60)
            {
                double incomePerSecond = PassiveIncomeManager.Instance != null
                    ? PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond() : 0d;

                double totalOfflineEarnings = incomePerSecond * secondsOffline;

                // [BUG-04 FIX] Null check eklendi — crash'i önler.
                if (totalOfflineEarnings > 0 && CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.AddMoney(totalOfflineEarnings, false);
                    Debug.Log($"[SaveManager] Offline Kazanç: {NumberFormatter.Format(totalOfflineEarnings)} ({secondsOffline:F0}s).");
                }
            }
        }
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("GameSave");
        Debug.Log("[SaveManager] İlerleme Sıfırlandı. Oyunu yeniden başlatın.");
    }
}
