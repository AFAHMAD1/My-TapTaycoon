using UnityEngine;

public class BanknotePool : ComponentPool<Banknote>
{
    public static BanknotePool Instance { get; private set; }
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
    public Banknote Spawn(Vector3 position, Quaternion rotation)
    {
        Banknote banknote = GetOrCreate();
        if (banknote == null)
        {
            return null;
        }

        Transform banknoteTransform = banknote.transform;
        // Bu satir: 'banknoteTransform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        banknoteTransform.SetParent(activeRoot, false);
        // Bu satir: 'banknoteTransform' objesi uzerindeki 'SetPositionAndRotation' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        banknoteTransform.SetPositionAndRotation(position, rotation);
        // Bu satir: 'gameObject' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
        banknote.gameObject.SetActive(true);
        // Bu satir: 'banknote' objesi uzerindeki 'InitializeForSpawn' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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

        // Bu satir: 'banknote' objesi uzerindeki 'ResetForPool' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
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

        // Bu satir: 'GameObject' uzerindeki 'Find' metodunu cagirir ve sonucu 'runtimeRoot' degiskenine koyar; sahnede veya transform altinda verilen isimde obje arar.
        GameObject runtimeRoot = GameObject.Find("BanknoteRuntime");
        if (runtimeRoot == null)
        {
            runtimeRoot = new GameObject("BanknoteRuntime");
        }

        activeRoot = runtimeRoot.transform;
    }
}
