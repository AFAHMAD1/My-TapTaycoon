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

    public void InitializeForSpawn(BanknotePool pool)
    {
        ownerPool = pool;
        isCollected = false;
        isReserved = false;
        transform.localScale = Vector3.zero;
        StopAllCoroutines();
        StartCoroutine(PopAnimation());
    }

    public void ResetForPool()
    {
        StopAllCoroutines();
        isCollected = false;
        isReserved = false;
        transform.localScale = Vector3.one;
        BanknoteRegistry.Unregister(this);
    }

    public void Release()
    {
        if (ownerPool != null)
        {
            ownerPool.Release(this);
            return;
        }

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator PopAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale * 1.25f, percent);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnDestroy()
    {
        BanknoteRegistry.Unregister(this);
    }
}
