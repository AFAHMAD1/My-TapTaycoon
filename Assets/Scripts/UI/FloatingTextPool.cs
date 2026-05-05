using UnityEngine;

public class FloatingTextPool : ComponentPool<FloatingText>
{
    public static FloatingTextPool Instance { get; private set; }
    [SerializeField] private Transform activeRoot;

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
        EnsureActiveRoot();
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    public void ConfigureActiveRoot(Transform root)
    {
        activeRoot = root;
        EnsureActiveRoot();
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    public FloatingText Spawn(Vector3 position, Quaternion rotation)
    {
        FloatingText floatingText = GetOrCreate();
        if (floatingText == null)
        {
            return null;
        }

        Transform floatingTransform = floatingText.transform;
        floatingTransform.SetParent(activeRoot, false);
        floatingTransform.SetPositionAndRotation(position, rotation);
        floatingText.gameObject.SetActive(true);
        floatingText.InitializeForSpawn(this);
        return floatingText;
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Release(FloatingText floatingText)
    {
        if (floatingText == null)
        {
            return;
        }

        floatingText.ResetForPool();
        ReleaseToPool(floatingText);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void EnsureActiveRoot()
    {
        if (activeRoot != null)
        {
            return;
        }

        GameObject runtimeRoot = GameObject.Find("FloatingTextRuntime");
        if (runtimeRoot == null)
        {
            runtimeRoot = new GameObject("FloatingTextRuntime");
        }

        activeRoot = runtimeRoot.transform;
    }
}
