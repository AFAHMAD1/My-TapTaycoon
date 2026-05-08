using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel içerisindeki alt-sekmelerin (Hesap, İstatistikler, Beceriler vb.)
/// yönetimini modüler şekilde sağlar.
/// </summary>
public class TabController : MonoBehaviour
{
    [System.Serializable]
    public class TabContent
    {
        public Button tabButton;        // Tıklanacak buton (Örn: Hesap Butonu)
        public GameObject contentPanel; // Açılacak panel (Örn: Hesap Paneli)

        [Tooltip("Bu sekmenin buton üzerinde görünecek etiketi. Boş bırakılırsa buton yazısı değiştirilmez.")]
        public string buttonLabel;      // Buton üzerindeki yazı (Account, Statistics, Skills vb.)
    }

    [Header("Alt Sekmeler")]
    public TabContent[] tabs;

    [Header("Açılış Ayarları")]
    [Tooltip("Hangi sekme varsayılan olarak açılsın? (0 = ilk sekme)")]
    public int defaultTabIndex = 0;

    [Tooltip("Panel ilk açıldığında varsayılan sekmeyi otomatik aç. Kapatmak için false yapın.")]
    public bool openDefaultTabOnStart = true;

    [Header("Kapatma Butonlari")]
    [Tooltip("Her alt panele SkillsPanel'deki gibi sag ustte kirmizi bir X kapatma butonu ekler.")]
    public bool addCloseButtonsToTabs = true;

    private void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            // Buton etiketini Inspector'dan gelen değerle güncelle
            ApplyButtonLabel(tabs[i]);

            int index = i; // Closure sorunu yaşamamak için yerel değişken
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => OpenTab(index));
            }

            if (addCloseButtonsToTabs && tabs[i].contentPanel != null)
            {
                EnsureCloseButton(tabs[i].contentPanel);
            }
        }

        // Tüm panelleri başlangıçta kapat
        CloseAllTabs();

        // Varsayılan sekmeyi aç (istenirse)
        if (openDefaultTabOnStart)
        {
            OpenTab(defaultTabIndex);
        }
    }

    private void OnEnable()
    {
        // Panel tekrar görünür olduğunda varsayılan sekmeyi aç
        if (openDefaultTabOnStart)
        {
            OpenTab(defaultTabIndex);
        }
    }

    private void EnsureCloseButton(GameObject panel)
    {
        Button closeButton = UIHelper.FindButton(panel.transform, "CloseButton", "KapatButonu");

        if (closeButton == null)
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

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => panel.SetActive(false));

        if (closeButton.gameObject.GetComponent<UIBounce>() == null)
        {
            closeButton.gameObject.AddComponent<UIBounce>();
        }
    }

    /// <summary>
    /// Buton üzerindeki TMP metnini Inspector'dan girilen 'buttonLabel' değeriyle günceller.
    /// Boş bırakılırsa mevcut yazı korunur.
    /// </summary>
    private void ApplyButtonLabel(TabContent tab)
    {
        if (tab.tabButton == null || string.IsNullOrWhiteSpace(tab.buttonLabel)) return;

        TextMeshProUGUI label = tab.tabButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = tab.buttonLabel;
        }
    }

    // -----------------------------------------------------------------
    // Named shortcuts — assign these directly in the Inspector onClick
    // without needing to pass an index argument.
    // -----------------------------------------------------------------

    /// <summary>Tab 0'ı (Account) açar. Inspector onClick için kullanılabilir.</summary>
    public void OpenTab0() => OpenTab(0);

    /// <summary>Tab 1'i (Statistics) açar. Inspector onClick için kullanılabilir.</summary>
    public void OpenTab1() => OpenTab(1);

    /// <summary>Tab 2'yi (Skills) açar. Inspector onClick için kullanılabilir.</summary>
    public void OpenTab2() => OpenTab(2);

    // -----------------------------------------------------------------

    /// <summary>
    /// Tüm içerik panellerini kapatır ve buton renklerini sıfırlar.
    /// </summary>
    public void CloseAllTabs()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].contentPanel != null)
                tabs[i].contentPanel.SetActive(false);

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
            bool isActive = (i == index);
            
            // Paneli aç / kapat
            if (tabs[i].contentPanel != null)
            {
                tabs[i].contentPanel.SetActive(isActive);
            }

            // Seçili butonu belirginleştirmek için renk veya saydamlık ayarı yapabiliriz
            if (tabs[i].tabButton != null)
            {
                ColorBlock colors = tabs[i].tabButton.colors;
                colors.normalColor = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
                tabs[i].tabButton.colors = colors;
            }
        }
    }
}
