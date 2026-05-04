using UnityEngine;

/// <summary>
/// Oyunun genel görsel atmosferini (arka plan, zemin vb.) değiştiren sınıftır.
/// </summary>
public class ThemeManager : MonoBehaviour
{
    [Header("Görsel Referanslar")]
    [Tooltip("Gök yüzü veya arka plan SpriteRenderer bileşeni.")]
    public SpriteRenderer backgroundRenderer;
    
    [Tooltip("Zemin (Yeşil alan) SpriteRenderer bileşeni.")]
    public SpriteRenderer groundRenderer;

    [Header("Tema Renk Tanımları")]
    public Color bgClassic = new Color(0.3f, 0.2f, 0.6f); 
    public Color groundClassic = new Color(0.2f, 0.7f, 0.2f); 

    public Color bgNight = new Color(0.1f, 0.1f, 0.15f); 
    public Color groundNight = new Color(0.2f, 0.2f, 0.25f); 

    public Color bgDay = new Color(0.5f, 0.8f, 1f); 
    public Color groundDay = new Color(0.9f, 0.8f, 0.5f); 

    /// <summary>
    /// Klasik (Mor-Yeşil) temayı uygular.
    /// </summary>
    public void ApplyClassicTheme()
    {
        SetTheme(bgClassic, groundClassic);
    }

    /// <summary>
    /// Gece (Karanlık) temasını uygular.
    /// </summary>
    public void ApplyNightTheme()
    {
        SetTheme(bgNight, groundNight);
    }

    /// <summary>
    /// Gündüz (Aydınlık) temasını uygular.
    /// </summary>
    public void ApplyDayTheme()
    {
        SetTheme(bgDay, groundDay);
    }

    // Renkleri SpriteRenderer'lara atayan yardımcı fonksiyon.
    private void SetTheme(Color bgColor, Color groundColor)
    {
        if (backgroundRenderer != null) backgroundRenderer.color = bgColor;
        if (groundRenderer != null) groundRenderer.color = groundColor;
        
        // Tema bilgisini kaydet (Oyun açıldığında hatırlanması için)
        PlayerPrefs.SetString("CurrentTheme", bgColor.ToString()); 
    }
}
