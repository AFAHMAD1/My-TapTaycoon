using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class BaseUpgradePanel : MonoBehaviour
{
    private static readonly int[] BuyAmountOptions = { 1, 10, 100, int.MaxValue };

    [Header("Arayuz Ayarlari")]
    public GameObject cardPrefab;
    
    [HideInInspector]
    public Transform contentContainer;
    
    [Header("Panel Ayarlari")]
    public string panelTitle = "Yeni Panel";
    public TextMeshProUGUI titleText;
    
    [Tooltip("Bu panelin en ustunde satin alma carpanlari (x1, x10, MAX) gosterilsin mi?")]
    public bool showBuyModeSelector = true;

    [Header("Yerlesim")]
    public float cardHeight = 220f;
    public float cardSpacing = 24f;
    public int topPadding = 12;
    public int sidePadding = 12;

    public int SelectedBuyAmount { get; private set; } = 1;

    protected List<UpgradeCard> uiCards = new List<UpgradeCard>();
    
    // UI objeleri buy mode dropdown için
    protected Transform buyModeButtonContext_SummaryBar;
    protected Button buyModeButtonContext_Button;
    protected TextMeshProUGUI buyModeButtonContext_Label;
    protected GameObject buyModeButtonContext_DropdownRoot;

    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    protected virtual void Start()
    {
        if (cardPrefab == null)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogError' metodunu cagirir; Unity Console'a hata mesaji yazar; duzeltilmesi gereken ciddi durumlari belirtir.
            Debug.LogError($"[{GetType().Name}] HATA: 'Card Prefab' alani bos! Lutfen Unity Inspector'dan UpgradeCard prefabini bu alana surukleyin.");
            return;
        }

        if (contentContainer == null)
        {
            // Eger kullanici atamayi unuttuysa, otomatik bulmaya calis:
            UnityEngine.UI.ScrollRect scrollRect = GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
            if (scrollRect != null && scrollRect.content != null)
            {
                contentContainer = scrollRect.content;
            }
            else
            {
                // Bu satir: 'Debug' objesi uzerindeki 'LogError' metodunu cagirir; Unity Console'a hata mesaji yazar; duzeltilmesi gereken ciddi durumlari belirtir.
                Debug.LogError($"[{GetType().Name}] HATA: 'Content Container' bulunamadi! Lutfen Inspector'dan atayin veya panele bir Scroll View ekleyin.");
                return;
            }
        }

        ConfigureContentLayout();
        
        // Başlığı ayarla
        if (titleText != null) titleText.text = panelTitle;

        if (showBuyModeSelector)
        {
            SetupBuyModeButton();
        }

        CreateCards();
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    protected abstract List<ICardDataProvider> GetDataProviders();

    // Panel acildiginda eski kartlari temizler, veri listesini alir ve her veri icin yeni kart olusturur.
    protected virtual void CreateCards()
    {
        ClearContainer();
        
        List<ICardDataProvider> providers = GetDataProviders();
        if (providers == null) return;

        // Bu dongu listedeki her bina/skill/kit verisi icin ekrana bir kart basar.
        foreach (var provider in providers)
        {
            CreateSingleCard(provider);
        }
    }
    
    // Card Prefab alanindaki prefab'i Content altina clone'lar ve karta hangi veriyi gosterecegini soyler.
    protected UpgradeCard CreateSingleCard(ICardDataProvider provider)
    {
        // Instantiate burada prefab'i sahnede cogaltir; parent olarak Scroll View'in Content objesi verilir.
        GameObject cardObj = Instantiate(cardPrefab, contentContainer);
        ConfigureCardLayout(cardObj);
        
        // Prefab'in icinde UpgradeCard yoksa kart davranisini runtime'da ekler.
        UpgradeCard uiCard = cardObj.GetComponent<UpgradeCard>();
        if (uiCard == null)
        {
            uiCard = cardObj.AddComponent<UpgradeCard>();
            // Eski componenti silebiliriz eğer varsa ama biz temiz çalışacağız
        }

        // Bu satir: 'uiCard' objesi uzerindeki 'Setup' metodunu cagirir; UI kartini veya gorsel objeyi verilen veriyle kullanima hazirlar.
        uiCard.Setup(provider, () => SelectedBuyAmount);
        // Bu satir: 'uiCards' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
        uiCards.Add(uiCard);
        return uiCard;
    }
    
    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    protected GameObject CreateSectionHeader(string title)
    {
        GameObject headerObj = new GameObject($"Header_{title}", typeof(RectTransform), typeof(LayoutElement));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        headerObj.transform.SetParent(contentContainer, false);
        
        LayoutElement layoutElement = headerObj.GetComponent<LayoutElement>();
        layoutElement.minHeight = 40f;
        layoutElement.preferredHeight = 40f;
        layoutElement.flexibleHeight = 0f;
        
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        textObj.transform.SetParent(headerObj.transform, false);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = title;
        text.fontSize = 28f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        
        return headerObj;
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    protected void ClearContainer()
    {
        // Bu satir: 'uiCards' objesi uzerindeki 'Clear' metodunu cagirir; listenin icindeki tum elemanlari siler; liste bos hale gelir.
        uiCards.Clear();
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(contentContainer.GetChild(i).gameObject);
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    protected void ConfigureContentLayout()
    {
        VerticalLayoutGroup layoutGroup = contentContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null) layoutGroup = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        layoutGroup.spacing = cardSpacing;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.padding.left = sidePadding;
        layoutGroup.padding.right = sidePadding;
        layoutGroup.padding.top = topPadding;
        layoutGroup.padding.bottom = topPadding;

        ContentSizeFitter fitter = contentContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void ConfigureCardLayout(GameObject cardObj)
    {
        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(0f, cardHeight);

        LayoutElement layoutElement = cardObj.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = cardObj.AddComponent<LayoutElement>();

        // Bileşenin aktif olduğundan emin olalım (UI kaymalarını önlemek için)
        layoutElement.enabled = true;
        layoutElement.minHeight = cardHeight;
        layoutElement.preferredHeight = cardHeight;
        layoutElement.flexibleHeight = 0f;

        // Kartın arka plan görüntüsünün açık olduğundan emin olalım
        if (cardObj.TryGetComponent<UnityEngine.UI.Image>(out var img))
        {
            img.enabled = true;
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    protected virtual void SetupBuyModeButton()
    {
        // Bu satir: 'UIHelper' uzerindeki 'FindChildRecursive' metodunu cagirir ve sonucu 'panelSummaryBar' degiskenine koyar; verilen parent'in alt cocuklarinda isme gore derin arama yapar. Normal Find sadece tek seviye bakarken bu metot alt seviyelere de iner.
        Transform panelSummaryBar = UIHelper.FindChildRecursive(transform, "SummaryBar");
        
        if (panelSummaryBar == null)
        {
            // Eger SummaryBar yoksa, otomatik olarak panelin icine (en uste) olustur
            GameObject summaryObj = new GameObject("SummaryBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
            summaryObj.transform.SetParent(transform, false);
            summaryObj.transform.SetSiblingIndex(0); // ScrollView'in ustunde gorunmesi icin en uste aliyoruz
            
            RectTransform rect = summaryObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 50f);

            HorizontalLayoutGroup layout = summaryObj.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.padding = new RectOffset(10, 20, 5, 5);
            layout.childControlHeight = false;
            layout.childControlWidth = false;

            LayoutElement le = summaryObj.GetComponent<LayoutElement>();
            le.minHeight = 50f;

            panelSummaryBar = summaryObj.transform;
        }

        // Bu satir: 'UIHelper' uzerindeki 'FindChildRecursive' metodunu cagirir ve sonucu 'buyModeTransform' degiskenine koyar; verilen parent'in alt cocuklarinda isme gore derin arama yapar. Normal Find sadece tek seviye bakarken bu metot alt seviyelere de iner.
        Transform buyModeTransform = UIHelper.FindChildRecursive(panelSummaryBar, "BuyModeButton");
        if (buyModeTransform == null)
        {
            // BuyModeButton yoksa otomatik olustur
            GameObject buttonObj = new GameObject("BuyModeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
            buttonObj.transform.SetParent(panelSummaryBar, false);
            
            RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(120f, 40f);

            Image btnImg = buttonObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 1f, 1f); // Mavi bir buton

            GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform txtRect = textObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();
            txt.text = "x1";
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.fontSize = 20f;

            buyModeTransform = buttonObj.transform;
        }

        buyModeButtonContext_SummaryBar = panelSummaryBar;
        buyModeButtonContext_Button = buyModeTransform.GetComponent<Button>();
        buyModeButtonContext_Label = buyModeTransform.GetComponentInChildren<TextMeshProUGUI>(true);

        if (buyModeButtonContext_Button != null)
        {
            // Bu satir: 'onClick' objesi uzerindeki 'RemoveAllListeners' metodunu cagirir; butondaki eski tiklama baglantilarini temizler; ayni is birden fazla kez calismasin diye kullanilir.
            buyModeButtonContext_Button.onClick.RemoveAllListeners();
            // Bu satir: 'onClick' objesi uzerindeki 'AddListener' metodunu cagirir; buton/toggle gibi UI olayina fonksiyon baglar; kullanici tiklayinca bu fonksiyon calisir.
            buyModeButtonContext_Button.onClick.AddListener(ToggleBuyModeDropdown);
            EnsureBuyModeDropdown();
            RefreshBuyModeButtonLabel();
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void EnsureBuyModeDropdown()
    {
        if (buyModeButtonContext_DropdownRoot != null || buyModeButtonContext_SummaryBar == null) return;

        GameObject dropdownObject = new GameObject("BuyModeDropdown", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        dropdownObject.transform.SetParent(buyModeButtonContext_SummaryBar, false);

        RectTransform dropdownRect = dropdownObject.GetComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0f, 1f);
        dropdownRect.anchorMax = new Vector2(0f, 1f);
        dropdownRect.pivot = new Vector2(1f, 0.5f);
        dropdownRect.sizeDelta = new Vector2(320f, 60f);

        Image dropdownImage = dropdownObject.GetComponent<Image>();
        dropdownImage.color = new Color(0.16f, 0.17f, 0.2f, 0.98f);

        HorizontalLayoutGroup layoutGroup = dropdownObject.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.padding = new RectOffset(6, 6, 6, 6);
        layoutGroup.spacing = 4f;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;

        LayoutElement layoutElement = dropdownObject.GetComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (int amount in BuyAmountOptions)
        {
            CreateBuyModeOption(dropdownObject.transform, amount);
        }

        buyModeButtonContext_DropdownRoot = dropdownObject;
        // Bu satir: 'buyModeButtonContext_DropdownRoot' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
        buyModeButtonContext_DropdownRoot.SetActive(false);
        PositionDropdown();
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void CreateBuyModeOption(Transform parent, int amount)
    {
        GameObject optionObject = new GameObject($"Option_{GetBuyAmountLabel(amount)}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        optionObject.transform.SetParent(parent, false);

        RectTransform optionRect = optionObject.GetComponent<RectTransform>();
        optionRect.sizeDelta = new Vector2(72f, 0f);

        Image optionImage = optionObject.GetComponent<Image>();
        optionImage.color = new Color(0.28f, 0.3f, 0.35f, 1f);

        Button optionButton = optionObject.GetComponent<Button>();
        optionButton.targetGraphic = optionImage;
        // Bu satir: 'onClick' objesi uzerindeki 'AddListener' metodunu cagirir; buton/toggle gibi UI olayina fonksiyon baglar; kullanici tiklayinca bu fonksiyon calisir.
        optionButton.onClick.AddListener(() => SetSelectedBuyAmount(amount));

        LayoutElement layoutElement = optionObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = amount >= int.MaxValue ? 82f : 72f;
        layoutElement.preferredHeight = 34f;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        labelObject.transform.SetParent(optionObject.transform, false);
        
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = GetBuyAmountLabel(amount);
        label.fontSize = 24f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        if (buyModeButtonContext_Label != null)
        {
            label.font = buyModeButtonContext_Label.font;
            label.fontSharedMaterial = buyModeButtonContext_Label.fontSharedMaterial;
        }
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    private void ToggleBuyModeDropdown()
    {
        if (buyModeButtonContext_DropdownRoot == null) return;
        
        bool active = !buyModeButtonContext_DropdownRoot.activeSelf;
        // Bu satir: 'buyModeButtonContext_DropdownRoot' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
        buyModeButtonContext_DropdownRoot.SetActive(active);
        
        if (active)
        {
            PositionDropdown();
            // Bu satir: 'transform' objesi uzerindeki 'SetAsLastSibling' metodunu cagirir; objeyi parent icinde en sona alir; UI'da genelde en onde gorunmesine yardim eder.
            buyModeButtonContext_DropdownRoot.transform.SetAsLastSibling();
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void SetSelectedBuyAmount(int amount)
    {
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'SelectedBuyAmount' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        SelectedBuyAmount = Mathf.Max(1, amount);
        RefreshBuyModeButtonLabel();
        
        if (buyModeButtonContext_DropdownRoot != null)
        {
            // Bu satir: 'buyModeButtonContext_DropdownRoot' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
            buyModeButtonContext_DropdownRoot.SetActive(false);
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void RefreshBuyModeButtonLabel()
    {
        if (buyModeButtonContext_Label != null)
        {
            buyModeButtonContext_Label.text = $"Satin Al {GetBuyAmountLabel(SelectedBuyAmount)}";
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    private string GetBuyAmountLabel(int amount)
    {
        return amount >= int.MaxValue ? "MAX" : $"x{amount}";
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    private void PositionDropdown()
    {
        if (buyModeButtonContext_Button == null || buyModeButtonContext_DropdownRoot == null) return;

        RectTransform buttonRect = buyModeButtonContext_Button.transform as RectTransform;
        RectTransform dropdownRect = buyModeButtonContext_DropdownRoot.transform as RectTransform;

        float spacing = 12f;
        float buttonLeft = buttonRect.anchoredPosition.x - (buttonRect.rect.width * buttonRect.pivot.x);
        float buttonCenterY = buttonRect.anchoredPosition.y + (buttonRect.rect.height * (0.5f - buttonRect.pivot.y));

        dropdownRect.anchoredPosition = new Vector2(buttonLeft - spacing, buttonCenterY);
    }
}
