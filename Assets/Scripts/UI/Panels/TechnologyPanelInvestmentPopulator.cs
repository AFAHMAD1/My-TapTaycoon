using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TechnologyPanelInvestmentPopulator : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject investmentTabTemplate;
    [SerializeField] private GameObject inputPanel;

    private const float CardHeight = 130f;
    private const float CardSpacing = 20f;
    private const float CardPadding = 10f;
    private const float IconAreaWidth = 130f;
    private const float RightAreaWidth = 230f;
    private const string GeneratedCardPrefix = "InvestmentCompany_";

    private bool hasPopulated;
    private InvestmentInputPanelController inputPanelController;

    private static readonly CompanyInfo[] Companies =
    {
        new CompanyInfo(1, "TechNova Solutions", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(2, "GreenLeaf Foods", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(3, "BluePeak Marketing", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(4, "BrightPath Education", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(5, "UrbanFix Services", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(6, "CloudCore IT", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(7, "FreshBox Delivery", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(8, "DesignHub Studio", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(9, "SafeHome Security", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(10, "QuickPrint Center", "K\u00fc\u00e7\u00fck", "kucuk firma"),
        new CompanyInfo(11, "Microsoft", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(12, "Apple", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(13, "Google", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(14, "Amazon", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(15, "Samsung", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(16, "Toyota", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(17, "Volkswagen", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(18, "Coca-Cola", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(19, "Nestl\u00e9", "B\u00fcy\u00fck", "buyuk firma"),
        new CompanyInfo(20, "IBM", "B\u00fcy\u00fck", "buyuk firma"),
    };

    private void Start()
    {
        Populate();
    }

    public void Populate()
    {
        if (hasPopulated)
        {
            return;
        }

        hasPopulated = true;

        if (!ResolveReferences())
        {
            return;
        }

        ConfigureScrollView();
        ClearPreviousRuntimeCards();

        investmentTabTemplate.SetActive(true);
        SetupCard(investmentTabTemplate, Companies[0]);

        for (int i = 1; i < Companies.Length; i++)
        {
            GameObject card = Instantiate(investmentTabTemplate, content);
            SetupCard(card, Companies[i]);
        }
    }

    private bool ResolveReferences()
    {
        if (scrollView == null)
        {
            scrollView = GetComponentInChildren<ScrollRect>(true);
        }

        if (content == null && scrollView != null)
        {
            content = scrollView.content;
        }

        if (content == null)
        {
            Debug.LogWarning("[TeknolojiPanel] Scroll View Content bulunamadi; yatirim kartlari olusturulamadi.", this);
            return false;
        }

        if (investmentTabTemplate == null)
        {
            investmentTabTemplate = FindInvestmentTemplate();
        }

        if (inputPanel == null)
        {
            inputPanel = FindChildGameObject("InputPanel");
        }

        if (inputPanel != null)
        {
            inputPanelController = inputPanel.GetComponent<InvestmentInputPanelController>();
            if (inputPanelController == null)
            {
                inputPanelController = inputPanel.AddComponent<InvestmentInputPanelController>();
            }

            inputPanelController.Configure();
        }

        if (investmentTabTemplate == null)
        {
            Debug.LogWarning("[TeknolojiPanel] InvestmentTab template bulunamadi; yatirim kartlari olusturulamadi.", this);
            return false;
        }

        return true;
    }

    private GameObject FindInvestmentTemplate()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (child.name.IndexOf("InvestmentTab", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return child.gameObject;
            }
        }

        return content.childCount > 0 ? content.GetChild(0).gameObject : null;
    }

    private void ConfigureScrollView()
    {
        if (scrollView != null)
        {
            scrollView.horizontal = false;
            scrollView.vertical = true;
        }

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = new RectOffset(12, 12, 10, 10);
        layout.spacing = CardSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void ClearPreviousRuntimeCards()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;
            if (child != investmentTabTemplate && child.name.StartsWith(GeneratedCardPrefix, StringComparison.Ordinal))
            {
                Destroy(child);
            }
        }
    }

    private void SetupCard(GameObject card, CompanyInfo company)
    {
        card.name = $"{GeneratedCardPrefix}{company.Id:00}_{company.Name}";
        ConfigureCardLayout(card);
        ConfigureCardInnerLayout(card.transform);

        TMP_Text priceText = FindText(card.transform, "PriceText");
        TMP_Text incomeText = FindText(card.transform, "IncomeText");
        Button actionButton = FindButton(card.transform);
        TMP_Text buttonText = actionButton != null ? actionButton.GetComponentInChildren<TMP_Text>(true) : null;

        if (priceText != null)
        {
            priceText.text = $"ID {company.Id} | Olcek: {company.Scale}";
            priceText.enableAutoSizing = true;
            priceText.fontSizeMin = 14f;
            priceText.fontSizeMax = 18f;
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.textWrappingMode = TextWrappingModes.NoWrap;
            priceText.overflowMode = TextOverflowModes.Truncate;
        }

        if (incomeText != null)
        {
            incomeText.text = $"{company.Name}\n{company.Description}";
            incomeText.enableAutoSizing = true;
            incomeText.fontSizeMin = 16f;
            incomeText.fontSizeMax = 22f;
            incomeText.alignment = TextAlignmentOptions.MidlineLeft;
            incomeText.textWrappingMode = TextWrappingModes.Normal;
            incomeText.overflowMode = TextOverflowModes.Truncate;
        }

        if (buttonText != null)
        {
            buttonText.text = "Yatirim Yap";
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = 16f;
            buttonText.fontSizeMax = 22f;
            buttonText.alignment = TextAlignmentOptions.Center;
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => OpenInputPanel(company));
        }
    }

    private void OpenInputPanel(CompanyInfo company)
    {
        if (inputPanel == null)
        {
            Debug.LogWarning($"[TeknolojiPanel] InputPanel bulunamadi. Secilen firma: {company.Name}", this);
            return;
        }

        inputPanel.SetActive(true);
        inputPanel.transform.SetAsLastSibling();
        inputPanelController?.FocusInput();
    }

    private static void ConfigureCardLayout(GameObject card)
    {
        RectTransform rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = new Vector2(0f, CardHeight);
        }

        LayoutElement layoutElement = card.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = card.AddComponent<LayoutElement>();
        }

        layoutElement.preferredHeight = CardHeight;
        layoutElement.flexibleWidth = 1f;
    }

    private static void ConfigureCardInnerLayout(Transform card)
    {
        RectTransform leftArea = FindRect(card, "LeftArea");
        RectTransform centerArea = FindRect(card, "CenterArea");
        RectTransform rightArea = FindRect(card, "RightArea");
        RectTransform icon = FindRect(card, "BuildingIcon");
        RectTransform priceText = FindRect(card, "PriceText");
        RectTransform incomeText = FindRect(card, "IncomeText");
        RectTransform actionButton = FindRect(card, "ActionButton");

        ConfigureRect(leftArea, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(CardPadding, 0f), new Vector2(IconAreaWidth, -CardPadding * 2f));
        ConfigureRect(rightArea, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-CardPadding, 0f), new Vector2(RightAreaWidth, -CardPadding * 2f));
        ConfigureRect(centerArea, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-(IconAreaWidth + RightAreaWidth + CardPadding * 4f), -CardPadding * 2f));

        if (centerArea != null)
        {
            centerArea.offsetMin = new Vector2(IconAreaWidth + CardPadding * 2f, CardPadding);
            centerArea.offsetMax = new Vector2(-(RightAreaWidth + CardPadding * 2f), -CardPadding);
        }

        ConfigureRect(icon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(92f, 92f));
        ConfigureRect(priceText, new Vector2(0f, 0.62f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-12f, -8f));
        ConfigureRect(actionButton, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.58f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        ConfigureRect(incomeText, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-16f, -12f));
    }

    private static void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.localScale = Vector3.one;
    }

    private static RectTransform FindRect(Transform root, string objectName)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            if (child.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return child as RectTransform;
            }
        }

        return null;
    }

    private static TMP_Text FindText(Transform root, string objectName)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }
        }

        return null;
    }

    private GameObject FindChildGameObject(string objectName)
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in transforms)
        {
            if (child.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static Button FindButton(Transform root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.gameObject.name.Equals("ActionButton", StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return buttons.Length > 0 ? buttons[0] : null;
    }

    private readonly struct CompanyInfo
    {
        public CompanyInfo(int id, string name, string scale, string description)
        {
            Id = id;
            Name = name;
            Scale = scale;
            Description = description;
        }

        public int Id { get; }
        public string Name { get; }
        public string Scale { get; }
        public string Description { get; }
    }
}
