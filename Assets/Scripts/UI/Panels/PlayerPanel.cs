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

        if (UpgradeManager.Instance == null || UpgradeManager.Instance.playerProfitUpgrades == null)
        {
            return providers;
        }

        foreach (PlayerProfitUpgrade playerUpgrade in UpgradeManager.Instance.playerProfitUpgrades)
        {
            if (playerUpgrade != null)
            {
                providers.Add(new PlayerProfitCardAdapter(playerUpgrade));
            }
        }

        return providers;
    }
}
