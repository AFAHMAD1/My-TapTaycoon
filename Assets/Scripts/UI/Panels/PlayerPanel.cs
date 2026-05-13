using System.Collections.Generic;
using UnityEngine;

public class PlayerPanel : BaseUpgradePanel
{
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

        if (UpgradeManager.Instance == null)
        {
            return providers;
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

        return providers;
    }
}
