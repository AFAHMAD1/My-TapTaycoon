using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class FootballPlayerMarketCardUI : MarketCardUIBase
{
    private FootballPlayerMarketData player;
    private bool ownedStatButtonsVisible;

    private const float OwnedStatButtonSize = 22f;
    private const float OwnedStatButtonGap = 7f;

    private static readonly Color OwnedStatButtonColor = new Color(0.96f, 0.73f, 0.08f, 1f);
    private static readonly Color OwnedStatButtonTextColor = new Color(0.08f, 0.11f, 0.14f, 1f);

    private static readonly OwnedStatButtonRow[] OwnedStatButtonRows =
    {
        new OwnedStatButtonRow("Speed", "SpeedPlusButton", FootballStat.Speed),
        new OwnedStatButtonRow("Pass_Accuracy", "PassAccuracyPlusButton", FootballStat.PassAccuracy),
        new OwnedStatButtonRow("Defensive_Ability", "DefensiveAbilityPlusButton", FootballStat.DefensiveAbility),
        new OwnedStatButtonRow("Strength", "StrengthPlusButton", FootballStat.Strength),
        new OwnedStatButtonRow("Shoot_Power", "ShootPowerPlusButton", FootballStat.ShootPower),
        new OwnedStatButtonRow("BallControl", "BallControlPlusButton", FootballStat.BallControl)
    };

    private static readonly Dictionary<string, FootballStat> StatButtonMap = new Dictionary<string, FootballStat>(StringComparer.OrdinalIgnoreCase)
    {
        { "UpgradeSpeedButton", FootballStat.Speed },
        { "UpgradePassAccuracyButton", FootballStat.PassAccuracy },
        { "UpgradeDefensiveAbilityButton", FootballStat.DefensiveAbility },
        { "UpgradeStrengthButton", FootballStat.Strength },
        { "UpgradeShootPowerButton", FootballStat.ShootPower }
    };

    public void Bind(FootballPlayerMarketData player)
    {
        this.player = player;
        SetText("Name", player.Name);
        SetText("Position", player.Type);
        SetText("Class", $"Class: {player.Class}");
        SetText("Age", $"Age - {player.Age}");
        SetText("Height", $"Height - {player.Height}");
        SetPrice(player.Price);
        SetStats(player.Stats);
        UpdateStatButtons();
        RefreshOwnedStatButtons();
    }

    public void ShowOwnedStatButtons()
    {
        ownedStatButtonsVisible = true;
        RefreshOwnedStatButtons();
    }

    public void HideOwnedStatButtons()
    {
        ownedStatButtonsVisible = false;
        RefreshOwnedStatButtons();
    }

    private void SetStats(FootballPlayerStats stats)
    {
        SetText("Speed", $"Speed - {stats.speed}");
        SetText("Pass_Accuracy", $"Pass_Accuracy - {stats.passAccuracy}");
        SetText("Defensive_Ability", $"Defensive_A - {stats.defensiveAbility}");
        SetText("Strength", $"Strength - {stats.strength}");
        SetText("Shoot_Power", $"Shoot_Power - {stats.shootPower}");
        SetText("BallControl", $"Ball_Control - {stats.ballControl}");
    }

    private void UpdateStatButtons()
    {
        if (player == null)
        {
            return;
        }

        foreach (var kvp in StatButtonMap)
        {
            Button button = UIHelper.FindButton(transform, kvp.Key);
            if (button == null)
            {
                continue;
            }

            FootballStat stat = kvp.Value;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => TryUpgradeStat(stat));
            button.interactable = player.CanUpgradeStat(stat);
        }
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

            FootballStat stat = row.Stat;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => TryUpgradeStat(stat));
            button.gameObject.SetActive(ownedStatButtonsVisible);
            button.interactable = ownedStatButtonsVisible && player != null && player.CanUpgradeStat(stat);
        }
    }

    private Button EnsureOwnedStatButton(OwnedStatButtonRow row)
    {
        Transform existing = UIHelper.FindChildRecursive(transform, row.ButtonName);
        if (existing != null)
        {
            return existing.GetComponent<Button>();
        }

        Transform statTextTransform = UIHelper.FindChildRecursive(transform, row.StatTextObjectName);
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

    private static Vector2 GetButtonPosition(RectTransform statRect, RectTransform parentRect)
    {
        float statHalfWidth = statRect.rect.width > 0f ? statRect.rect.width * 0.5f : statRect.sizeDelta.x * 0.5f;
        float parentHalfWidth = parentRect.rect.width > 0f ? parentRect.rect.width * 0.5f : parentRect.sizeDelta.x * 0.5f;
        float wantedX = statRect.anchoredPosition.x + statHalfWidth + OwnedStatButtonGap + (OwnedStatButtonSize * 0.5f);
        float maxX = parentHalfWidth - (OwnedStatButtonSize * 0.5f) - 2f;
        float x = Mathf.Min(wantedX, maxX);
        return new Vector2(x, statRect.anchoredPosition.y);
    }

    private void TryUpgradeStat(FootballStat stat)
    {
        if (player == null || !player.CanUpgradeStat(stat))
        {
            return;
        }

        double upgradeCost = player.GetStatUpgradeCost(stat);
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[MarketPanel] CurrencyManager bulunamadi; futbolcu kriteri gelistirilemedi.", this);
            return;
        }

        if (!CurrencyManager.Instance.SpendMoney(upgradeCost))
        {
            Debug.Log("[MarketPanel] Futbolcu kriteri gelistirmesi icin para yetersiz.", this);
            return;
        }

        player.TryApplyPaidStatUpgrade(stat, out _);
        Bind(player);
    }

    private readonly struct OwnedStatButtonRow
    {
        public readonly string StatTextObjectName;
        public readonly string ButtonName;
        public readonly FootballStat Stat;

        public OwnedStatButtonRow(string statTextObjectName, string buttonName, FootballStat stat)
        {
            StatTextObjectName = statTextObjectName;
            ButtonName = buttonName;
            Stat = stat;
        }
    }
}
