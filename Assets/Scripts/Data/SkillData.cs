using UnityEngine;

public enum SkillEffectType
{
    // Isletmelerin kazancini belli sureligine artirir.
    ProfitBoost,        // Örn: İşletme Süper Yüklendi (2x işletme kârı)
    // Oyuncuya aninda para verir.
    InstantCash,        // Örn: Hızlı Nakit (2 dakikalık gelir)
    // Oyuncu tiklamadan otomatik tiklama etkisi verir.
    AutoClicker,        // Örn: Otomatik Dokun
    // Tiklamadan gelen kazanci belli sureligine artirir.
    ClickPowerBoost     // Örn: Midas'ın Eli (Dokunma kârı 10x)
}

[CreateAssetMenu(fileName = "New Skill", menuName = "IdleGame/Skill Data")]
public class SkillData : UpgradableEntityData
{
    [Header("Beceri Ayarlari")]
    // Skill kullanildiginda hangi tur etki calisacak bunu belirler.
    public SkillEffectType effectType;
    // UI aciklamasinda level'a gore doldurulacak metin sablonudur.
    [TextArea] public string descriptionTemplate; // Örn: "{0} Saniyeliğine {1}x işletme kârı"
    public Sprite icon;
    
    [Header("Güç ve Bekleme (Level'a göre artar/azalır)")]
    public ScaledValue effectDuration = new ScaledValue(); // Becerinin ne kadar süre aktif kalacağı (örn: 30 saniye)
    public ScaledValue effectPower = new ScaledValue();    // Becerinin gücü (örn: 2x, 10x)
    public ScaledValue cooldownTime = new ScaledValue();   // Bekleme süresi (Dakika veya saniye)
    
    [Header("Gorsel Tema")]
    public Color cardBackgroundColor = new Color32(245, 240, 225, 255);
}
