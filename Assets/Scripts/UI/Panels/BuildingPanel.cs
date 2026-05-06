using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingPanel : BaseUpgradePanel
{
    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    protected override void Start()
    {
        // Bu satir: base, yani miras alinan ust sinif uzerindeki 'Start' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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
                // Bu satir: 'providers' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
                providers.Add(new BuildingCardAdapter(PassiveIncomeManager.Instance.buildings[i], i));
            }
        }
        
        return providers;
    }

    // SummaryStatsPanel (zemin istatistikleri) Binalar Paneli logic'inde idi, burada bırakıyoruz.
    private void SetupGroundStatsPanel()
    {
        // Bu satir: 'UIHelper' uzerindeki 'FindInActiveScene' metodunu cagirir ve sonucu 'groundTransform' degiskenine koyar; aktif sahnede verilen isimdeki objeyi arar. Bulursa Transform'unu dondurur, bulamazsa null dondurur.
        Transform groundTransform = UIHelper.FindInActiveScene("Ground");
        if (groundTransform == null) return;

        TMP_FontAsset fontAsset = null;
        Material fontMaterial = null;

        if (buyModeButtonContext_Label != null)
        {
            fontAsset = buyModeButtonContext_Label.font;
            fontMaterial = buyModeButtonContext_Label.fontSharedMaterial;
        }

        // Bu satir: 'UIHelper' uzerindeki 'FindChildRecursive' metodunu cagirir ve sonucu 'panelTransform' degiskenine koyar; verilen parent'in alt cocuklarinda isme gore derin arama yapar. Normal Find sadece tek seviye bakarken bu metot alt seviyelere de iner.
        Transform panelTransform = UIHelper.FindChildRecursive(groundTransform, "GroundStatsPanel");
        if (panelTransform == null)
        {
            GameObject panelObject = new GameObject("GroundStatsPanel", typeof(RectTransform));
            // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
            panelObject.transform.SetParent(groundTransform, false);
            panelTransform = panelObject.transform;
            
            RectTransform panelRect = panelTransform as RectTransform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            // Bu satir: 'panelRect' objesi uzerindeki 'SetAsLastSibling' metodunu cagirir; objeyi parent icinde en sona alir; UI'da genelde en onde gorunmesine yardim eder.
            panelRect.SetAsLastSibling();
        }

        SummaryStatsPanel panel = panelTransform.GetComponent<SummaryStatsPanel>();
        if (panel == null)
        {
            panel = panelTransform.gameObject.AddComponent<SummaryStatsPanel>();
        }

        // Bu satir: SummaryStatsPanel'i baslatir; fontAsset ve fontMaterial verilerek paneldeki yazilarin ayni font stilini kullanmasi saglanir.
        panel.Initialize(fontAsset, fontMaterial);
    }
}
