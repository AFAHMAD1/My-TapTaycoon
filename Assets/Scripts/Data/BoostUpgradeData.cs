using UnityEngine;

public enum BoostTargetType
{
    // Tum binalari ayni anda etkileyen boost turu.
    AllBuildings,
    // Sadece Inspector'da secilen tek bir binayi etkileyen boost turu.
    SpecificBuilding,
    // Otomatik toplayicinin hareket hizini artiran boost turu.
    CollectorSpeed,
    // Otomatik toplayicinin tek seferde tasiyabilecegi para miktarini artiran boost turu.
    CollectorCapacity
}

[CreateAssetMenu(fileName = "New Boost Upgrade", menuName = "IdleGame/Boost Upgrade")]
public class BoostUpgradeData : UpgradableEntityData
{
    // Kartta veya aciklama alaninda oyuncuya gosterilecek boost metni.
    public string description;
    
    [Header("Boost Etkisi")]
    // Bu boost'un oyunda hangi hedefi etkileyecegini belirler.
    public BoostTargetType targetType = BoostTargetType.AllBuildings;
    
    [Tooltip("Sadece SpecificBuilding seciliyse gecerli olur.")]
    public IncomeBuildingData targetBuilding;

    [Tooltip("Orn: 5 (5 kat daha fazla kazanc)")]
    // Carpim degeri; 5 yazarsan hedeflenen gelir/hiz 5 katina cikar.
    public float boostMultiplier = 5f;

    [Header("Gorunum")]
    public Color cardBackgroundColor = new Color32(236, 217, 196, 255);
    public Color buttonColor = new Color32(32, 151, 220, 255);
}
