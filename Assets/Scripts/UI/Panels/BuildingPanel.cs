using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingPanel : BaseUpgradePanel
{
    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    protected override void Start()
    {
        base.Start();
        SetupGroundStatsPanel();
    }

    // Bina panelinde gosterilecek kart verilerini PassiveIncomeManager'daki buildings listesinden hazirlar.
    protected override List<ICardDataProvider> GetDataProviders()
    {
        var providers = new List<ICardDataProvider>();
        
        if (PassiveIncomeManager.Instance != null)
        {
            // Bu dongu buildings listesindeki her binayi kart sisteminin anlayacagi BuildingCardAdapter'a cevirir.
            for (int i = 0; i < PassiveIncomeManager.Instance.buildings.Count; i++)
            {
                providers.Add(new BuildingCardAdapter(PassiveIncomeManager.Instance.buildings[i], i));
            }
        }
        
        return providers;
    }

    // SummaryStatsPanel (zemin istatistikleri) Binalar Paneli logic'inde idi, burada bırakıyoruz.
    private void SetupGroundStatsPanel()
    {
        Transform groundTransform = UIHelper.FindInActiveScene("Ground");
        if (groundTransform == null) return;

        TMP_FontAsset fontAsset = null;
        Material fontMaterial = null;

        if (buyModeButtonContext_Label != null)
        {
            fontAsset = buyModeButtonContext_Label.font;
            fontMaterial = buyModeButtonContext_Label.fontSharedMaterial;
        }

        Transform panelTransform = UIHelper.FindChildRecursive(groundTransform, "GroundStatsPanel");
        if (panelTransform == null)
        {
            GameObject panelObject = new GameObject("GroundStatsPanel", typeof(RectTransform));
            panelObject.transform.SetParent(groundTransform, false);
            panelTransform = panelObject.transform;
            
            RectTransform panelRect = panelTransform as RectTransform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.SetAsLastSibling();
        }

        SummaryStatsPanel panel = panelTransform.GetComponent<SummaryStatsPanel>();
        if (panel == null)
        {
            panel = panelTransform.gameObject.AddComponent<SummaryStatsPanel>();
        }

        panel.Initialize(fontAsset, fontMaterial);
    }
}
