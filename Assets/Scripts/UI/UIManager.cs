using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Tab Panels")]
    public GameObject[] allPanels;

    [Header("Settings")]
    public GameObject startPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

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

    public void OpenPanel(UnityEngine.Object targetObject)
    {
        OpenPanel(targetObject as GameObject);
    }

    public void TogglePanel(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        bool wasActive = targetPanel.activeSelf;
        HideAllPanels();
        
        // Eğer zaten açıksa kapat (Toggle), kapalıysa aç
        targetPanel.SetActive(!wasActive);
    }

    public void TogglePanel(UnityEngine.Object targetObject)
    {
        TogglePanel(targetObject as GameObject);
    }

    public void HideAllPanels()
    {
        if (allPanels == null) return;

        foreach (GameObject panel in allPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
