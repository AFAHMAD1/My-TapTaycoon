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
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        HideAllPanels();

        if (startPanel != null)
        {
            OpenPanel(startPanel);
        }
    }

    public void OpenPanel(GameObject targetPanel)
    {
        HideAllPanels();

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
