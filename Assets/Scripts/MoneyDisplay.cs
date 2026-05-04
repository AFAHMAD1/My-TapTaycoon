using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI profitText;
    [SerializeField] private string profitPrefix = "Kar: ";
    [SerializeField] private string profitSuffix = " / saniye";
    [SerializeField] private string currencyPrefix = "$";

    private readonly List<TextMeshProUGUI> profitTexts = new List<TextMeshProUGUI>();

    private void Awake()
    {
        CacheProfitTexts();
        UpdateProfitText(0d);
    }

    private void OnEnable()
    {
        CacheProfitTexts();
    }

    private void Update()
    {
        double passiveIncomePerSecond = 0d;
        if (PassiveIncomeManager.Instance != null)
        {
            passiveIncomePerSecond = PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond();
        }

        UpdateProfitText(passiveIncomePerSecond);
    }

    private void CacheProfitTexts()
    {
        profitTexts.Clear();

        if (profitText != null)
        {
            profitTexts.Add(profitText);
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null || text.name != "ProfitText" || profitTexts.Contains(text))
            {
                continue;
            }

            profitTexts.Add(text);
        }
    }

    private void UpdateProfitText(double amountPerSecond)
    {
        string formattedAmount = amountPerSecond <= 0d
            ? $"{currencyPrefix}0"
            : $"{currencyPrefix}{NumberFormatter.Format(amountPerSecond)}";
        string finalText = $"{profitPrefix}{formattedAmount}{profitSuffix}";

        for (int i = 0; i < profitTexts.Count; i++)
        {
            if (profitTexts[i] != null)
            {
                profitTexts[i].text = finalText;
            }
        }
    }
}
