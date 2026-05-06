using System.Collections;
using UnityEngine;

/// <summary>
/// Toplayıcı karakterin (robot) sahnedeki fiziksel hareketlerini (koşma, zıplama) yöneten sınıftır.
/// </summary>
public class CollectorMovement : MonoBehaviour
{
    [Header("Yatay Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 6f; // Temel hareket hızı

    [Header("Zıplama Ayarları")]
    [SerializeField] private float jumpDuration = 0.25f; // Zıplama süresi
    [SerializeField] private float jumpHeight = 1.5f; // Zıplama yüksekliği

    /// <summary>
    /// Geliştirmelerden gelen hızı da hesaba katarak güncel hızı döner.
    /// </summary>
    public float GetCurrentSpeed()
    {
        return moveSpeed * (UpgradeManager.Instance != null ? UpgradeManager.Instance.GetCollectorSpeedMultiplier() : 1f);
    }

    /// <summary>
    /// Karakteri yatay eksende (X) hedef noktaya doğru hareket ettirir.
    /// </summary>
    public IEnumerator MoveToX(float targetX)
    {
        // Bu dongu kosul dogru kaldigi surece calisir; kosul bozulunca durur.
        while (Mathf.Abs(transform.position.x - targetX) > 0.05f)
        {
            Vector3 pos = transform.position;

            pos.x = Mathf.MoveTowards(
                pos.x,
                targetX,
                GetCurrentSpeed() * Time.deltaTime
            );

            transform.position = pos;

            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        transform.position = finalPos;
    }

    /// <summary>
    /// Karakteri belirlenen bir noktaya zıplayarak götürür (Parabolik hareket).
    /// </summary>
    public IEnumerator JumpToPoint(Vector3 targetPosition)
    {
        Vector3 start = transform.position;
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'duration' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        float duration = Mathf.Max(0.01f, jumpDuration);
        float elapsed = 0f;

        // Bu dongu kosul dogru kaldigi surece calisir; kosul bozulunca durur.
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Bu satir: 'Mathf' uzerindeki 'Clamp01' metodunu cagirir ve sonucu 't' degiskenine koyar; degeri 0 ile 1 arasina sikistirir; yuzde/progress hesabi icin kullanilir.
            float t = Mathf.Clamp01(elapsed / duration);
            // Bu satir: 'Vector3' uzerindeki 'Lerp' metodunu cagirir ve sonucu 'pos' degiskenine koyar; iki deger arasinda yavas gecis hesaplar; animasyon ve hareketlerde kullanilir.
            Vector3 pos = Vector3.Lerp(start, targetPosition, t);
            
            // Zıplama eğrisi (Sinüs dalgası ile)
            pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPosition;
    }
}
