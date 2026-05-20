using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InvestmentManager : MonoBehaviour
{
    private const float MinResultPercent = 0.001f;
    private const float MaxResultPercent = 400f;

    public event Action<CompanyInvestmentData> CompanyChanged;
    public event Action<InvestmentResult> InvestmentResolved;

    [SerializeField] private List<CompanyInvestmentData> companies = new List<CompanyInvestmentData>();

    public IReadOnlyList<CompanyInvestmentData> Companies => companies;

    private void Awake()
    {
        EnsureDefaultCompanies();
    }

    private void Update()
    {
        TickActiveInvestments(Time.deltaTime);
    }

    public void EnsureDefaultCompanies()
    {
        if (companies != null && companies.Count > 0)
        {
            return;
        }

        companies = new List<CompanyInvestmentData>
        {
            CreateCompany(1, "TechNova Solutions", CompanySize.Small),
            CreateCompany(2, "GreenLeaf Foods", CompanySize.Small),
            CreateCompany(3, "BluePeak Marketing", CompanySize.Small),
            CreateCompany(4, "BrightPath Education", CompanySize.Small),
            CreateCompany(5, "UrbanFix Services", CompanySize.Small),
            CreateCompany(6, "CloudCore IT", CompanySize.Small),
            CreateCompany(7, "FreshBox Delivery", CompanySize.Small),
            CreateCompany(8, "DesignHub Studio", CompanySize.Small),
            CreateCompany(9, "SafeHome Security", CompanySize.Small),
            CreateCompany(10, "QuickPrint Center", CompanySize.Small),
            CreateCompany(11, "Microsoft", CompanySize.Large),
            CreateCompany(12, "Apple", CompanySize.Large),
            CreateCompany(13, "Google", CompanySize.Large),
            CreateCompany(14, "Amazon", CompanySize.Large),
            CreateCompany(15, "Samsung", CompanySize.Large),
            CreateCompany(16, "Toyota", CompanySize.Large),
            CreateCompany(17, "Volkswagen", CompanySize.Large),
            CreateCompany(18, "Coca-Cola", CompanySize.Large),
            CreateCompany(19, "Nestle", CompanySize.Large),
            CreateCompany(20, "IBM", CompanySize.Large),
        };
    }

    public CompanyInvestmentData GetCompany(int companyId)
    {
        EnsureDefaultCompanies();
        return companies.Find(company => company.companyId == companyId);
    }

    public bool StartInvestment(int companyId)
    {
        CompanyInvestmentData company = GetCompany(companyId);
        return company != null && StartInvestment(companyId, company.investmentCost);
    }

    public bool StartInvestment(int companyId, double requestedAmount)
    {
        CompanyInvestmentData company = GetCompany(companyId);
        if (company == null)
        {
            Debug.LogWarning($"[InvestmentManager] Company id {companyId} could not be found.");
            return false;
        }

        if (company.isInvestmentActive)
        {
            SetStatus(company, "Yatirim aktif", FormatRemainingTime(company.remainingTime));
            return false;
        }

        double amount = requestedAmount > 0d ? requestedAmount : company.investmentCost;
        if (amount < company.investmentCost)
        {
            SetStatus(company, $"Minimum ${NumberFormatter.FormatPrice(company.investmentCost)}", "Musait");
            return false;
        }

        if (CurrencyManager.Instance == null)
        {
            SetStatus(company, "Para sistemi yok", "Musait");
            return false;
        }

        if (!CurrencyManager.Instance.SpendMoney(amount))
        {
            SetStatus(company, "Yetersiz bakiye", "Musait");
            return false;
        }

        company.activeInvestmentAmount = amount;
        company.remainingTime = UnityEngine.Random.Range(company.minInvestmentDuration, company.maxInvestmentDuration);
        company.isInvestmentActive = true;
        SetStatus(company, "Yatirim aktif", FormatRemainingTime(company.remainingTime));
        return true;
    }

    private void TickActiveInvestments(float deltaTime)
    {
        if (companies == null)
        {
            return;
        }

        foreach (CompanyInvestmentData company in companies)
        {
            if (company == null || !company.isInvestmentActive)
            {
                continue;
            }

            company.remainingTime = Mathf.Max(0f, company.remainingTime - deltaTime);

            if (company.remainingTime <= 0f)
            {
                ResolveInvestment(company);
            }
            else
            {
                company.timerText = FormatRemainingTime(company.remainingTime);
                CompanyChanged?.Invoke(company);
            }
        }
    }

    private void ResolveInvestment(CompanyInvestmentData company)
    {
        bool success = UnityEngine.Random.value <= company.successChance;
        double receivedAmount = 0d;
        double resultPercent = UnityEngine.Random.Range(MinResultPercent, MaxResultPercent);
        double resultPortion = company.activeInvestmentAmount * (resultPercent / 100d);

        if (success)
        {
            double payout = company.activeInvestmentAmount + resultPortion;
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddMoney(payout, false);
            }

            receivedAmount = payout;
            SetStatus(company, "Basarili! Kar kazanildi", "Musait");
        }
        else
        {
            double refund = System.Math.Max(0d, company.activeInvestmentAmount - resultPortion);
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddMoney(refund, false);
            }

            receivedAmount = refund;
            SetStatus(company, "Basarisiz! Yatirim kaybedildi", "Musait");
        }

        InvestmentResolved?.Invoke(new InvestmentResult(company.companyId, company.companyName, success, receivedAmount));

        company.isInvestmentActive = false;
        company.remainingTime = 0f;
        company.activeInvestmentAmount = 0d;
        CompanyChanged?.Invoke(company);
    }

    private void SetStatus(CompanyInvestmentData company, string status, string timer)
    {
        company.statusText = status;
        company.timerText = timer;
        CompanyChanged?.Invoke(company);
    }

    private static CompanyInvestmentData CreateCompany(int id, string name, CompanySize size)
    {
        bool isLarge = size == CompanySize.Large;
        return new CompanyInvestmentData
        {
            companyId = id,
            companyName = name,
            companySize = size,
            investmentCost = isLarge ? 10000d : 1000d,
            successChance = isLarge ? 0.70f : 0.30f,
            minInvestmentDuration = 60f,
            maxInvestmentDuration = 120f,
            isInvestmentActive = false,
            remainingTime = 0f,
            statusText = "Hazir",
            timerText = "Musait"
        };
    }

    private static string FormatRemainingTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainderSeconds = totalSeconds % 60;
        return $"Sonuc {minutes:00}:{remainderSeconds:00}";
    }
}

public readonly struct InvestmentResult
{
    public InvestmentResult(int companyId, string companyName, bool success, double receivedAmount)
    {
        CompanyId = companyId;
        CompanyName = companyName;
        Success = success;
        ReceivedAmount = receivedAmount;
    }

    public int CompanyId { get; }
    public string CompanyName { get; }
    public bool Success { get; }
    public double ReceivedAmount { get; }
}
