using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [Tooltip("Yazinin yukari dogru cikma hizi")]
    public float moveSpeed = 1.5f;

    [Tooltip("Yazinin ekranda kalma ve yok olma suresi")]
    public float lifetime = 0.6f;

    private TextMeshPro textMesh;
    private SpriteRenderer iconRenderer;
    private Color originalTextColor;
    private Color originalIconColor;
    private float timer = 0f;
    private Vector3 initialLocalScale;
    private FloatingTextPool ownerPool;

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshPro>(true);
        iconRenderer = GetComponentInChildren<SpriteRenderer>(true);
        initialLocalScale = transform.localScale;

        if (textMesh != null)
        {
            originalTextColor = textMesh.color;
        }

        if (iconRenderer != null)
        {
            originalIconColor = iconRenderer.color;
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    public void InitializeForSpawn(FloatingTextPool pool)
    {
        ownerPool = pool;
        timer = 0f;
        transform.localScale = initialLocalScale;

        if (textMesh != null)
        {
            textMesh.color = originalTextColor;
            textMesh.text = string.Empty;
        }

        if (iconRenderer != null)
        {
            iconRenderer.color = originalIconColor;
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    public void Setup(string textContent)
    {
        timer = 0f;

        if (textMesh != null)
        {
            textMesh.text = textContent;
            textMesh.color = originalTextColor;
        }

        if (iconRenderer != null)
        {
            iconRenderer.color = originalIconColor;
        }

        transform.position += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0f, 0.2f), 0f);
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void ResetForPool()
    {
        timer = 0f;
        transform.localScale = initialLocalScale;

        if (textMesh != null)
        {
            textMesh.text = string.Empty;
            textMesh.color = originalTextColor;
        }

        if (iconRenderer != null)
        {
            iconRenderer.color = originalIconColor;
        }
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        // Bu satir: 'Mathf' uzerindeki 'Lerp' metodunu cagirir ve sonucu 'alpha' degiskenine koyar; iki deger arasinda yavas gecis hesaplar; animasyon ve hareketlerde kullanilir.
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);

        if (textMesh != null)
        {
            textMesh.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);
        }

        if (iconRenderer != null)
        {
            iconRenderer.color = new Color(originalIconColor.r, originalIconColor.g, originalIconColor.b, alpha);
        }

        if (timer >= lifetime)
        {
            if (ownerPool != null)
            {
                // Bu satir: 'ownerPool' objesi uzerindeki 'Release' metodunu cagirir; kullanimi biten objeyi serbest birakir veya havuza geri gonderir.
                ownerPool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
