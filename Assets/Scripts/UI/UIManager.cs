using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Tab Panels")]
    public GameObject[] allPanels;

    [Header("Settings")]
    public GameObject startPanel;

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    private void Start()
    {
        // En başta tüm panelleri gizle
        HideAllPanels();

        // Eğer bir başlangıç paneli seçildiyse onu aç
        if (startPanel != null)
        {
            OpenPanel(startPanel);
        }
    }

    /// <summary>
    /// Bu fonksiyonu butonların OnClick() eventinde çağıracağız.
    /// Parametre olarak açılmasını istediğimiz Panel (GameObject) gönderilmeli.
    /// </summary>
    public void OpenPanel(GameObject targetPanel)
    {
        // Önce tüm panelleri kapat
        HideAllPanels();

        // Sonra hedeflenen paneli aç
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void TogglePanel(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        bool wasActive = targetPanel.activeSelf;
        HideAllPanels();
        
        // Eğer zaten açıksa kapat (Toggle), kapalıysa aç
        targetPanel.SetActive(!wasActive);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void HideAllPanels()
    {
        if (allPanels == null) return;

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
