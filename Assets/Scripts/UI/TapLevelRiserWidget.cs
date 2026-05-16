using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TapLevelRiserWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
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
        titleText ??= UIHelper.FindText(transform, "IncomeText", "Middel_Text", "BuildingNameText");
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
        RectTransform rootRect = transform as RectTransform;
        if (rootRect != null)
        {
            rootRect.localScale = Vector3.one;
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
            SetStretch(middlePanel, 108f, 8f, 168f, 8f);
        }

        RectTransform buttonPanel = UIHelper.FindChildRecursive(transform, "Button_Panel") as RectTransform;
        if (buttonPanel != null)
        {
            buttonPanel.anchorMin = new Vector2(1f, 0f);
            buttonPanel.anchorMax = new Vector2(1f, 1f);
            buttonPanel.pivot = new Vector2(0.5f, 0.5f);
            buttonPanel.anchoredPosition = new Vector2(-82f, 0f);
            buttonPanel.sizeDelta = new Vector2(156f, -12f);
            buttonPanel.localScale = Vector3.one;
        }

        if (titleText != null)
        {
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.enableAutoSizing = false;
            titleText.fontSize = 20f;
            titleText.overflowMode = TextOverflowModes.Truncate;
        }

        if (priceText != null)
        {
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.enableAutoSizing = false;
            priceText.fontSize = 20f;
            priceText.overflowMode = TextOverflowModes.Truncate;
        }

        if (levelText != null)
        {
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.enableAutoSizing = false;
            levelText.fontSize = 20f;
            levelText.overflowMode = TextOverflowModes.Truncate;
        }

        if (buttonLabel != null)
        {
            buttonLabel.alignment = TextAlignmentOptions.Center;
            buttonLabel.enableAutoSizing = false;
            buttonLabel.fontSize = 18f;
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
        double multiplier = UpgradeManager.Instance.TapLevelRiserMultiplier;
        double cost = UpgradeManager.Instance.TapLevelRiserNextCost;

        if (titleText != null)
        {
            titleText.text = $"Kazanc: x{NumberFormatter.Format(multiplier)} / Tap";
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
