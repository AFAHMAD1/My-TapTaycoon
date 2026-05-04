using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// UI elemanlarına (butonlar vb.) tıklama veya üzerine gelme durumunda 
/// esneme/zıplama efekti veren yardımcı bileşendir.
/// </summary>
public class UIBounce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Ayarlar")]
    public float scaleFactor = 0.9f; // Tıklandığında ne kadar küçülsün?
    public float duration = 0.1f; // Animasyon hızı
    public bool bounceOnEnable = true; // Panel açıldığında zıplama efekti yapsın mı?

    private Vector3 originalScale; // Nesnenin orijinal boyutu
    private Coroutine currentCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (bounceOnEnable)
        {
            StartCoroutine(BounceEffect());
        }
    }

    // Fare/Parmak basıldığında küçül
    public void OnPointerDown(PointerEventData eventData)
    {
        StopCurrent();
        currentCoroutine = StartCoroutine(ScaleTo(originalScale * scaleFactor));
    }

    // Bırakıldığında orijinal boyuta dön
    public void OnPointerUp(PointerEventData eventData)
    {
        StopCurrent();
        currentCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    // Fare üzerine geldiğinde hafifçe büyü
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopCurrent();
        currentCoroutine = StartCoroutine(ScaleTo(originalScale * 1.05f));
    }

    // Fare ayrıldığında orijinal boyuta dön
    public void OnPointerExit(PointerEventData eventData)
    {
        StopCurrent();
        currentCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    private void StopCurrent()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
    }

    /// <summary>
    /// Nesneyi belirli bir boyuta yumuşak bir şekilde getirir.
    /// </summary>
    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Oyun durdurulsa bile (Pause) animasyon çalışsın
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    /// <summary>
    /// Dikkat çekici bir zıplama efekti.
    /// </summary>
    private IEnumerator BounceEffect()
    {
        yield return ScaleTo(originalScale * 1.1f);
        yield return ScaleTo(originalScale);
    }
}
