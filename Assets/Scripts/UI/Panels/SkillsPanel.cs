using System.Collections.Generic;
using UnityEngine;

public class SkillsPanel : BaseUpgradePanel
{
    protected override List<ICardDataProvider> GetDataProviders()
    {
        var providers = new List<ICardDataProvider>();

        if (SkillManager.Instance != null)
        {
            foreach (var skill in SkillManager.Instance.unlockedSkills)
            {
                providers.Add(new SkillCardAdapter(skill));
            }
        }

        return providers;
    }
}
