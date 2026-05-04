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

    private void Awake()
    {
        // Butonlara tıklama dinleyicilerini ekle
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        
        if (musicToggle != null) musicToggle.onValueChanged.AddListener(SetMusic);
        if (soundToggle != null) soundToggle.onValueChanged.AddListener(SetSound);
    }

    private void OnEnable()
    {
        // Panel açıldığında güncel versiyonu ve ayarları yükle
        if (versionText != null) versionText.text = "v" + Application.version;
        
        if (musicToggle != null) musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        if (soundToggle != null) soundToggle.isOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void SetMusic(bool isOn)
    {
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        // İleride buraya ses yöneticisi (AudioManager) bağlanabilir.
    }

    private void SetSound(bool isOn)
    {
        PlayerPrefs.SetInt("SoundEnabled", isOn ? 1 : 0);
    }

    /// <summary>
    /// Oyunu sıfırlama işlemini tetikler.
    /// </summary>
    private void OnResetClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetProgress();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
