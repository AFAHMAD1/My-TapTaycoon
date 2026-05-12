using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Dinamik olarak binaları, geliştirmeleri veya mağaza ürünlerini gösteren kart bileşeni.
/// ICardDataProvider arayüzünü kullanan her türlü veriyi görselleştirebilir.
/// </summary>
public class UpgradeCard : MonoBehaviour
{
    // Arayüz renk paleti (Sabitler)
    private static readonly Color ProgressBackgroundColor = new Color32(28, 176, 12, 255);
    private static readonly Color ProgressFillColor = new Color32(247, 211, 45, 255);
    private static readonly Color ProgressTextColor = Color.white;

    [Header("Kart Referansları (UI Elemanları)")]
    public TextMeshProUGUI nameText; // Nesne adı
    public TextMeshProUGUI incomeText; // Kazanç bilgisi
    public TextMeshProUGUI levelText; // Mevcut seviye
    public TextMeshProUGUI costText; // Satın alma maliyeti
    public TextMeshProUGUI descriptionText; // Açıklama metni
    public Button buyButton; // Satın al butonu
    public TextMeshProUGUI buyButtonText; // Buton üzerindeki yazı
    public Image cardBackgroundImage; // Kartın arka planı
    public Image iconImage; // Nesne ikonu
    public Image buyButtonImage; // Butonun görseli
    public Transform priceRow;
    public Image priceIconImage;
    public TextMeshProUGUI leftActionText;
    public Slider progressSlider; // Üretim süresini gösteren bar
    public TextMeshProUGUI progressTimeText; // Süreyi yazılı gösteren metin

    private ICardDataProvider provider; // Kartın verisini sağlayan adaptör
    private System.Func<int> getBuyAmountFunc; // Kaçar kaçar satın alınacağını (1, 10, 100) dönen fonksiyon

    /// <summary>
    /// Kartı belirli bir veri kaynağı ile ilklendirir.
    /// </summary>
    public void Setup(ICardDataProvider dataProvider, System.Func<int> getBuyAmount)
    {
        provider = dataProvider;
        getBuyAmountFunc = getBuyAmount;
        
        // Görünürlüğü garantilemek için tüm alt bileşenleri aktif et
        EnableAllComponents();

        // Referanslar boşsa otomatik olarak bulmaya çalış
        AutoAssignReferencesIfNeeded();
        ConfigureIncomeText();
        EnsureDescriptionText();

        // Satın alma butonuna dinleyici ekle
        if (buyButton != null)
        {
            // Bu satir: 'onClick' objesi uzerindeki 'RemoveListener' metodunu cagirir; daha once eklenmis belirli bir UI olay baglantisini kaldirir.
            buyButton.onClick.RemoveListener(OnPrimaryBuyClicked);
            // Bu satir: 'onClick' objesi uzerindeki 'AddListener' metodunu cagirir; buton/toggle gibi UI olayina fonksiyon baglar; kullanici tiklayinca bu fonksiyon calisir.
            buyButton.onClick.AddListener(OnPrimaryBuyClicked);

            // Etkileşimli animasyon ekle (UIBounce)
            if (buyButton.gameObject.GetComponent<UIBounce>() == null)
                buyButton.gameObject.AddComponent<UIBounce>();
        }

        // Veri kaynağının konfigürasyonuna göre UI elemanlarını aç/kapat (Örn: Mağaza ürününde seviye gösterme)
        ApplyDisplayConfig();
        ForceUpdate(); // İlk güncellemeyi yap
    }

