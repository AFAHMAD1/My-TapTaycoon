using UnityEngine;

public class Banknote : MonoBehaviour
{
    [HideInInspector] public bool isCollected = false;
    [HideInInspector] public bool isReserved = false;

    private BanknotePool ownerPool;

    public double Value
    {
        get
        {
            if (UpgradeManager.Instance != null)
            {
                return System.Math.Max(1.0, UpgradeManager.Instance.CurrentClickValue);
            }

            return 1.0;
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    public void InitializeForSpawn(BanknotePool pool)
    {
        ownerPool = pool;
        isCollected = false;
        isReserved = false;
        transform.localScale = Vector3.zero;
        StopAllCoroutines();
        StartCoroutine(PopAnimation());
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void ResetForPool()
    {
        StopAllCoroutines();
        isCollected = false;
        isReserved = false;
        transform.localScale = Vector3.one;
        BanknoteRegistry.Unregister(this);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Release()
    {
        if (ownerPool != null)
        {
            ownerPool.Release(this);
            return;
        }

        Destroy(gameObject);
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    private System.Collections.IEnumerator PopAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;

        // Bu dongu kosul dogru kaldigi surece calisir; kosul bozulunca durur.
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale * 1.25f, percent);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    // Obje yok edilirken calisir; geride referans veya event kalmasini onler.
    private void OnDestroy()
    {
        BanknoteRegistry.Unregister(this);
    }
}
