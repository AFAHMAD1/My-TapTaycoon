using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TapLevelRiserWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button upgradeButton;

    private TextMeshProUGUI buttonLabel;
    private float nextRefreshTime;
    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        Refresh();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime) return;

        nextRefreshTime = Time.unscaledTime + 0.25f;
        Refresh();
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
        }
    }

    public void Initialize()
    {
        if (initialized) return;

        ResolveReferences();
        ConfigurePrefabLayout();

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        initialized = true;
        Refresh();
    }

    private void ResolveReferences()
    {
        titleText ??= UIHelper.FindText(transform, "IncomeText", "Middel_Text", "BuildingNameText", "TapButton");
        descriptionText ??= UIHelper.FindText(transform, "TapRiserDescriptionText");
        priceText ??= UIHelper.FindText(transform, "PriceText", "Fiyat");
        levelText ??= UIHelper.FindText(transform, "LevelText");
        upgradeButton ??= UIHelper.FindButton(transform, "TapButton", "ActionButton", "Upgrade_Button");

        if (upgradeButton == null)
        {
            upgradeButton = GetComponentInChildren<Button>(true);
        }

        if (upgradeButton != null)
        {
            buttonLabel = upgradeButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    private void ConfigurePrefabLayout()
    {
        EnsureDescriptionText();

        RectTransform rootRect = transform as RectTransform;
        if (rootRect != null)
        {
            rootRect.localScale = Vector3.one;
        }

        RectTransform prefabRoot = UIHelper.FindChildRecursive(transform, "Tap_Level_Riser") as RectTransform;
        if (prefabRoot != null && prefabRoot != rootRect)
        {
            SetStretch(prefabRoot, 0f, 0f, 0f, 0f);
            prefabRoot.localScale = Vector3.one;
            prefabRoot.gameObject.SetActive(true);
        }

        RectTransform nestedCanvas = UIHelper.FindChildRecursive(transform, "Canvas") as RectTransform;
        if (nestedCanvas != null && nestedCanvas != rootRect)
        {
            SetStretch(nestedCanvas, 0f, 0f, 0f, 0f);
            nestedCanvas.localScale = Vector3.one;
        }

        RectTransform levelPanel = UIHelper.FindChildRecursive(transform, "Level_up_panel") as RectTransform;
        if (levelPanel != null)
        {
            SetStretch(levelPanel, 4f, 4f, 4f, 4f);
        }

        RectTransform iconPanel = FindByPartialName(transform, "Icon's_Panel") as RectTransform;
        if (iconPanel != null)
        {
            iconPanel.anchorMin = new Vector2(0f, 0f);
            iconPanel.anchorMax = new Vector2(0f, 1f);
            iconPanel.pivot = new Vector2(0.5f, 0.5f);
            iconPanel.anchoredPosition = new Vector2(54f, 0f);
            iconPanel.sizeDelta = new Vector2(96f, -12f);
            iconPanel.localScale = Vector3.one;
        }

        RectTransform middlePanel = FindByPartialName(transform, "Text's_Panel") as RectTransform;
        if (middlePanel != null)
        {
            SetStretch(middlePanel, 108f, 8f, 206f, 8f);
        }

        RectTransform buttonPanel = UIHelper.FindChildRecursive(transform, "RightArea") as RectTransform;
        if (buttonPanel == null)
        {
            buttonPanel = UIHelper.FindChildRecursive(transform, "Button_Panel") as RectTransform;
        }

        if (buttonPanel != null)
        {
            buttonPanel.gameObject.SetActive(true);
            buttonPanel.anchorMin = new Vector2(1f, 0f);
            buttonPanel.anchorMax = new Vector2(1f, 1f);
            buttonPanel.pivot = new Vector2(0.5f, 0.5f);
            buttonPanel.anchoredPosition = new Vector2(-102f, 0f);
            buttonPanel.sizeDelta = new Vector2(196f, -12f);
            buttonPanel.localScale = Vector3.one;
        }

        RectTransform priceRect = priceText != null ? priceText.rectTransform : null;
        if (priceRect != null)
        {
            priceRect.anchorMin = new Vector2(0f, 0.62f);
            priceRect.anchorMax = new Vector2(1f, 1f);
            priceRect.pivot = new Vector2(0.5f, 0.5f);
            priceRect.offsetMin = new Vector2(4f, 0f);
            priceRect.offsetMax = new Vector2(-4f, -2f);
            priceRect.localScale = Vector3.one;
        }

        RectTransform upgradeButtonRect = upgradeButton != null ? upgradeButton.transform as RectTransform : null;
        if (upgradeButtonRect != null)
        {
            upgradeButton.gameObject.SetActive(true);
            upgradeButtonRect.anchorMin = new Vector2(0f, 0.05f);
            upgradeButtonRect.anchorMax = new Vector2(1f, 0.62f);
            upgradeButtonRect.pivot = new Vector2(0.5f, 0.5f);
            upgradeButtonRect.offsetMin = new Vector2(8f, 4f);
            upgradeButtonRect.offsetMax = new Vector2(-8f, -4f);
            upgradeButtonRect.localScale = Vector3.one;
        }

        if (buttonLabel != null)
        {
            RectTransform labelRect = buttonLabel.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.localScale = Vector3.one;
            buttonLabel.raycastTarget = false;
        }

        if (titleText != null)
        {
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.48f);
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(8f, 0f);
            titleRect.offsetMax = new Vector2(-8f, -2f);
            titleRect.localScale = Vector3.one;

            titleText.alignment = TextAlignmentOptions.Midline;
            titleText.enableAutoSizing = false;
            titleText.fontSize = 20f;
            titleText.overflowMode = TextOverflowModes.Truncate;
            titleText.raycastTarget = false;
        }

        if (descriptionText != null)
        {
            RectTransform descriptionRect = descriptionText.rectTransform;
            descriptionRect.anchorMin = Vector2.zero;
            descriptionRect.anchorMax = new Vector2(1f, 0.48f);
            descriptionRect.offsetMin = new Vector2(8f, 2f);
            descriptionRect.offsetMax = new Vector2(-8f, 0f);
            descriptionRect.localScale = Vector3.one;

            descriptionText.alignment = TextAlignmentOptions.Midline;
            descriptionText.enableAutoSizing = false;
            descriptionText.fontSize = titleText != null ? titleText.fontSize : 20f;
            descriptionText.fontStyle = titleText != null ? titleText.fontStyle : FontStyles.Normal;
            descriptionText.overflowMode = TextOverflowModes.Truncate;
            descriptionText.raycastTarget = false;
        }

        if (priceText != null)
        {
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.enableAutoSizing = false;
            priceText.fontSize = 28f;
            priceText.overflowMode = TextOverflowModes.Truncate;
            priceText.raycastTarget = false;
        }

        if (levelText != null)
        {
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.enableAutoSizing = false;
            levelText.fontSize = 20f;
            levelText.overflowMode = TextOverflowModes.Truncate;
            levelText.raycastTarget = false;
        }

        if (buttonLabel != null)
        {
            buttonLabel.alignment = TextAlignmentOptions.Center;
            buttonLabel.enableAutoSizing = false;
            buttonLabel.fontSize = 26f;
            buttonLabel.overflowMode = TextOverflowModes.Truncate;
        }
    }

    private void Refresh()
    {
        if (UpgradeManager.Instance == null)
        {
            if (upgradeButton != null) upgradeButton.interactable = false;
            return;
        }

        int level = UpgradeManager.Instance.TapLevelRiserLevel;
        double cost = UpgradeManager.Instance.TapLevelRiserNextCost;

        if (titleText != null)
        {
            titleText.text = "Para Masteri";
        }

        if (descriptionText != null)
        {
            descriptionText.text = "Daha Fazla Para Kazan";
        }

        if (priceText != null)
        {
            priceText.text = "Fiyat: $" + NumberFormatter.FormatPrice(cost);
        }

        if (levelText != null)
        {
            levelText.text = $"Lv.{level}";
        }

        if (buttonLabel != null)
        {
            buttonLabel.text = "Yukselt";
        }

        if (upgradeButton != null)
        {
            upgradeButton.interactable = UpgradeManager.Instance.CanBuyTapLevelRiser();
        }
    }

    private void OnUpgradeClicked()
    {
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryBuyTapLevelRiser())
        {
            Refresh();
        }
    }

    private void EnsureDescriptionText()
    {
        if (descriptionText != null || titleText == null) return;

        GameObject descriptionObject = new GameObject("TapRiserDescriptionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        descriptionObject.layer = titleText.gameObject.layer;
        descriptionObject.transform.SetParent(titleText.transform.parent, false);

        descriptionText = descriptionObject.GetComponent<TextMeshProUGUI>();
        descriptionText.font = titleText.font;
        descriptionText.fontSharedMaterial = titleText.fontSharedMaterial;
        descriptionText.color = titleText.color;
        descriptionText.fontStyle = titleText.fontStyle;
        descriptionText.fontSize = titleText.fontSize;
    }

    private static void SetStretch(RectTransform rect, float left, float bottom, float right, float top)
    {
        if (rect == null) return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.localScale = Vector3.one;
    }

    private static Transform FindByPartialName(Transform parent, string partialName)
    {
        if (parent == null) return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.Contains(partialName))
            {
                return child;
            }

            Transform nested = FindByPartialName(child, partialName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }
}
