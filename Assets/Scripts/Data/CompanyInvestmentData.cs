using System;
using UnityEngine;

[Serializable]
public class CompanyInvestmentData
{
    public int companyId;
    public string companyName;
    public CompanySize companySize;
    public double investmentCost;
    [Range(0f, 1f)] public float successChance;
    public float minInvestmentDuration;
    public float maxInvestmentDuration;
    public float profitMultiplier;
    public bool isInvestmentActive;
    public float remainingTime;

    [NonSerialized] public double activeInvestmentAmount;
    [NonSerialized] public string statusText = "Hazir";
    [NonSerialized] public string timerText = "Musait";

    public string SizeText => companySize == CompanySize.Large ? "Buyuk" : "Kucuk";
    public string RiskText => companySize == CompanySize.Large ? "Dusuk Risk" : "Yuksek Risk";
    public double PossibleReward => GetRewardFor(investmentCost);

    public double GetRewardFor(double amount)
    {
        return Math.Max(0d, amount) * profitMultiplier;
    }
}
