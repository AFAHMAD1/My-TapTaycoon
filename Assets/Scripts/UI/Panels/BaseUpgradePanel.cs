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

    protected virtual void Start()
    {
        if (cardPrefab == null)
        {
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

    protected abstract List<ICardDataProvider> GetDataProviders();

    protected virtual void CreateCards()
    {
        ClearContainer();
        
        List<ICardDataProvider> providers = GetDataProviders();
        if (providers == null) return;

        foreach (var provider in providers)
        {
            CreateSingleCard(provider);
        }
    }
    
    protected UpgradeCard CreateSingleCard(ICardDataProvider provider)
    {
        GameObject cardObj = Instantiate(cardPrefab, contentContainer);
        ConfigureCardLayout(cardObj);
        
        // Eğer hedefte UpgradeCard yoksa ve BuildingUIItem varsa uyar / dönüştür
        // Şimdilik prefab'ın içinde UpgradeCard olduğunu varsayıyoruz.
        // Ama geriye dönük uyumluluk için, eğer prefabda sadece BuildingUIItem varsa ve UpgradeCard yoksa hata vermemeliyiz.
        // Aslında BuildingUIItem'i tamamen sileceğimiz için prefab'da UpgradeCard olmalı.
        UpgradeCard uiCard = cardObj.GetComponent<UpgradeCard>();
        if (uiCard == null)
        {
            uiCard = cardObj.AddComponent<UpgradeCard>();
            // Eski componenti silebiliriz eğer varsa ama biz temiz çalışacağız
        }

        uiCard.Setup(provider, () => SelectedBuyAmount);
        uiCards.Add(uiCard);
        return uiCard;
    }
    
    protected GameObject CreateSectionHeader(string title)
    {
        GameObject headerObj = new GameObject($"Header_{title}", typeof(RectTransform), typeof(LayoutElement));
        headerObj.transform.SetParent(contentContainer, false);
        
        LayoutElement layoutElement = headerObj.GetComponent<LayoutElement>();
        layoutElement.minHeight = 40f;
        layoutElement.preferredHeight = 40f;
        layoutElement.flexibleHeight = 0f;
        
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
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

    protected void ClearContainer()
    {
        uiCards.Clear();
        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(contentContainer.GetChild(i).gameObject);
        }
    }

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

    protected virtual void SetupBuyModeButton()
    {
        Transform panelSummaryBar = UIHelper.FindChildRecursive(transform, "SummaryBar");
        
        if (panelSummaryBar == null)
        {
            // Eger SummaryBar yoksa, otomatik olarak panelin icine (en uste) olustur
            GameObject summaryObj = new GameObject("SummaryBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
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

        Transform buyModeTransform = UIHelper.FindChildRecursive(panelSummaryBar, "BuyModeButton");
        if (buyModeTransform == null)
        {
            // BuyModeButton yoksa otomatik olustur
            GameObject buttonObj = new GameObject("BuyModeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(panelSummaryBar, false);
            
            RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(120f, 40f);

            Image btnImg = buttonObj.GetComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 1f, 1f); // Mavi bir buton

            GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
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
            buyModeButtonContext_Button.onClick.RemoveAllListeners();
            buyModeButtonContext_Button.onClick.AddListener(ToggleBuyModeDropdown);
            EnsureBuyModeDropdown();
            RefreshBuyModeButtonLabel();
        }
    }

    private void EnsureBuyModeDropdown()
    {
        if (buyModeButtonContext_DropdownRoot != null || buyModeButtonContext_SummaryBar == null) return;

        GameObject dropdownObject = new GameObject("BuyModeDropdown", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
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

        foreach (int amount in BuyAmountOptions)
        {
            CreateBuyModeOption(dropdownObject.transform, amount);
        }

        buyModeButtonContext_DropdownRoot = dropdownObject;
        buyModeButtonContext_DropdownRoot.SetActive(false);
        PositionDropdown();
    }

    private void CreateBuyModeOption(Transform parent, int amount)
    {
        GameObject optionObject = new GameObject($"Option_{GetBuyAmountLabel(amount)}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        optionObject.transform.SetParent(parent, false);

        RectTransform optionRect = optionObject.GetComponent<RectTransform>();
        optionRect.sizeDelta = new Vector2(72f, 0f);

        Image optionImage = optionObject.GetComponent<Image>();
        optionImage.color = new Color(0.28f, 0.3f, 0.35f, 1f);

        Button optionButton = optionObject.GetComponent<Button>();
        optionButton.targetGraphic = optionImage;
        optionButton.onClick.AddListener(() => SetSelectedBuyAmount(amount));

        LayoutElement layoutElement = optionObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = amount >= int.MaxValue ? 82f : 72f;
        layoutElement.preferredHeight = 34f;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
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

    private void ToggleBuyModeDropdown()
    {
        if (buyModeButtonContext_DropdownRoot == null) return;
        
        bool active = !buyModeButtonContext_DropdownRoot.activeSelf;
        buyModeButtonContext_DropdownRoot.SetActive(active);
        
        if (active)
        {
            PositionDropdown();
            buyModeButtonContext_DropdownRoot.transform.SetAsLastSibling();
        }
    }

    private void SetSelectedBuyAmount(int amount)
    {
        SelectedBuyAmount = Mathf.Max(1, amount);
        RefreshBuyModeButtonLabel();
        
        if (buyModeButtonContext_DropdownRoot != null)
        {
            buyModeButtonContext_DropdownRoot.SetActive(false);
        }
    }

    private void RefreshBuyModeButtonLabel()
    {
        if (buyModeButtonContext_Label != null)
        {
            buyModeButtonContext_Label.text = $"Satin Al {GetBuyAmountLabel(SelectedBuyAmount)}";
        }
    }

    private string GetBuyAmountLabel(int amount)
    {
        return amount >= int.MaxValue ? "MAX" : $"x{amount}";
    }

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
