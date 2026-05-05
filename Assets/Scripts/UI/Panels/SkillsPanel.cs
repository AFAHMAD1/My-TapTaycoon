using System.Collections.Generic;
using UnityEngine;

public class SkillsPanel : BaseUpgradePanel
{
    // ExitButton tiklaninca bu fonksiyon cagrilir ve SkillsPanel objesini kapatir.
    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    protected override List<ICardDataProvider> GetDataProviders()
    {
        var providers = new List<ICardDataProvider>();

        if (SkillManager.Instance != null)
        {
            // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
            foreach (var skill in SkillManager.Instance.unlockedSkills)
            {
                providers.Add(new SkillCardAdapter(skill));
            }
        }

        return providers;
    }
}
