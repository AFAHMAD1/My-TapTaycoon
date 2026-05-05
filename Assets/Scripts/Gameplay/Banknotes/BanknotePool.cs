using UnityEngine;

public class BanknotePool : ComponentPool<Banknote>
{
    public static BanknotePool Instance { get; private set; }
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
    public Banknote Spawn(Vector3 position, Quaternion rotation)
    {
        Banknote banknote = GetOrCreate();
        if (banknote == null)
        {
            return null;
        }

        Transform banknoteTransform = banknote.transform;
        banknoteTransform.SetParent(activeRoot, false);
        banknoteTransform.SetPositionAndRotation(position, rotation);
        banknote.gameObject.SetActive(true);
        banknote.InitializeForSpawn(this);
        return banknote;
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    public void Release(Banknote banknote)
    {
        if (banknote == null)
        {
            return;
        }

        banknote.ResetForPool();
        ReleaseToPool(banknote);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void EnsureActiveRoot()
    {
        if (activeRoot != null)
        {
            return;
        }

        GameObject runtimeRoot = GameObject.Find("BanknoteRuntime");
        if (runtimeRoot == null)
        {
            runtimeRoot = new GameObject("BanknoteRuntime");
        }

        activeRoot = runtimeRoot.transform;
    }
}
