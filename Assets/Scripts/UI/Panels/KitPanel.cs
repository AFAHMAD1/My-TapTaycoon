using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KitPanel : BaseUpgradePanel
{
    [Header("Magaza Icerikleri")]
    public List<StoreItemData> storeItems = new List<StoreItemData>();

    [Header("Kit Ozet Cubugu")]
    public TextMeshProUGUI progressText; // Alınan Kitler: 2/5 yazacak text

    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    protected override void Start()
    {
        // Önce BaseUpgradePanel'in Start'ını çalıştır (Kartları oluştursun)
        base.Start();

        // Sonra bizim yazımızı güncelle
        UpdateProgressText();
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void UpdateProgressText()
    {
        if (progressText == null) return;

        int totalKits = storeItems.Count;
        int purchasedKits = 0;

        // NOT: İleride StoreItemData içine "Alındı mı?" (isPurchased) verisi
        // veya SaveManager'a Kit kayıt sistemi eklediğimizde burayı ona göre saydıracağız.
        // Şimdilik test için 0 / Toplam şeklinde gösteriyoruz.

        progressText.text = $"Alınan Kitler: {purchasedKits} / {totalKits}";
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    protected override List<ICardDataProvider> GetDataProviders()
    {
        var providers = new List<ICardDataProvider>();

        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var item in storeItems)
        {
            if (item != null)
            {
                providers.Add(new StoreCardAdapter(item));
            }
        }

        return providers;
    }
}

