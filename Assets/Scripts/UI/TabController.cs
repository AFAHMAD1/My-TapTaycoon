using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabController : MonoBehaviour
{
    [System.Serializable]
    public class TabContent
    {
        public Button tabButton;
        public GameObject contentPanel;

        [Tooltip("Optional button label. Leave empty to preserve the existing button text.")]
        public string buttonLabel;
    }

    [Header("Alt Sekmeler")]
    public TabContent[] tabs;

    [Header("Acilis Ayarlari")]
    [Tooltip("Which tab should be opened by default? 0 = first tab.")]
    public int defaultTabIndex = 0;

    [Tooltip("Open the default tab when this controller starts/enables.")]
    public bool openDefaultTabOnStart = true;

    [Header("Kapatma Butonlari")]
    [Tooltip("Adds a red X close button to each tab content panel. Keep disabled when panels already include ClosingButton prefabs.")]
    public bool addCloseButtonsToTabs = false;

    private void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            ApplyButtonLabel(tabs[i]);

            int index = i;
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => OpenTab(index));
            }

            if (tabs[i].contentPanel != null)
            {
                WireCloseButton(tabs[i].contentPanel);
            }
        }

        CloseAllTabs();
        if (openDefaultTabOnStart)
        {
            OpenTab(defaultTabIndex);
        }
    }

    private void OnEnable()
    {
        if (openDefaultTabOnStart)
        {
            OpenTab(defaultTabIndex);
        }
    }

    private void WireCloseButton(GameObject panel)
    {
        Button closeButton = UIHelper.FindButton(panel.transform, "ClosingButton", "CloseButton", "ExitButton", "KapatButonu");

        if (closeButton == null && addCloseButtonsToTabs)
        {
            GameObject buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(panel.transform, false);
            buttonObject.transform.SetAsLastSibling();

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-15f, -15f);
            rect.sizeDelta = new Vector2(50f, 50f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = "X";
            text.fontSize = 30f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            closeButton = buttonObject.GetComponent<Button>();
        }

        if (closeButton == null)
        {
            return;
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => panel.SetActive(false));

        if (closeButton.gameObject.GetComponent<UIBounce>() == null)
        {
            closeButton.gameObject.AddComponent<UIBounce>();
        }
    }

    private void ApplyButtonLabel(TabContent tab)
    {
        if (tab.tabButton == null || string.IsNullOrWhiteSpace(tab.buttonLabel)) return;

        TextMeshProUGUI label = tab.tabButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = tab.buttonLabel;
        }
    }

    public void OpenTab0() => OpenTab(0);
    public void OpenTab1() => OpenTab(1);
    public void OpenTab2() => OpenTab(2);

    public void CloseAllTabs()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].contentPanel != null)
            {
                tabs[i].contentPanel.SetActive(false);
            }

            if (tabs[i].tabButton != null)
            {
                ColorBlock colors = tabs[i].tabButton.colors;
                colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                tabs[i].tabButton.colors = colors;
            }
        }
    }

    public void OpenTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = i == index;

            if (tabs[i].contentPanel != null)
            {
                tabs[i].contentPanel.SetActive(isActive);
            }

            if (tabs[i].tabButton != null)
            {
                ColorBlock colors = tabs[i].tabButton.colors;
                colors.normalColor = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
                tabs[i].tabButton.colors = colors;
            }
        }
    }
}
