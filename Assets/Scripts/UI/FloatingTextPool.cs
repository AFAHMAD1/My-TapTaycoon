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

    public void ConfigureActiveRoot(Transform root)
    {
        activeRoot = root;
        EnsureActiveRoot();
    }

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

    public void Release(FloatingText floatingText)
    {
        if (floatingText == null)
        {
            return;
        }

        floatingText.ResetForPool();
        ReleaseToPool(floatingText);
    }

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
