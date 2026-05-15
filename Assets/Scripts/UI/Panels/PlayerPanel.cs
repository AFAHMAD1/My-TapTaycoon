using System.Collections.Generic;
using UnityEngine;

public class PlayerPanel : BaseUpgradePanel
{
    [Header("Skill Sale Items")]
    [SerializeField] private bool showQuickCashSaleItem = true;
    [SerializeField] private bool showBusinessSurchargeSaleItem = true;

    protected override void Start()
    {
        Transform cardRoot = UIHelper.FindChildRecursive(transform, "Cardroots");
        if (cardRoot != null)
        {
            contentContainer = cardRoot;
        }

        base.Start();
    }

    private void OnEnable()
    {
        PurchaseService.OnAnyPurchaseCompleted += RefreshPanel;
    }

    private void OnDisable()
    {
        PurchaseService.OnAnyPurchaseCompleted -= RefreshPanel;
    }

    private void RefreshPanel()
    {
        if (gameObject.activeInHierarchy)
        {
            CreateCards();
        }
    }

    protected override List<ICardDataProvider> GetDataProviders()
    {
        List<ICardDataProvider> providers = new List<ICardDataProvider>();

        if (UpgradeManager.Instance != null && UpgradeManager.Instance.playerProfitUpgrades != null)
        {
            foreach (PlayerProfitUpgrade playerUpgrade in UpgradeManager.Instance.playerProfitUpgrades)
            {
                if (playerUpgrade != null)
                {
                    providers.Add(new PlayerProfitCardAdapter(playerUpgrade));
                }
            }
        }

        if (showQuickCashSaleItem)
        {
            providers.Add(new QuickCashUnlockCardAdapter(GetQuickCashSkillData()));
        }

        if (showBusinessSurchargeSaleItem)
        {
            providers.Add(new BusinessSurchargeUnlockCardAdapter(GetSkillData(SkillId.BusinessSurcharge)));
        }

        return providers;
    }

    private SkillData GetQuickCashSkillData()
    {
        return GetSkillData(SkillId.QuickCash);
    }

    private SkillData GetSkillData(SkillId skillId)
    {
        if (SkillManager.Instance == null || SkillManager.Instance.allSkillDatas == null)
        {
            return null;
        }

        foreach (SkillData skillData in SkillManager.Instance.allSkillDatas)
        {
            if (skillData != null && skillData.skillId == skillId)
            {
                return skillData;
            }
        }

        return null;
    }
}
