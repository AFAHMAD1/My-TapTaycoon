using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CompanyCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button investButton;

    private CompanyInvestmentData company;
    private InvestmentManager investmentManager;
    private Action<int> investRequested;

    public void Bind(CompanyInvestmentData companyData, InvestmentManager manager, Action<int> onInvestRequested)
    {
        company = companyData;
        investmentManager = manager;
        investRequested = onInvestRequested;

        AutoAssignReferences();

        if (investmentManager != null)
        {
            investmentManager.CompanyChanged -= OnCompanyChanged;
            investmentManager.CompanyChanged += OnCompanyChanged;
        }

        if (investButton != null)
        {
            investButton.onClick.RemoveAllListeners();
            investButton.onClick.AddListener(OnInvestButtonClicked);
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (investmentManager != null)
        {
            investmentManager.CompanyChanged -= OnCompanyChanged;
        }
    }

    private void OnCompanyChanged(CompanyInvestmentData changedCompany)
    {
        if (company != null && changedCompany != null && changedCompany.companyId == company.companyId)
        {
            Refresh();
        }
    }

    private void OnInvestButtonClicked()
    {
        if (company == null)
        {
            return;
        }

        if (investRequested != null)
        {
            investRequested.Invoke(company.companyId);
        }
        else
        {
            investmentManager?.StartInvestment(company.companyId);
        }
    }

    private void Refresh()
    {
        if (company == null)
        {
            return;
        }

        string chancePercent = Mathf.RoundToInt(company.successChance * 100f).ToString();
        string possibleReward = NumberFormatter.FormatPrice(company.PossibleReward);

        if (detailsText != null)
        {
            detailsText.text =
                $"{company.companyName}\n" +
                $"Olcek: {company.SizeText} | {company.RiskText}\n" +
                $"Maliyet: ${NumberFormatter.FormatPrice(company.investmentCost)} | Sans: {chancePercent}%\n" +
                $"Olası kar: ${possibleReward}";
        }

        if (statusText != null)
        {
            statusText.text = $"{company.timerText}\n{company.statusText}";
        }

        if (buttonText != null)
        {
            buttonText.text = company.isInvestmentActive ? "Bekleniyor..." : "Yatirim Yap";
        }

        if (investButton != null)
        {
            investButton.interactable = !company.isInvestmentActive;
        }
    }

    private void AutoAssignReferences()
    {
        detailsText ??= FindText("IncomeText");
        statusText ??= FindText("PriceText");
        investButton ??= FindButton();
        if (buttonText == null && investButton != null)
        {
            buttonText = investButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private TMP_Text FindText(string objectName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.name.Equals(objectName, StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }
        }

        return null;
    }

    private Button FindButton()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.gameObject.name.Equals("ActionButton", StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return buttons.Length > 0 ? buttons[0] : null;
    }
}
