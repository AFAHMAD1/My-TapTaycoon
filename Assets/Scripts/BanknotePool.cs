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

    public void ConfigureActiveRoot(Transform root)
    {
        activeRoot = root;
        EnsureActiveRoot();
    }

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

    public void Release(Banknote banknote)
    {
        if (banknote == null)
        {
            return;
        }

        banknote.ResetForPool();
        ReleaseToPool(banknote);
    }

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
