using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LeagueTeamsPanelPopulator : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private int cardCount = 10;
    [SerializeField] private int columns = 1;
    [SerializeField] private Vector2 spacing = new Vector2(8f, 8f);

    private Transform templateCard;

    private void OnEnable()
    {
        Populate();
    }

    public void Populate()
    {
        ResolveReferences();

        if (scrollRect == null || content == null || content.childCount == 0)
        {
            return;
        }

        NormalizeScrollView();
        EnsureCardCount();
        BindCards();
        StartCoroutine(RebuildAfterLayoutPass());
    }

    public void ClearCards()
    {
        ResolveReferences();

        if (content == null)
        {
            return;
        }

        CacheTemplateCard();

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Transform child = content.GetChild(i);
            if (child == templateCard)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            Destroy(child.gameObject);
        }

        content.anchoredPosition = Vector2.zero;
    }

    private void ResolveReferences()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        if (content == null && scrollRect != null)
        {
            content = scrollRect.content;
        }
    }

    private void NormalizeScrollView()
    {
        RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        StretchToParent(scrollRectTransform);

        if (scrollRect.viewport != null)
        {
            StretchToParent(scrollRect.viewport);
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.content = content;
        if (scrollRect.viewport != null)
        {
            scrollRect.viewport.pivot = new Vector2(0f, 1f);
        }

        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = content.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, columns);

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void EnsureCardCount()
    {
        int safeCardCount = LeagueManager.Instance != null
            ? LeagueManager.Instance.GetTable().Count
            : Mathf.Max(1, cardCount);

        CacheTemplateCard();

        if (templateCard == null)
        {
            return;
        }

        templateCard.gameObject.SetActive(true);
        templateCard.SetAsFirstSibling();
        templateCard.name = "TeamCard_1";

        while (content.childCount < safeCardCount)
        {
            GameObject clone = Instantiate(templateCard.gameObject, content);
            clone.name = $"TeamCard_{content.childCount}";
            clone.SetActive(true);
        }

        for (int i = content.childCount - 1; i >= safeCardCount; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void BindCards()
    {
        if (LeagueManager.Instance == null)
        {
            return;
        }

        IReadOnlyList<LeagueTeam> table = LeagueManager.Instance.GetTable();
        int bindCount = Mathf.Min(table.Count, content.childCount);

        for (int i = 0; i < bindCount; i++)
        {
            Transform card = content.GetChild(i);
            LeagueTeam team = table[i];
            card.name = $"{team.LeaguePosition}_{team.Name}";

            SetText(card, "Number", team.LeaguePosition.ToString());
            SetText(card, "MatchPlayed", $"MP: {team.PlayedMatches}");
            SetText(card, "MatchWin", $"MW: {team.Wins}");
            SetText(card, "Draw", $"Dr: {team.Draws}");
            SetText(card, "MatchLost", $"ML: {team.Losses}");
            SetText(card, "GoalsFor", $"GF: {team.GoalsFor}");
            SetText(card, "GoalsAgainst", $"GA: {team.GoalsAgainst}");
            SetText(card, "GoalDifference", $"GD: {team.GoalDifference}");
            SetText(card, "Power", $"Power: {team.Power:0}");
            SetText(card, "TeamPower", $"Power: {team.Power:0}");
            SetText(card, "Points", $"Points: {team.Points}");
            SetTeamName(card, team.Name);
        }
    }

    private void CacheTemplateCard()
    {
        if (templateCard != null)
        {
            return;
        }

        if (content != null && content.childCount > 0)
        {
            templateCard = content.GetChild(0);
        }
    }

    private IEnumerator RebuildAfterLayoutPass()
    {
        yield return null;
        RecalculateCellWidth();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (scrollRect.viewport != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.viewport);
        }
    }

    private void RecalculateCellWidth()
    {
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null || scrollRect.viewport == null)
        {
            return;
        }

        float viewportWidth = scrollRect.viewport.rect.width;
        if (viewportWidth <= 0f)
        {
            return;
        }

        int safeColumns = Mathf.Max(1, columns);
        float horizontalPadding = grid.padding.left + grid.padding.right;
        float totalSpacing = spacing.x * (safeColumns - 1);
        float cellWidth = Mathf.Floor((viewportWidth - horizontalPadding - totalSpacing) / safeColumns);
        if (cellWidth > 0f)
        {
            grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
        }
    }

    private static void SetText(Transform root, string childName, string value)
    {
        Transform child = UIHelper.FindChildRecursive(root, childName);
        if (child == null)
        {
            return;
        }

        TMP_Text text = child.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void SetTeamName(Transform card, string teamName)
    {
        Transform existing = UIHelper.FindChildRecursive(card, "TeamName");
        TextMeshProUGUI text = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;

        if (text == null)
        {
            GameObject textObject = new GameObject("TeamName", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(card, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(160f, 0f);
            rect.sizeDelta = new Vector2(250f, 80f);

            text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = 28f;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }

        text.text = teamName;
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
