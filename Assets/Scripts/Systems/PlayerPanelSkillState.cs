using UnityEngine;

public enum PlayerPanelSkillKind
{
    BuildingProfitMultiplier,
    BuildingAutoCollect,
    ClickProfitMultiplier,
    AllBuildingsProfitMultiplier,
    FastCash,
    SuperBusiness,
    AutoClick,
    CashPileChance,
    MidasTouch
}

[System.Serializable]
public class PlayerPanelSkillState
{
    public int deckIndex;
    public PlayerPanelSkillKind kind;
    public int targetBuildingIndex = -1;
    public float multiplier = 1f;
    public float durationSeconds;
    public float cashMinutes;
    public double cost;
    public bool purchased;

    public string GetDisplayName()
    {
        switch (kind)
        {
            case PlayerPanelSkillKind.BuildingProfitMultiplier:
                return $"{GetBuildingLabel()} Kar Carpani";
            case PlayerPanelSkillKind.BuildingAutoCollect:
                return "Otomatik Topla";
            case PlayerPanelSkillKind.ClickProfitMultiplier:
                return "Dokunma Kari";
            case PlayerPanelSkillKind.AllBuildingsProfitMultiplier:
                return "Tum Isletmeler";
            case PlayerPanelSkillKind.FastCash:
                return "Hizli Nakit";
            case PlayerPanelSkillKind.SuperBusiness:
                return "Super Isletme";
            case PlayerPanelSkillKind.AutoClick:
                return "Otomatik Dokun";
            case PlayerPanelSkillKind.CashPileChance:
                return "Nakit Yigini Sansi";
            case PlayerPanelSkillKind.MidasTouch:
                return "Midasin Eli";
            default:
                return "Skill";
        }
    }

    public string GetDescription()
    {
        switch (kind)
        {
            case PlayerPanelSkillKind.BuildingProfitMultiplier:
                return $"{GetBuildingLabel()} kazancini {multiplier:0.#}x yapar.";
            case PlayerPanelSkillKind.BuildingAutoCollect:
                return $"{GetBuildingLabel()} karini otomatik toplar.";
            case PlayerPanelSkillKind.ClickProfitMultiplier:
                return $"Dokunma kazancini {multiplier:0.#}x arttirir.";
            case PlayerPanelSkillKind.AllBuildingsProfitMultiplier:
                return $"Tum isletme kazancini {multiplier:0.#}x arttirir.";
            case PlayerPanelSkillKind.FastCash:
                return $"{cashMinutes:0.#} dakikalik bina kazancini hemen verir.";
            case PlayerPanelSkillKind.SuperBusiness:
                return $"{durationSeconds:0} saniye boyunca isletme kazanci {multiplier:0.#}x olur.";
            case PlayerPanelSkillKind.AutoClick:
                return $"{durationSeconds:0} saniye boyunca saniyede {multiplier:0.#} otomatik dokunur.";
            case PlayerPanelSkillKind.CashPileChance:
                return $"{durationSeconds:0} saniye boyunca nakit yigini sansini {multiplier:0.#}x yapar.";
            case PlayerPanelSkillKind.MidasTouch:
                return $"{durationSeconds:0} saniye boyunca dokunma kazanci {multiplier:0.#}x olur.";
            default:
                return "";
        }
    }

    private string GetBuildingLabel()
    {
        if (PassiveIncomeManager.Instance != null &&
            PassiveIncomeManager.Instance.buildings != null &&
            targetBuildingIndex >= 0 &&
            targetBuildingIndex < PassiveIncomeManager.Instance.buildings.Count &&
            PassiveIncomeManager.Instance.buildings[targetBuildingIndex] != null &&
            PassiveIncomeManager.Instance.buildings[targetBuildingIndex].data != null &&
            !string.IsNullOrWhiteSpace(PassiveIncomeManager.Instance.buildings[targetBuildingIndex].data.entityName))
        {
            return PassiveIncomeManager.Instance.buildings[targetBuildingIndex].data.entityName;
        }

        return targetBuildingIndex >= 0 ? $"{targetBuildingIndex + 1}. Bina" : "Bina";
    }
}
