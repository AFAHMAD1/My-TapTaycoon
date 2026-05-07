using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillsPanel : BaseUpgradePanel
{
    [Header("Kapatma Ayarlari")]
    public Button closeButton;

    protected override void Start()
    {
        base.Start();
        EnsureCloseButton();
    }

    private void EnsureCloseButton()
    {
        if (closeButton == null)
        {
            closeButton = UIHelper.FindButton(transform, "CloseButton", "KapatButonu");
        }

        if (closeButton == null)
        {
            GameObject buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(transform, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-15f, -15f);
            rect.sizeDelta = new Vector2(50f, 50f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = "X";
            text.fontSize = 30f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            closeButton = buttonObject.GetComponent<Button>();
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);

            if (closeButton.gameObject.GetComponent<UIBounce>() == null)
            {
                closeButton.gameObject.AddComponent<UIBounce>();
            }
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    protected override List<ICardDataProvider> GetDataProviders()
    {
        var providers = new List<ICardDataProvider>();

        if (SkillManager.Instance != null)
        {
            foreach (var skill in SkillManager.Instance.unlockedSkills)
            {
                providers.Add(new SkillCardAdapter(skill));
            }
        }

        return providers;
    }
}
