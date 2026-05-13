using UnityEngine;

[System.Serializable]
public class CardDisplayConfig
{
    // Kartta ikon alaninin gorunup gorunmeyecegini belirler.
    public bool showIcon = true;
    // Bina uretim sureci gibi ilerleme barlari icin kullanilir.
    public bool showProgressBar = false;
    // Skill/boost gibi kartlarda aciklama metnini acip kapatir.
    public bool showDescription = false;
    // Gelir satirinin gorunup gorunmeyecegini belirler.
    public bool showIncome = true;
    // Level yazisinin gorunup gorunmeyecegini belirler.
    public bool showLevel = true;
    // Kartta ikinci bir buton gerekiyorsa bunu acar.
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
        showDescription = true,
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
