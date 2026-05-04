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

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
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
                ownerPool.Release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