    // Kart üzerindeki tüm layout ve görsel bileşenleri zorla aktif yapar.
    private void EnableAllComponents()
    {
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp != null) comp.enabled = true;
        }

        if (TryGetComponent<Image>(out var img)) img.enabled = true;
        
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var layout in GetComponentsInChildren<LayoutGroup>(true)) layout.enabled = true;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var fitter in GetComponentsInChildren<ContentSizeFitter>(true)) fitter.enabled = true;
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (var graphic in GetComponentsInChildren<Graphic>(true)) graphic.enabled = true;
    }

    // Veri kaynağının isteğine göre (DisplayConfig) ikon, seviye, gelir gibi alanları gizler veya gösterir.
    private void ApplyDisplayConfig()
    {
        if (provider == null || provider.DisplayConfig == null) return;
        CardDisplayConfig config = provider.DisplayConfig;

        if (config.showProgressBar)
        {
            EnsureProgressBarVisuals();
            SetupProgressSliderClickHandler();
        }

        if (iconImage != null) iconImage.gameObject.SetActive(config.showIcon);
        if (progressSlider != null) progressSlider.gameObject.SetActive(config.showProgressBar);
        if (descriptionText != null) descriptionText.gameObject.SetActive(config.showDescription);
        if (incomeText != null) incomeText.gameObject.SetActive(config.showIncome);
        if (levelText != null) levelText.gameObject.SetActive(config.showLevel);
        
        EnsureSecondaryButton(config.showSecondaryButton);
    }

    private Button secondaryButton;
    private TextMeshProUGUI secondaryButtonText;

    // Eğer veri kaynağı "İkincil Buton" (Örn: Prestij butonu) istiyorsa onu oluşturur.
    private void EnsureSecondaryButton(bool show)
    {
        if (secondaryButton == null)
        {
            // Bu satir: 'UIHelper' uzerindeki 'FindChildRecursive' metodunu cagirir ve sonucu 'existing' degiskenine koyar; verilen parent'in alt cocuklarinda isme gore derin arama yapar. Normal Find sadece tek seviye bakarken bu metot alt seviyelere de iner.
            Transform existing = UIHelper.FindChildRecursive(transform, "SecondaryButton");
            if (existing != null)
            {
                secondaryButton = existing.GetComponent<Button>();
                secondaryButtonText = secondaryButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            else if (show)
            {
                // Mavi bir buton oluştur ve yerleştir
                GameObject btnObj = new GameObject("SecondaryButton", typeof(RectTransform), typeof(Image), typeof(Button));
                // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
                btnObj.transform.SetParent(transform, false);
                
                RectTransform rect = btnObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, -45f);
                rect.sizeDelta = new Vector2(180f, 45f);

                Image img = btnObj.GetComponent<Image>();
                if (buyButtonImage != null) img.sprite = buyButtonImage.sprite;
                img.type = Image.Type.Sliced;
                img.color = new Color32(33, 115, 206, 255);

                GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
                txtObj.transform.SetParent(btnObj.transform, false);
                RectTransform txtRect = txtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;

                secondaryButtonText = txtObj.GetComponent<TextMeshProUGUI>();
                secondaryButtonText.fontSize = 24f;
                secondaryButtonText.alignment = TextAlignmentOptions.Center;
                secondaryButtonText.text = "Prestij!";
                secondaryButtonText.color = Color.white;

                secondaryButton = btnObj.GetComponent<Button>();
            }
        }

        if (secondaryButton != null)
        {
            // Bu satir: 'gameObject' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
            secondaryButton.gameObject.SetActive(show);
            if (show)
            {
                // Bu satir: 'onClick' objesi uzerindeki 'RemoveAllListeners' metodunu cagirir; butondaki eski tiklama baglantilarini temizler; ayni is birden fazla kez calismasin diye kullanilir.
                secondaryButton.onClick.RemoveAllListeners();
                // Bu satir: 'onClick' objesi uzerindeki 'AddListener' metodunu cagirir; buton/toggle gibi UI olayina fonksiyon baglar; kullanici tiklayinca bu fonksiyon calisir.
                secondaryButton.onClick.AddListener(() => provider.OnSecondaryButtonClick());
                if (secondaryButtonText != null) secondaryButtonText.text = provider.GetSecondaryButtonText();
            }
        }
    }

    // Ana satın alma butonuna tıklandığında veri kaynağına haber verir.
    private void OnPrimaryBuyClicked()
    {
        if (provider != null && getBuyAmountFunc != null)
        {
            // Bu satir: 'provider' objesi uzerindeki 'Purchase' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
            provider.Purchase(getBuyAmountFunc());
        }
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        // Kart bilgilerini her karede (veya değişimde) güncelle
        ForceUpdate();
    }

    /// <summary>
    /// Kart üzerindeki tüm metinleri, fiyatları ve renkleri veri kaynağına göre günceller.
    /// </summary>
    private void ForceUpdate()
    {
        if (provider == null) return;

        CardDisplayConfig config = provider.DisplayConfig;
        int buyAmount = getBuyAmountFunc != null ? getBuyAmountFunc() : 1;

        // İsim Güncelleme
        if (nameText != null) nameText.text = $"<size=24><b>{provider.DisplayName}</b></size>";

        // Gelir Güncelleme
        if (config.showIncome && incomeText != null)
        {
            // Bu satir: 'provider' uzerindeki 'GetIncomePerCycle' metodunu cagirir ve donen sonucu 'incomePerCycle' degiskenine kaydeder.
            double incomePerCycle = provider.GetIncomePerCycle();
            incomeText.text =
                $"<size=18>Kazanç </size>" +
                $"<size=20><color=#{ColorUtility.ToHtmlStringRGB(provider.IncomeColor)}>${NumberFormatter.FormatIncome(incomePerCycle)} / Tur</color></size>";
        }

        // Açıklama Güncelleme
        if (config.showDescription && descriptionText != null) descriptionText.text = provider.Description;

        // Seviye Güncelleme
        if (config.showLevel && levelText != null)
        {
            levelText.text = provider.CurrentLevel == 0 ? "<size=22>Lv.0</size>" : $"<size=22>Lv.{provider.CurrentLevel}</size>";
        }

        // Fiyat Güncelleme
        if (costText != null)
        {
            // Bu satir: 'provider' uzerindeki 'GetCost' metodunu cagirir ve donen sonucu 'cost' degiskenine kaydeder.
            double cost = provider.GetCost(buyAmount);
            costText.text = GetCostText(cost);
        }

        // Progress Bar Güncelleme
        if (config.showProgressBar && provider.HasProgressBar()) UpdateProgressBar();

        // Buton Aktiflik Kontrolü (Para yetiyor mu?)
        if (buyButton != null) buyButton.interactable = provider.CanAfford(buyAmount);

        // Renk ve İkon Güncellemeleri
        if (cardBackgroundImage != null && provider.CardColor.a > 0f) cardBackgroundImage.color = provider.CardColor;
        if (buyButtonImage != null && provider.ButtonColor.a > 0f) buyButtonImage.color = provider.ButtonColor;
        if (config.showIcon && iconImage != null)
        {
            iconImage.sprite = provider.Icon;
            iconImage.color = provider.IconTintColor;
        }

        ApplyStoreVisuals();

        if (buyButtonText != null) buyButtonText.text = provider.GetBuyButtonText(buyAmount);
    }

    private string GetCostText(double cost)
    {
        if (provider is IStoreCardVisualProvider storeVisual)
        {
            return storeVisual.PriceText;
        }

        return "Fiyat: $" + NumberFormatter.FormatPrice(cost);
    }

    private void ApplyStoreVisuals()
    {
        if (!(provider is IStoreCardVisualProvider storeVisual)) return;

        if (priceRow != null) priceRow.gameObject.SetActive(storeVisual.ShowPriceRow);
        if (costText != null) costText.gameObject.SetActive(storeVisual.ShowPriceRow);

        if (priceIconImage != null)
        {
            bool showPriceIcon = storeVisual.ShowPriceRow && storeVisual.PriceIcon != null;
            priceIconImage.gameObject.SetActive(showPriceIcon);
            priceIconImage.sprite = storeVisual.PriceIcon;
        }

        if (leftActionText != null)
        {
            leftActionText.text = storeVisual.LeftActionText;
        }

        if (buyButton != null && storeVisual.ButtonSize != Vector2.zero)
        {
            RectTransform buttonRect = buyButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = storeVisual.ButtonSize;
            }
        }
    }

    // Üretim ilerleme çubuğunu günceller.
    private void UpdateProgressBar()
    {
        if (progressSlider == null || progressTimeText == null) return;
        progressSlider.value = provider.GetProgressNormalized();
        progressTimeText.text = provider.GetProgressText();
    }

    // Eğer prefab içinde açıklama metni yoksa dinamik olarak oluşturur.
    private void EnsureDescriptionText()
    {
        if (descriptionText != null) return;
        // Bu satir: 'UIHelper' uzerindeki 'FindText' metodunu cagirir ve sonucu 'descriptionText' degiskenine koyar; UI hiyerarsisinde verilen isimlerden birine sahip TextMeshPro yazisini arar.
        descriptionText = UIHelper.FindText(transform, "DescriptionText", "DescText");
        
        if (descriptionText == null && incomeText != null)
        {
            GameObject descObj = Instantiate(incomeText.gameObject, incomeText.transform.parent);
            descObj.name = "DescriptionText";
            descriptionText = descObj.GetComponent<TextMeshProUGUI>();
            descriptionText.text = "";
            descriptionText.alignment = TextAlignmentOptions.Left;
            descriptionText.fontSize = 20f;
        }
    }

    // Unity Inspector'dan atanmamış bileşenleri isimlerine göre bulup atar.
    private void AutoAssignReferencesIfNeeded()
    {
        nameText ??= UIHelper.FindText(transform, "AdYazisi", "NameText", "BuildingNameText");
        incomeText ??= UIHelper.FindText(transform, "GelirYazisi", "IncomeText");
        levelText ??= UIHelper.FindText(transform, "SeviyeYazisi", "LevelText");
        costText ??= UIHelper.FindText(transform, "FiyatYazisi", "PriceText");
        leftActionText ??= UIHelper.FindText(transform, "LeftActionText", "ActionLabel", "FollowText");
        progressTimeText ??= UIHelper.FindText(transform, "ProgressTimeText", "ProgressLabel");

        if (buyButton == null) buyButton = UIHelper.FindButton(transform, "Button", "ActionButton");
        if (buyButtonText == null && buyButton != null) buyButtonText = buyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (cardBackgroundImage == null) cardBackgroundImage = GetComponent<Image>();
        if (iconImage == null) iconImage = UIHelper.FindImage(transform, "Image", "BuildingIcon");
        if (priceIconImage == null) priceIconImage = UIHelper.FindImage(transform, "PriceIcon", "CostIcon");
        if (buyButtonImage == null && buyButton != null) buyButtonImage = buyButton.GetComponent<Image>();
        if (progressSlider == null) progressSlider = UIHelper.FindSlider(transform, "ProgressSlider");
        priceRow ??= UIHelper.FindChildRecursive(transform, "PriceRow");
    }

    private void ConfigureIncomeText()
    {
        if (incomeText == null) return;

        incomeText.enableAutoSizing = true;
        incomeText.fontSizeMin = 12f;
        incomeText.fontSizeMax = 20f;
        incomeText.enableWordWrapping = false;
        incomeText.overflowMode = TextOverflowModes.Overflow;
    }

    // Progress bar'a tıklanarak manuel toplama yapılmasına imkan sağlar.
    private void SetupProgressSliderClickHandler()
    {
        if (progressSlider == null) return;

        progressSlider.interactable = false;
        // Bu satir: 'transform' uzerindeki 'Find' metodunu cagirir ve sonucu 'overlayTransform' degiskenine koyar; sahnede veya transform altinda verilen isimde obje arar.
        Transform overlayTransform = progressSlider.transform.Find("ClickOverlay");
        Button clickButton;

        if (overlayTransform == null)
        {
            GameObject overlayObj = new GameObject("ClickOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
            overlayObj.transform.SetParent(progressSlider.transform, false);

            RectTransform rect = overlayObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image img = overlayObj.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // Tamamen şeffaf tıklama alanı
            img.raycastTarget = true;

            clickButton = overlayObj.GetComponent<Button>();
        }
        else
        {
            clickButton = overlayTransform.GetComponent<Button>();
        }

        if (clickButton != null)
        {
            // Bu satir: 'onClick' objesi uzerindeki 'RemoveAllListeners' metodunu cagirir; butondaki eski tiklama baglantilarini temizler; ayni is birden fazla kez calismasin diye kullanilir.
            clickButton.onClick.RemoveAllListeners();
            // Bu satir: 'onClick' objesi uzerindeki 'AddListener' metodunu cagirir; buton/toggle gibi UI olayina fonksiyon baglar; kullanici tiklayinca bu fonksiyon calisir.
            clickButton.onClick.AddListener(() => provider.OnProgressClick());
        }
    }

    // Progress bar'ın görsel stilini (renkler, fontlar) kod ile düzenler.
    private void EnsureProgressBarVisuals()
    {
        if (progressSlider == null) progressSlider = CreateProgressSlider();
        if (progressSlider == null) return;

        progressSlider.direction = Slider.Direction.LeftToRight;
        progressSlider.transition = Selectable.Transition.None;

        // Bu satir: 'UIHelper' uzerindeki 'FindImage' metodunu cagirir ve sonucu 'backgroundImage' degiskenine koyar; UI hiyerarsisinde verilen isimlerden birine sahip Image component'ini arar.
        Image backgroundImage = UIHelper.FindImage(progressSlider.transform, "Background") ?? CreateProgressBackground(progressSlider.transform);
        backgroundImage.color = ProgressBackgroundColor;

        // Bu satir: 'UIHelper' uzerindeki 'FindImage' metodunu cagirir ve sonucu 'fillImage' degiskenine koyar; UI hiyerarsisinde verilen isimlerden birine sahip Image component'ini arar.
        Image fillImage = UIHelper.FindImage(progressSlider.transform, "Fill") ?? CreateProgressFill(progressSlider.transform);
        fillImage.color = ProgressFillColor;
        progressSlider.fillRect = fillImage.rectTransform;

        if (progressTimeText == null) progressTimeText = CreateProgressLabel(progressSlider.transform);
        if (progressTimeText != null) progressTimeText.color = ProgressTextColor;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private Slider CreateProgressSlider()
    {
        GameObject sliderObject = new GameObject("ProgressSlider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        sliderObject.transform.SetParent(transform, false);

        Image rootImage = sliderObject.GetComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0f);
        rootImage.raycastTarget = true;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        return slider;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private Image CreateProgressBackground(Transform parent)
    {
        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        backgroundObject.transform.SetParent(parent, false);

        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = backgroundObject.GetComponent<Image>();
        image.type = Image.Type.Sliced;
        return image;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private Image CreateProgressFill(Transform parent)
    {
        GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        fillAreaObject.transform.SetParent(parent, false);

        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(3f, 3f);
        fillAreaRect.offsetMax = new Vector2(-3f, -3f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        fillObject.transform.SetParent(fillAreaObject.transform, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image image = fillObject.GetComponent<Image>();
        image.type = Image.Type.Sliced;
        return image;
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private TextMeshProUGUI CreateProgressLabel(Transform parent)
    {
        GameObject labelObject = new GameObject("ProgressTimeText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.fontSize = 26f;
        label.text = "00:00:00";
        label.alignment = TextAlignmentOptions.Center;
        return label;
    }
}
