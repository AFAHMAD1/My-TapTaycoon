using UnityEngine;

public class FloatingTextPool : ComponentPool<FloatingText>
{
    public static FloatingTextPool Instance { get; private set; }
    [SerializeField] private Transform activeRoot;

    protected override void Awake()
    {
        Instance = this;
        // Bu satir: base, yani miras alinan ust sinif uzerindeki 'Awake' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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
        // Bu satir: 'floatingTransform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        floatingTransform.SetParent(activeRoot, false);
        // Bu satir: 'floatingTransform' objesi uzerindeki 'SetPositionAndRotation' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        floatingTransform.SetPositionAndRotation(position, rotation);
        // Bu satir: 'gameObject' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
        floatingText.gameObject.SetActive(true);
        // Bu satir: 'floatingText' objesi uzerindeki 'InitializeForSpawn' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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

        // Bu satir: 'floatingText' objesi uzerindeki 'ResetForPool' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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

        // Bu satir: 'GameObject' uzerindeki 'Find' metodunu cagirir ve sonucu 'runtimeRoot' degiskenine koyar; sahnede veya transform altinda verilen isimde obje arar.
        GameObject runtimeRoot = GameObject.Find("FloatingTextRuntime");
        if (runtimeRoot == null)
        {
            runtimeRoot = new GameObject("FloatingTextRuntime");
        }

        activeRoot = runtimeRoot.transform;
    }
}
