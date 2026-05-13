using UnityEngine;
using UnityEngine.UI;

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
    }

    [Header("Alt Sekmeler")]
    public TabContent[] tabs;

    [Header("Açılış Ayarları")]
    public int defaultTabIndex = 0;
    public bool openDefaultTabOnStart = true;
    private bool hasStarted;

    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    private void Start()
    {
        // Tüm butonlara tıklama event'ini kodla otomatik ata
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // Closure sorunu yaşamamak için yerel değişken
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => OpenTab(index));
            }
        }

        // Oyun başladığında varsayılan sekmeyi aç.
        // KitPanel'de defaultTabIndex 2 oldugu icin ilk acilan alt panel SkillsPanel olur.
        hasStarted = true;
        ApplyOpeningState();
    }

    private void OnEnable()
    {
        if (hasStarted)
        {
            ApplyOpeningState();
        }
    }

    private void ApplyOpeningState()
    {
        if (openDefaultTabOnStart)
        {
            OpenTab(defaultTabIndex);
        }
        else
        {
            CloseAllTabs();
        }
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void OpenTab(int index)
    {
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
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
}
