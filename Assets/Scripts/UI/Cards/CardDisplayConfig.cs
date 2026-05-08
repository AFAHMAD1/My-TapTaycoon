using UnityEngine;

[System.Serializable]
public class CardDisplayConfig
{
    public bool showIcon = true;
    public bool showProgressBar = false;
    public bool showDescription = false;
    public bool showIncome = true;
    public bool showLevel = true;
    public bool showSecondaryButton = false; // Yeni! (Orn: Prestij butonu)
    public bool useSkillCardLayout = false;
    
    // Varsayılan olarak bina kartı görünümü
    public static CardDisplayConfig DefaultBuildingConfig => new CardDisplayConfig
    {
        showIcon = true,
        showProgressBar = true,
        showDescription = false,
        showIncome = true,
        showLevel = true
    };

    // Collector kartı (progress bar yok, prestij butonu simdilik kapali)
    public static CardDisplayConfig DefaultCollectorConfig => new CardDisplayConfig
    {
        showIcon = true,
        showProgressBar = false,
        showDescription = false,
        showIncome = true,
        showLevel = true,
        showSecondaryButton = false
    };
    
    // Özellik/Boost kartı (level ve income yerine açıklama var)
    public static CardDisplayConfig DefaultFeatureConfig => new CardDisplayConfig
    {
        showIcon = false, // Varsa true yapılabilir
        showProgressBar = false,
        showDescription = true,
        showIncome = false,
        showLevel = false
    };
}
