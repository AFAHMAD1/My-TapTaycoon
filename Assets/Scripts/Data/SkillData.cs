using UnityEngine;

public enum SkillEffectType
{
    ProfitBoost,        // Örn: İşletme Süper Yüklendi (2x işletme kârı)
    InstantCash,        // Örn: Hızlı Nakit (2 dakikalık gelir)
    AutoClicker,        // Örn: Otomatik Dokun
    ClickPowerBoost     // Örn: Midas'ın Eli (Dokunma kârı 10x)
}

[CreateAssetMenu(fileName = "New Skill", menuName = "IdleGame/Skill Data")]
public class SkillData : UpgradableEntityData
{
    [Header("Beceri Ayarlari")]
    public SkillEffectType effectType;
    [TextArea] public string descriptionTemplate; // Örn: "{0} Saniyeliğine {1}x işletme kârı"
    public Sprite icon;
    
    [Header("Güç ve Bekleme (Level'a göre artar/azalır)")]
    public ScaledValue effectDuration = new ScaledValue(); // Becerinin ne kadar süre aktif kalacağı (örn: 30 saniye)
    public ScaledValue effectPower = new ScaledValue();    // Becerinin gücü (örn: 2x, 10x)
    public ScaledValue cooldownTime = new ScaledValue();   // Bekleme süresi (Dakika veya saniye)
    
    [Header("Gorsel Tema")]
    public Color cardBackgroundColor = new Color32(245, 240, 225, 255);
}
