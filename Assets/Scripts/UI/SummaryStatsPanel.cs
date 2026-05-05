using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryStatsPanel : MonoBehaviour
{
    private const string TapLabelText = "Dokunma Basina Kar";
    private const string BusinessLabelText = "Saniye Basina Isletme Kari";
    private const string NetWorthLabelText = "Net Deger";
    private const string TotalProfitLabelText = "Toplam Saniye Basi Kar";

    private RectTransform panelRect;
    private TextMeshProUGUI tapValueText;
    private TextMeshProUGUI businessValueText;
    private TextMeshProUGUI netWorthValueText;
    private TextMeshProUGUI totalValueText;

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    public void Initialize(TMP_FontAsset fontAsset, Material fontMaterial)
    {
        EnsurePanelRoot();
        BuildLayout(fontAsset, fontMaterial);
        UpdateValues();
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        UpdateValues();
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void EnsurePanelRoot()
    {
        panelRect = transform as RectTransform;
        if (panelRect == null)
        {
            panelRect = gameObject.AddComponent<RectTransform>();
        }

        Image backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0f, 0f, 0f, 0f);
            backgroundImage.raycastTarget = false;
        }

        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.preferredHeight = Mathf.Max(layoutElement.preferredHeight, 0f);
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void BuildLayout(TMP_FontAsset fontAsset, Material fontMaterial)
    {
        if (transform.Find("StatsRoot") != null)
        {
            CacheValueReferences();
            return;
        }

        GameObject statsRoot = new GameObject("StatsRoot", typeof(RectTransform), typeof(LayoutElement));
        statsRoot.transform.SetParent(transform, false);

        RectTransform statsRect = statsRoot.GetComponent<RectTransform>();
        statsRect.anchorMin = Vector2.zero;
        statsRect.anchorMax = Vector2.one;
        statsRect.offsetMin = new Vector2(32f, 32f);
        statsRect.offsetMax = new Vector2(-32f, -32f);

        LayoutElement statsLayout = statsRoot.GetComponent<LayoutElement>();
        statsLayout.ignoreLayout = true;

        CreateMetricBlock(
            statsRoot.transform,
            "TapProfitBlock",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, -12f),
            new Vector2(260f, 72f),
            TapLabelText,
            out tapValueText,
            fontAsset,
            fontMaterial,
            TextAlignmentOptions.TopLeft);

        CreateMetricBlock(
            statsRoot.transform,
            "BusinessProfitBlock",
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 12f),
            new Vector2(300f, 72f),
            BusinessLabelText,
            out businessValueText,
            fontAsset,
            fontMaterial,
            TextAlignmentOptions.BottomLeft);

        CreateMetricBlock(
            statsRoot.transform,
            "NetWorthBlock",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, -12f),
            new Vector2(260f, 72f),
            NetWorthLabelText,
            out netWorthValueText,
            fontAsset,
            fontMaterial,
            TextAlignmentOptions.TopRight);

        CreateMetricBlock(
            statsRoot.transform,
            "TotalProfitBlock",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(180f, -8f),
            new Vector2(360f, 88f),
            TotalProfitLabelText,
            out totalValueText,
            fontAsset,
            fontMaterial,
            TextAlignmentOptions.Center);
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    private void CacheValueReferences()
    {
        tapValueText = FindValueText("TapProfitBlock");
        businessValueText = FindValueText("BusinessProfitBlock");
        netWorthValueText = FindValueText("NetWorthBlock");
        totalValueText = FindValueText("TotalProfitBlock");
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    private TextMeshProUGUI FindValueText(string blockName)
    {
        Transform block = transform.Find($"StatsRoot/{blockName}/Value");
        return block != null ? block.GetComponent<TextMeshProUGUI>() : null;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void CreateMetricBlock(
        Transform parent,
        string blockName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        string labelText,
        out TextMeshProUGUI valueText,
        TMP_FontAsset fontAsset,
        Material fontMaterial,
        TextAlignmentOptions alignment)
    {
        GameObject blockObject = new GameObject(blockName, typeof(RectTransform));
        blockObject.transform.SetParent(parent, false);

        RectTransform blockRect = blockObject.GetComponent<RectTransform>();
        blockRect.anchorMin = anchorMin;
        blockRect.anchorMax = anchorMax;
        blockRect.pivot = pivot;
        blockRect.anchoredPosition = anchoredPosition;
        blockRect.sizeDelta = sizeDelta;

        CreateText(
            blockObject.transform,
            "Label",
            labelText,
            fontAsset,
            fontMaterial,
            18f,
            alignment,
            FontStyles.Bold,
            new Vector2(0f, 0.5f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f));

        valueText = CreateText(
            blockObject.transform,
            "Value",
            "$0",
            fontAsset,
            fontMaterial,
            blockName == "TotalProfitBlock" ? 34f : 24f,
            alignment,
            FontStyles.Bold,
            new Vector2(0f, 0f),
            new Vector2(1f, 0.52f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f));
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string text,
        TMP_FontAsset fontAsset,
        Material fontMaterial,
        float fontSize,
        TextAlignmentOptions alignment,
        FontStyles fontStyle,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.pivot = pivot;
        textRect.offsetMin = offsetMin;
        textRect.offsetMax = offsetMax;

        TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = alignment;
        tmpText.fontStyle = fontStyle;
        tmpText.color = Color.white;
        tmpText.textWrappingMode = TextWrappingModes.NoWrap;
        tmpText.overflowMode = TextOverflowModes.Ellipsis;
        tmpText.raycastTarget = false;

        if (fontAsset != null)
        {
            tmpText.font = fontAsset;
        }

        if (fontMaterial != null)
        {
            tmpText.fontSharedMaterial = fontMaterial;
        }

        return tmpText;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void UpdateValues()
    {
        double tapProfit = UpgradeManager.Instance != null ? UpgradeManager.Instance.CurrentClickValue : 0d;
        double businessProfitPerSecond = PassiveIncomeManager.Instance != null
            ? PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond()
            : 0d;
        double activeProfitPerSecond = CurrencyManager.Instance != null
            ? CurrencyManager.Instance.GetRecentActiveIncomePerSecond()
            : 0d;
        double netWorth = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentMoney : 0d;
        double totalProfitPerSecond = businessProfitPerSecond + activeProfitPerSecond;

        SetValue(tapValueText, tapProfit);
        SetValue(businessValueText, businessProfitPerSecond);
        SetValue(netWorthValueText, netWorth);
        SetValue(totalValueText, totalProfitPerSecond);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void SetValue(TextMeshProUGUI target, double value)
    {
        if (target == null)
        {
            return;
        }

        target.text = "$" + NumberFormatter.Format(value);
    }
}
