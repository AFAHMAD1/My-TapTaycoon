using System.Collections.Generic;
using UnityEngine;

public class PlayerPanel : BaseUpgradePanel
{
    // Obje aktif olunca calisir; event dinleyicileri veya gecici durumlar burada hazirlanir.
    private void OnEnable()
    {
        PurchaseService.OnAnyPurchaseCompleted += RefreshPanel;
    }

    // Obje pasif olunca calisir; acik kalan event/durumlar burada temizlenir.
    private void OnDisable()
    {
        PurchaseService.OnAnyPurchaseCompleted -= RefreshPanel;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void RefreshPanel()
    {
        if (gameObject.activeInHierarchy)
        {
            CreateCards();
        }
    }

    // GetDataProviders yerine direkt CreateCards'ı eziyoruz ki araya görsel (Header) ekleyebilelim.
    protected override void CreateCards()
    {
        ClearContainer();
        
        if (UpgradeManager.Instance == null) return;

        // 1. Collector (Money Master) Kartı
        if (UpgradeManager.Instance.collectorUpgrade != null &&
            UpgradeManager.Instance.collectorUpgrade.data != null)
        {
            CreateSingleCard(new CollectorCardAdapter(UpgradeManager.Instance.collectorUpgrade));
        }

        // 2. Araya "Kilidi Açılabilenler" Header'ını Ekle
        if (UpgradeManager.Instance.boostUpgrades != null)
        {
            int totalBoosts = UpgradeManager.Instance.boostUpgrades.Count;
            int maxedBoosts = 0;

            // Kaç tanesinin max level (açılmış/tamamlanmış) olduğunu sayalım
            foreach (var boost in UpgradeManager.Instance.boostUpgrades)
            {
                if (boost != null && boost.IsMaxLevel) maxedBoosts++;
            }

            // BaseUpgradePanel içinde hazır bulunan CreateSectionHeader metodunu çağırıyoruz
            GameObject header = CreateSectionHeader($"Kilidi Açılabilenler     ({maxedBoosts}/{totalBoosts})");
            
            // Eğer istersen header objesinin içindeki Text'in rengini vb. kodla değiştirebilirsin
            // TextMeshProUGUI txt = header.GetComponentInChildren<TextMeshProUGUI>();
            // txt.color = new Color(0.9f, 0.9f, 0.9f);

            // 3. Boost Kartlarını Ekle
            foreach (var boost in UpgradeManager.Instance.boostUpgrades)
            {
                if (boost == null || boost.data == null || boost.IsMaxLevel) continue;
                CreateSingleCard(new BoostCardAdapter(boost));
            }
        }
    }

    // BaseUpgradePanel abstract bir sınıf olduğu için bu metodu zorunlu kılıyor.
    // Biz yukarıda CreateCards'ı ezdiğimiz için bu metodun içi boş dönebilir, hata vermemesi için ekledik.
    protected override List<ICardDataProvider> GetDataProviders()
    {
        return null;
    }
}
