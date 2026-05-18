using System.Collections.Generic;
using UnityEngine;

public class PlayerPanel : BaseUpgradePanel
{
    [Header("Skill Sale Items")]
    [SerializeField] private bool showQuickCashSaleItem = true;
    [SerializeField] private bool showBusinessSurchargeSaleItem = true;
    [SerializeField] private bool showOtomaticBasSaleItem = true;
    [SerializeField] private bool showHandOfMidasSaleItem = true;

    private TapLevelRiserWidget tapLevelRiserWidget;

    protected override void Start()
    {
        Transform cardRoot = UIHelper.FindChildRecursive(transform, "Cardroots");
        if (cardRoot != null)
        {
            contentContainer = cardRoot;
        }

        SetupTapLevelRiser();

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

        if (UpgradeManager.Instance == null)
        {
            return providers;
        }

        if (UpgradeManager.Instance.playerProfitUpgrades != null)
        {
            foreach (PlayerProfitUpgrade playerUpgrade in UpgradeManager.Instance.playerProfitUpgrades)
            {
                if (playerUpgrade != null)
                {
                    providers.Add(new PlayerProfitCardAdapter(playerUpgrade));
                }
            }
        }

        if (UpgradeManager.Instance.collectorUpgrade != null)
        {
            providers.Add(new CollectorCardAdapter(UpgradeManager.Instance.collectorUpgrade));
        }

        List<PlayerPanelSkillState> visibleSkills = UpgradeManager.Instance.GetVisiblePlayerPanelSkills();
        foreach (PlayerPanelSkillState skill in visibleSkills)
        {
            if (skill != null)
            {
                providers.Add(new PlayerPanelSkillCardAdapter(skill));
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

        if (showOtomaticBasSaleItem)
        {
            providers.Add(new OtomaticBasUnlockCardAdapter(GetSkillData(SkillId.OtomaticBas)));
        }

        if (showHandOfMidasSaleItem)
        {
            providers.Add(new HandOfMidasUnlockCardAdapter(GetSkillData(SkillId.HandOfMidas)));
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

    private void SetupTapLevelRiser()
    {
        if (tapLevelRiserWidget != null) return;

        Transform tapRiser = FindTapRiserTab();
        if (tapRiser == null)
        {
            Debug.LogWarning("[PlayerPanel] TapRiser_Upgrader tab was not found under PlayerPanel.");
            return;
        }

        tapLevelRiserWidget = tapRiser.GetComponent<TapLevelRiserWidget>();
        if (tapLevelRiserWidget == null)
        {
            tapLevelRiserWidget = tapRiser.gameObject.AddComponent<TapLevelRiserWidget>();
        }

        tapLevelRiserWidget.Initialize();
    }

    private Transform FindTapRiserTab()
    {
        Transform directTab = FindDirectChild("TapRiser_Upgrader");
        if (directTab != null)
        {
            return directTab;
        }

        directTab = FindDirectChild("Tap_Level_Riser");
        if (directTab != null)
        {
            return directTab;
        }

        return UIHelper.FindChildRecursive(transform, "TapRiser_Upgrader") ??
               UIHelper.FindChildRecursive(transform, "Tap_Level_Riser");
    }

    private Transform FindDirectChild(string childName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }
}
