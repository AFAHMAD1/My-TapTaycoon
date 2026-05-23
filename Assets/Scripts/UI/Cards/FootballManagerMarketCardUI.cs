using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FootballManagerMarketCardUI : MarketCardUIBase
{
    private FootballManagerMarketData manager;

    private static readonly Color StatTextColor = new Color(0.08f, 0.11f, 0.14f, 1f);
    private static readonly Color OwnedStatButtonColor = new Color(0.96f, 0.73f, 0.08f, 1f);
    private static readonly Color OwnedStatButtonTextColor = new Color(0.08f, 0.11f, 0.14f, 1f);

    private const float OwnedStatButtonSize = 22f;
    private const float OwnedStatButtonGap = 7f;

    private bool ownedStatButtonsVisible;

    private static readonly OwnedStatButtonRow[] OwnedStatButtonRows =
    {
        new OwnedStatButtonRow("ManagerLevel", "Manager Level", "ManagerLevelPlusButton", ManagerStat.ManagerLevel),
        new OwnedStatButtonRow("Pass_Accuracy", "Training Boost", "TrainingBoostPlusButton", ManagerStat.TrainingBoost),
        new OwnedStatButtonRow("Defensive_Ability", "Tactic Boost", "TacticBoostPlusButton", ManagerStat.TacticBoost),
        new OwnedStatButtonRow("Strength", "Experience", "ExperiencePlusButton", ManagerStat.Experience),
        new OwnedStatButtonRow("Football_IQ", "Football IQ", "FootballIQPlusButton", ManagerStat.FootballIQ)
    };

    public void Bind(FootballManagerMarketData manager)
    {
        this.manager = manager;
        SetText("Name", manager.Name);
        SetText("Class", $"Class: {manager.Class}");
        SetPrice(manager.Price);
        SetStats(manager.Stats);
        if (ownedStatButtonsVisible)
        {
            RefreshOwnedStatButtons();
        }
    }

    public void ShowOwnedStatButtons()
    {
        ownedStatButtonsVisible = true;
        RefreshOwnedStatButtons();
    }

    public void HideOwnedStatButtons()
    {
        ownedStatButtonsVisible = false;
        HideExistingOwnedStatButtons();
    }

    private void SetStats(ManagerStats stats)
    {
        SetStatText("ManagerLevel", "Manager Level", $"Manager Level - {stats.managerLevel}");
        SetStatText("Pass_Accuracy", "Training Boost", $"Training Boost - {stats.trainingBoost}");
        SetStatText("Defensive_Ability", "Tactic Boost", $"Tactic Boost - {stats.tacticBoost}");
        SetStatText("Strength", "Experience", $"Experience - {stats.experience}");
        SetStatText("Shoot_Power", "Age", $"Age - {stats.age}");
        SetStatText("Football_IQ", "Football", $"Football IQ - {stats.footballIQ}");
    }

    private void SetStatText(string objectName, string labelPrefix, string value)
    {
        TMP_Text text = FindText(objectName);
        if (text == null)
        {
            text = FindTextByPrefix(labelPrefix);
        }

        if (text == null)
        {
            return;
        }

        text.text = value;
        text.color = StatTextColor;
    }

    private void RefreshOwnedStatButtons()
    {
        foreach (OwnedStatButtonRow row in OwnedStatButtonRows)
        {
            Button button = EnsureOwnedStatButton(row);
            if (button == null)
            {
                continue;
            }

            ManagerStat stat = row.Stat;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => TryUpgradeStat(stat));
            button.gameObject.SetActive(ownedStatButtonsVisible);
            button.interactable = ownedStatButtonsVisible && manager != null && manager.CanUpgradeStat(stat);
        }
    }

    private void HideExistingOwnedStatButtons()
    {
        foreach (OwnedStatButtonRow row in OwnedStatButtonRows)
        {
            Transform existing = UIHelper.FindChildRecursive(transform, row.ButtonName);
            if (existing == null)
            {
                continue;
            }

            existing.gameObject.SetActive(false);
            if (existing.TryGetComponent(out Button button))
            {
                button.interactable = false;
            }
        }
    }

    private Button EnsureOwnedStatButton(OwnedStatButtonRow row)
    {
        Transform existing = UIHelper.FindChildRecursive(transform, row.ButtonName);
        if (existing != null)
        {
            return existing.GetComponent<Button>();
        }

        Transform statTextTransform = FindStatTextTransform(row);
        if (statTextTransform == null)
        {
            return null;
        }

        RectTransform statRect = statTextTransform.GetComponent<RectTransform>();
        RectTransform parentRect = statTextTransform.parent as RectTransform;
        if (statRect == null || parentRect == null)
        {
            return null;
        }

        GameObject buttonObject = new GameObject(row.ButtonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parentRect, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = statRect.anchorMin;
        buttonRect.anchorMax = statRect.anchorMax;
        buttonRect.pivot = statRect.pivot;
        buttonRect.sizeDelta = new Vector2(OwnedStatButtonSize, OwnedStatButtonSize);
        buttonRect.anchoredPosition = GetButtonPosition(statRect, parentRect);

        Image image = buttonObject.GetComponent<Image>();
        image.color = OwnedStatButtonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        button.onClick.RemoveAllListeners();

        GameObject textObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = "+";
        text.fontSize = 18f;
        text.fontStyle = FontStyles.Bold;
        text.color = OwnedStatButtonTextColor;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return button;
    }

    private Transform FindStatTextTransform(OwnedStatButtonRow row)
    {
        TMP_Text text = FindText(row.StatTextObjectName);
        if (text == null)
        {
            text = FindTextByPrefix(row.LabelPrefix);
        }

        return text != null ? text.transform : null;
    }

    private static Vector2 GetButtonPosition(RectTransform statRect, RectTransform parentRect)
    {
        float statHalfWidth = statRect.rect.width > 0f ? statRect.rect.width * 0.5f : statRect.sizeDelta.x * 0.5f;
        float parentHalfWidth = parentRect.rect.width > 0f ? parentRect.rect.width * 0.5f : parentRect.sizeDelta.x * 0.5f;
        float wantedX = statRect.anchoredPosition.x + statHalfWidth + OwnedStatButtonGap + (OwnedStatButtonSize * 0.5f);
        float maxX = parentHalfWidth - (OwnedStatButtonSize * 0.5f) - 2f;
        float x = Mathf.Min(wantedX, maxX);
        return new Vector2(x, statRect.anchoredPosition.y);
    }

    private void TryUpgradeStat(ManagerStat stat)
    {
        if (manager == null || !manager.CanUpgradeStat(stat))
        {
            return;
        }

        double upgradeCost = manager.GetStatUpgradeCost(stat);
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[MarketPanel] CurrencyManager bulunamadi; menajer kriteri gelistirilemedi.", this);
            return;
        }

        if (!CurrencyManager.Instance.SpendMoney(upgradeCost))
        {
            Debug.Log("[MarketPanel] Menajer kriteri gelistirmesi icin para yetersiz.", this);
            return;
        }

        manager.TryApplyPaidStatUpgrade(stat, out _);
        Bind(manager);
    }

    private readonly struct OwnedStatButtonRow
    {
        public readonly string StatTextObjectName;
        public readonly string LabelPrefix;
        public readonly string ButtonName;
        public readonly ManagerStat Stat;

        public OwnedStatButtonRow(string statTextObjectName, string labelPrefix, string buttonName, ManagerStat stat)
        {
            StatTextObjectName = statTextObjectName;
            LabelPrefix = labelPrefix;
            ButtonName = buttonName;
            Stat = stat;
        }
    }
}
