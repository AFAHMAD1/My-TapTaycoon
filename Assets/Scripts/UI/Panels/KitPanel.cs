using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KitPanel : BaseUpgradePanel
{
    [Header("Magaza Icerikleri")]
    public List<StoreItemData> storeItems = new List<StoreItemData>();

    [Header("Kit Kart Yerlesimi")]
    public bool preservePrefabCardLayout = true;
    public float fallbackCardHeight = 160f;

    [Header("Kit Ozet Cubugu")]
    public TextMeshProUGUI progressText; // Alınan Kitler: 2/5 yazacak text

    // Unity bu fonksiyonu oyun baslarken calistirir; burada baslangic kurulumu yapilir.
    protected override void Start()
    {
        // Önce BaseUpgradePanel'in Start'ını çalıştır (Kartları oluştursun)
        // Bu satir: base, yani miras alinan ust sinif uzerindeki 'Start' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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
                // Bu satir: 'providers' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
                providers.Add(new StoreCardAdapter(item));
            }
        }

        return providers;
    }
    protected override void ConfigureContentLayout()
    {
        base.ConfigureContentLayout();

        VerticalLayoutGroup layoutGroup = contentContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null) return;

        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandHeight = false;
    }

    protected override void ConfigureCardLayout(GameObject cardObj)
    {
        if (!preservePrefabCardLayout)
        {
            base.ConfigureCardLayout(cardObj);
            return;
        }

        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = Vector2.zero;
            float height = rectTransform.sizeDelta.y > 0f ? rectTransform.sizeDelta.y : fallbackCardHeight;
            rectTransform.sizeDelta = new Vector2(0f, height);
        }

        LayoutElement layoutElement = cardObj.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.enabled = true;
            layoutElement.flexibleHeight = 0f;
        }

        if (cardObj.TryGetComponent<UnityEngine.UI.Image>(out var img))
        {
            img.enabled = true;
        }
    }
}

