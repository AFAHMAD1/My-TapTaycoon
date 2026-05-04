using UnityEngine;

/// <summary>
/// Oyun içindeki görsel efektleri (konfeti, patlama vb.) yöneten merkezi sınıf.
/// </summary>
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("Particle Prefabs")]
    public GameObject levelUpEffectPrefab; // Seviye atlayınca patlayan konfeti
    public GameObject clickEffectPrefab;   // Tıklayınca çıkan efekt

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Belirli bir pozisyonda seviye atlama efekti oluşturur.
    /// </summary>
    public void PlayLevelUpEffect(Vector3 position)
    {
        if (levelUpEffectPrefab != null)
        {
            GameObject effect = Instantiate(levelUpEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 3f); // 3 saniye sonra temizle
        }
    }

    /// <summary>
    /// Tıklama efektini oynatır.
    /// </summary>
    public void PlayClickEffect(Vector3 position)
    {
        if (clickEffectPrefab != null)
        {
            GameObject effect = Instantiate(clickEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
}
