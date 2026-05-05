using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PassiveIncomeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI profitText;
    [SerializeField] private string profitPrefix = "Kar: ";
    [SerializeField] private string profitSuffix = " / saniye";
    [SerializeField] private string currencyPrefix = "$";

    private readonly List<TextMeshProUGUI> profitTexts = new List<TextMeshProUGUI>();

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        CacheProfitTexts();
        UpdateProfitText(0d);
    }

    // Obje aktif olunca calisir; event dinleyicileri veya gecici durumlar burada hazirlanir.
    private void OnEnable()
    {
        CacheProfitTexts();
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        double passiveIncomePerSecond = 0d;
        if (PassiveIncomeManager.Instance != null)
        {
            passiveIncomePerSecond = PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond();
        }

        UpdateProfitText(passiveIncomePerSecond);
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    private void CacheProfitTexts()
    {
        profitTexts.Clear();

        if (profitText != null)
        {
            profitTexts.Add(profitText);
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null || text.name != "ProfitText" || profitTexts.Contains(text))
            {
                continue;
            }

            profitTexts.Add(text);
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void UpdateProfitText(double amountPerSecond)
    {
        string formattedAmount = amountPerSecond <= 0d
            ? $"{currencyPrefix}0"
            : $"{currencyPrefix}{NumberFormatter.Format(amountPerSecond)}";
        string finalText = $"{profitPrefix}{formattedAmount}{profitSuffix}";

        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < profitTexts.Count; i++)
        {
            if (profitTexts[i] != null)
            {
                profitTexts[i].text = finalText;
            }
        }
    }
}
