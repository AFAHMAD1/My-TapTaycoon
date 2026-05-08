using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsPanelView : MonoBehaviour
{
    private const string GeneratedRootName = "GeneratedStatisticsLayout";

    private static readonly (string label, string value)[] BonusRows =
    {
        ("Prestij Bonusu:", "x1"),
        ("Ülke Bonusu:", "x1"),
        ("Görev / Meydan Okuma Bonusu:", "x1")
    };

    private static readonly (string label, string value)[] StatRows =
    {
        ("Oynama Süresi:", "00:00:00"),
        ("Toplam Dokunma Sayısı:", "0"),
        ("Prestij Sayısı:", "0"),
        ("En Yüksek Net Varlık:", "$0"),
        ("En Yüksek İşletme PPS’i:", "$0/sn"),
        ("Toplam Kâr:", "$0"),
        ("Gönderilen Toplam Ordu:", "0"),
        ("Alınan Toplam Roket Hediyesi:", "0"),
        ("Toplam Süper Güçlendirilmiş İşletme Sayısı:", "0"),
        ("Toplam Hızlı Para Sayısı:", "0"),
        ("Toplam Otomatik Dokunma Sayısı:", "0"),
        ("Toplam Midas Eli Sayısı:", "0"),
        ("Toplam Para Yığını Şansı Sayısı:", "0"),
        ("Alınan Toplam Para Yığınları:", "0"),
        ("Toplam Dokunma Yükseltmeleri:", "0"),
        ("Toplam İşletme Yükseltmeleri:", "0"),
        ("Toplam İşletme Geliştirmeleri:", "0"),
        ("Toplam Açılabilir Öğeler:", "0"),
        ("Kurulumdan Sonra Geçen Gün:", "0 gün")
    };

    private TMP_FontAsset cachedFont;
    private Material cachedFontMaterial;

    private void Start()
    {
        Build();
    }

    [ContextMenu("Rebuild Statistics Panel")]
    public void Build()
    {
        CaptureFontReferences();
        ClearGeneratedLayout();

        RectTransform root = CreateRect("GeneratedStatisticsLayout", transform);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = new Vector2(34f, 34f);
        root.offsetMax = new Vector2(-34f, -34f);

        VerticalLayoutGroup rootLayout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(18, 18, 18, 18);
        rootLayout.spacing = 14f;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        AddHeader(root);
        AddScrollContent(root);
    }

    private void AddHeader(Transform parent)
    {
        TextMeshProUGUI title = CreateText("Header", parent, "Harika İstatistiklerim", 42f, FontStyles.Bold, TextAlignmentOptions.Center);
        title.color = new Color32(56, 45, 38, 255);
        title.enableAutoSizing = true;
        title.fontSizeMin = 24f;
        title.fontSizeMax = 42f;

        LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 58f;
        titleLayout.flexibleHeight = 0f;
    }

    private void AddScrollContent(Transform parent)
    {
        GameObject scrollObject = new GameObject("StatisticsScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);

        Image scrollImage = scrollObject.GetComponent<Image>();
        scrollImage.color = new Color(1f, 1f, 1f, 0.06f);

        LayoutElement scrollLayout = scrollObject.AddComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1f;
        scrollLayout.minHeight = 680f;

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;

        RectTransform viewport = CreateRect("Viewport", scrollObject.transform);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = CreateRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;

        VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(16, 16, 16, 16);
        contentLayout.spacing = 10f;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 35f;
        scrollRect.decelerationRate = 0.18f;

        AddSection(content, "Bonuslar");
        foreach ((string label, string value) in BonusRows)
        {
            AddStatRow(content, label, value);
        }

        AddSection(content, "İstatistikler");
        foreach ((string label, string value) in StatRows)
        {
            AddStatRow(content, label, value);
        }
    }

    private void AddSection(Transform parent, string text)
    {
        TextMeshProUGUI section = CreateText($"Section_{text}", parent, text, 30f, FontStyles.Bold, TextAlignmentOptions.Left);
        section.color = new Color32(88, 60, 43, 255);

        LayoutElement layout = section.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 46f;
    }

    private void AddStatRow(Transform parent, string label, string value)
    {
        RectTransform row = CreateRect($"Row_{label}", parent);
        row.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 8, 8);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 48f;

        TextMeshProUGUI labelText = CreateText("Label", row, label, 22f, FontStyles.Normal, TextAlignmentOptions.Left);
        labelText.color = new Color32(61, 50, 44, 255);
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 13f;
        labelText.fontSizeMax = 22f;
        LayoutElement labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        labelLayout.minWidth = 420f;

        TextMeshProUGUI valueText = CreateText("Value", row, value, 22f, FontStyles.Bold, TextAlignmentOptions.Right);
        valueText.color = new Color32(38, 117, 83, 255);
        valueText.enableAutoSizing = true;
        valueText.fontSizeMin = 14f;
        valueText.fontSizeMax = 22f;
        LayoutElement valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 190f;
    }

    private TextMeshProUGUI CreateText(string objectName, Transform parent, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;

        if (cachedFont != null)
        {
            tmp.font = cachedFont;
            tmp.fontSharedMaterial = cachedFontMaterial;
        }

        return tmp;
    }

    private RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<RectTransform>();
    }

    private void CaptureFontReferences()
    {
        if (cachedFont != null)
        {
            return;
        }

        TextMeshProUGUI existingText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (existingText != null)
        {
            cachedFont = existingText.font;
            cachedFontMaterial = existingText.fontSharedMaterial;
        }
    }

    private void ClearGeneratedLayout()
    {
        List<GameObject> childrenToDelete = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == GeneratedRootName)
            {
                childrenToDelete.Add(child.gameObject);
            }
        }

        foreach (GameObject child in childrenToDelete)
        {
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }
}
