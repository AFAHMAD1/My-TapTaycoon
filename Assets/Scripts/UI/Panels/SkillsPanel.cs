using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillsPanel : BaseUpgradePanel
{
    [Header("Kapatma Ayarları")]
    public Button closeButton;

    protected override void Start()
    {
        showBuyModeSelector = false;
        cardHeight = 190f;
        cardSpacing = 18f;
        topPadding = 24;
        sidePadding = 24;
        base.Start();
        ConfigureScrollSpeed();
        EnsureCloseButton();
    }

    private void ConfigureScrollSpeed()
    {
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>(true);
        if (scrollRect == null)
        {
            return;
        }

        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 35f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.18f;
        scrollRect.elasticity = 0.08f;
    }

    /// <summary>
    /// Eğer buton atanmamışsa sahneden bulur veya çalışma anında sıfırdan oluşturur.
    /// </summary>
    private void EnsureCloseButton()
    {
        // 1. Önce isminden bulmaya çalış
        if (closeButton == null)
        {
            closeButton = UIHelper.FindButton(transform, "CloseButton", "KapatButonu");
        }
        
        // 2. Hala yoksa, SIFIRDAN OLUŞTUR (Red "X" Button)
        if (closeButton == null)
        {
            GameObject btnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(transform, false);

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-15f, -15f); // Sağ üst köşeden 15px içerde
            rect.sizeDelta = new Vector2(50f, 50f);

            // Kırmızı Arkaplan
            Image img = btnObj.GetComponent<Image>();
            img.color = new Color(0.8f, 0.2f, 0.2f, 1f); 

            // "X" Yazısı
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();
            txt.text = "X";
            txt.fontSize = 30f;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            txt.raycastTarget = false;

            closeButton = btnObj.GetComponent<Button>();
        }

        // 3. Click olayını bağla ve efekt ekle
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
            
            // Tıklama zıplama efekti
            if (closeButton.gameObject.GetComponent<UIBounce>() == null)
                closeButton.gameObject.AddComponent<UIBounce>();
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
