using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Oyun ayarlarını (ses, müzik, sıfırlama) yöneten UI panelidir.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Button closeButton; // Kapatma butonu
    public Button resetButton; // Oyunu sıfırlama butonu
    public Toggle musicToggle; // Müzik aç/kapat
    public Toggle soundToggle; // Ses aç/kapat
    public TextMeshProUGUI versionText; // Oyun versiyonu yazısı

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        // Butonlara tıklama dinleyicilerini ekle
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        
        if (musicToggle != null) musicToggle.onValueChanged.AddListener(SetMusic);
        if (soundToggle != null) soundToggle.onValueChanged.AddListener(SetSound);
    }

    // Obje aktif olunca calisir; event dinleyicileri veya gecici durumlar burada hazirlanir.
    private void OnEnable()
    {
        // Panel açıldığında güncel versiyonu ve ayarları yükle
        if (versionText != null) versionText.text = "v" + Application.version;
        
        if (musicToggle != null) musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        if (soundToggle != null) soundToggle.isOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Close()
    {
        // Bu satir: 'gameObject' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
        gameObject.SetActive(false);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void SetMusic(bool isOn)
    {
        // Bu satir: 'PlayerPrefs' objesi uzerindeki 'SetInt' metodunu cagirir; PlayerPrefs icine tam sayi degeri kaydeder.
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        // İleride buraya ses yöneticisi (AudioManager) bağlanabilir.
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void SetSound(bool isOn)
    {
        // Bu satir: 'PlayerPrefs' objesi uzerindeki 'SetInt' metodunu cagirir; PlayerPrefs icine tam sayi degeri kaydeder.
        PlayerPrefs.SetInt("SoundEnabled", isOn ? 1 : 0);
    }

    /// <summary>
    /// Oyunu sıfırlama işlemini tetikler.
    /// </summary>
    private void OnResetClicked()
    {
        if (SaveManager.Instance != null)
        {
            // Bu satir: 'Instance' objesi uzerindeki 'ResetProgress' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
            SaveManager.Instance.ResetProgress();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // Bu satir: 'Application' objesi uzerindeki 'Quit' metodunu cagirir; oyunu kapatma istegi gonderir; Editor'da genelde etkisi sinirlidir.
            Application.Quit();
#endif
        }
    }
}
