using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kaydedilecek verilerin yapısını tutan sınıf.
/// </summary>
[Serializable]
public class SaveData
{
    public double currentMoney; // Mevcut para
    public List<int> buildingLevels = new List<int>(); // Bina seviyeleri listesi
    public int collectorLevel; // Tıklama/Karakter seviyesi
    public List<int> boostLevels = new List<int>(); // Özel geliştirme seviyeleri
    public string lastSaveTime; // En son ne zaman kaydedildi? (Offline kazanç için)
}

/// <summary>
/// Oyunun kaydetme, yükleme ve offline kazanç hesaplama işlemlerini yönetir.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private bool autoSave = true; // Otomatik kaydetme açık mı?
    [SerializeField] private float autoSaveInterval = 30f; // Kaç saniyede bir kaydedilsin?

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

    }

    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    private void Start()
    {
        LoadGame();

        // Belirlenen aralıklarla otomatik kaydetmeyi başlat
        if (autoSave) InvokeRepeating(nameof(SaveGame), autoSaveInterval, autoSaveInterval);
    }

    // Oyuncu oyundan çıkarken kaydet
    private void OnApplicationQuit() => SaveGame();

    // Oyuncu oyunu alta aldığında (mobilde önemli) kaydet
    private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }

    /// <summary>
    /// Mevcut oyun durumunu PlayerPrefs üzerine JSON olarak kaydeder.
    /// </summary>
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Parayı kaydet
        if (CurrencyManager.Instance != null)
            data.currentMoney = CurrencyManager.Instance.currentMoney;

        // Binaları kaydet
        if (PassiveIncomeManager.Instance != null)
        {
            // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
            foreach (var building in PassiveIncomeManager.Instance.buildings)
                // Bu satir: 'buildingLevels' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
                data.buildingLevels.Add(building.currentLevel);
        }

        // Geliştirmeleri kaydet
        if (UpgradeManager.Instance != null)
        {
            if (UpgradeManager.Instance.collectorUpgrade != null)
                data.collectorLevel = UpgradeManager.Instance.collectorUpgrade.currentLevel;

            // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
            foreach (var boost in UpgradeManager.Instance.boostUpgrades)
                // Bu satir: 'boostLevels' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
                data.boostLevels.Add(boost.currentLevel);
        }

        // Zaman damgasını kaydet
        data.lastSaveTime = DateTime.UtcNow.ToString();

        // JSON'a dönüştür ve sakla
        // Bu satir: 'JsonUtility' uzerindeki 'ToJson' metodunu cagirir ve sonucu 'json' degiskenine koyar; SaveData nesnesini JSON metnine cevirir; kayit icin string hale getirir.
        string json = JsonUtility.ToJson(data);
        // Bu satir: 'PlayerPrefs' objesi uzerindeki 'SetString' metodunu cagirir; PlayerPrefs icine string veri kaydeder.
        PlayerPrefs.SetString("GameSave", json);
        // Bu satir: 'PlayerPrefs' objesi uzerindeki 'Save' metodunu cagirir; PlayerPrefs degisikliklerini diske kaydeder.
        PlayerPrefs.Save();
        
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log("[SaveManager] Oyun Kaydedildi.");
    }

    /// <summary>
    /// Kayıtlı verileri yükler ve oyun dünyasına uygular.
    /// </summary>
    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("GameSave"))
        {
            // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
            Debug.Log("[SaveManager] Kayıtlı dosya bulunamadı.");
            return;
        }

        // Bu satir: 'PlayerPrefs' uzerindeki 'GetString' metodunu cagirir ve sonucu 'json' degiskenine koyar; PlayerPrefs icinden kayitli string veriyi okur.
        string json = PlayerPrefs.GetString("GameSave");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Verileri ilgili Manager sınıflarına dağıt
        if (CurrencyManager.Instance != null)
            // Bu satir: 'Instance' objesi uzerindeki 'SetMoney' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
            CurrencyManager.Instance.SetMoney(data.currentMoney);

        if (PassiveIncomeManager.Instance != null && data.buildingLevels.Count > 0)
        {
            // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
            for (int i = 0; i < PassiveIncomeManager.Instance.buildings.Count; i++)
            {
                if (i < data.buildingLevels.Count)
                    PassiveIncomeManager.Instance.buildings[i].currentLevel = data.buildingLevels[i];
            }
        }

        if (UpgradeManager.Instance != null)
        {
            if (UpgradeManager.Instance.collectorUpgrade != null)
                UpgradeManager.Instance.collectorUpgrade.currentLevel = data.collectorLevel;

            if (data.boostLevels != null)
            {
                // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
                for (int i = 0; i < UpgradeManager.Instance.boostUpgrades.Count; i++)
                {
                    if (i < data.boostLevels.Count)
                        UpgradeManager.Instance.boostUpgrades[i].currentLevel = data.boostLevels[i];
                }
            }
        }

        CalculateOfflineEarnings(data.lastSaveTime);

        if (PassiveIncomeManager.Instance != null)
        {
            // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
            foreach (var building in PassiveIncomeManager.Instance.buildings)
            {
                // Bu satir: 'building' objesi uzerindeki 'InitializeVisualState' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
                building.InitializeVisualState();
            }
        }

        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log("[SaveManager] Veriler Yüklendi.");
    }

    /// <summary>
    /// Oyuncunun çevrimdışı kaldığı süreyi hesaplayıp parayı ekler.
    /// </summary>
    private void CalculateOfflineEarnings(string lastSaveTimeString)
    {
        if (string.IsNullOrEmpty(lastSaveTimeString)) return;

        if (DateTime.TryParse(lastSaveTimeString, out DateTime lastSaveTime))
        {
            TimeSpan timeOffline = DateTime.UtcNow - lastSaveTime;
            double secondsOffline = timeOffline.TotalSeconds;

            // En az 1 dakika offline kalmışsa kazanç ver
            if (secondsOffline > 60)
            {
                double incomePerSecond = PassiveIncomeManager.Instance != null ? PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond() : 0d;
                double totalOfflineEarnings = incomePerSecond * secondsOffline;

                if (totalOfflineEarnings > 0)
                {
                    if (CurrencyManager.Instance != null)
                    {
                        // Bu satir: 'Instance' objesi uzerindeki 'AddMoney' metodunu cagirir; oyuncunun bakiyesine para ekler.
                        CurrencyManager.Instance.AddMoney(totalOfflineEarnings, false);
                        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
                        Debug.Log($"[SaveManager] Offline Kazanc: ${NumberFormatter.Format(totalOfflineEarnings)} ({secondsOffline:F0} saniye icin).");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tüm ilerlemeyi sıfırlar.
    /// </summary>
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        // Bu satir: 'PlayerPrefs' objesi uzerindeki 'DeleteKey' metodunu cagirir; PlayerPrefs icindeki belirtilen kayit anahtarini siler.
        PlayerPrefs.DeleteKey("GameSave");
        // Bu satir: 'Debug' objesi uzerindeki 'Log' metodunu cagirir; Unity Console'a bilgi mesaji yazar.
        Debug.Log("[SaveManager] İlerleme Sıfırlandı. Oyunu yeniden başlatın.");
    }
}
