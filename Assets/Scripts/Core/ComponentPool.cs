using System.Collections.Generic;
using UnityEngine;

public class ComponentPool<T> : MonoBehaviour where T : Component
{
    [SerializeField] private T prefab;
    [SerializeField] private int initialSize = 16;
    [SerializeField] private Transform poolRoot;
    [SerializeField] private bool allowRuntimeExpansion = false;

    private readonly Queue<T> availableItems = new Queue<T>();
    private readonly HashSet<T> knownItems = new HashSet<T>();
    private bool hasInitialized;

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    protected virtual void Awake()
    {
        if (poolRoot == null)
        {
            poolRoot = transform;
        }

        InitializePoolIfNeeded();
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    public void Configure(T prefabReference, int preloadCount, Transform root = null, bool allowExpansion = false)
    {
        prefab = prefabReference;
        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'initialSize' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        initialSize = Mathf.Max(0, preloadCount);
        allowRuntimeExpansion = allowExpansion;

        if (root != null)
        {
            poolRoot = root;
        }
        else if (poolRoot == null)
        {
            poolRoot = transform;
        }

        InitializePoolIfNeeded(true);
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    protected T GetOrCreate()
    {
        if (availableItems.Count > 0)
        {
            // Bu satir: 'availableItems' objesi uzerindeki 'Dequeue' metodunu cagirir; Queue'nun basindaki elemani cikarip alir; pool sisteminde hazir obje almak icin kullanilir.
            return availableItems.Dequeue();
        }

        if (!allowRuntimeExpansion)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogWarning' metodunu cagirir; Unity Console'a uyari mesaji yazar; oyun durmaz ama ayar eksigi olabilir.
            Debug.LogWarning($"{name}: Pool bos. Yeni nesne uretilmedi.");
            return null;
        }

        T instance = CreateInstance();
        RegisterItem(instance);
        return instance;
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    protected void ReleaseToPool(T item)
    {
        if (item == null)
        {
            return;
        }

        RegisterItem(item);
        // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
        item.transform.SetParent(poolRoot, false);
        // Bu satir: 'gameObject' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
        item.gameObject.SetActive(false);

        if (!availableItems.Contains(item))
        {
            // Bu satir: 'availableItems' objesi uzerindeki 'Enqueue' metodunu cagirir; elemani Queue'nun sonuna ekler; pool sisteminde objeyi tekrar kullanima hazirlar.
            availableItems.Enqueue(item);
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void InitializePoolIfNeeded(bool forceRebuild = false)
    {
        if (hasInitialized && !forceRebuild)
        {
            return;
        }

        if (poolRoot == null)
        {
            return;
        }

        if (prefab == null && poolRoot.childCount == 0)
        {
            return;
        }

        // Bu satir: 'availableItems' objesi uzerindeki 'Clear' metodunu cagirir; listenin icindeki tum elemanlari siler; liste bos hale gelir.
        availableItems.Clear();
        // Bu satir: 'knownItems' objesi uzerindeki 'Clear' metodunu cagirir; listenin icindeki tum elemanlari siler; liste bos hale gelir.
        knownItems.Clear();
        hasInitialized = true;
        CollectExistingPoolItems();
        WarmupMissingItems();
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    private void CollectExistingPoolItems()
    {
        if (poolRoot == null)
        {
            return;
        }

        T[] existingItems = poolRoot.GetComponentsInChildren<T>(true);
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < existingItems.Length; i++)
        {
            T item = existingItems[i];
            if (item == null || item.transform == poolRoot)
            {
                continue;
            }

            RegisterItem(item);
            // Bu satir: 'transform' objesi uzerindeki 'SetParent' metodunu cagirir; UI/obje hiyerarsisinde bu objeyi verilen parent altina tasir.
            item.transform.SetParent(poolRoot, false);
            // Bu satir: 'gameObject' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
            item.gameObject.SetActive(false);
            // Bu satir: 'availableItems' objesi uzerindeki 'Enqueue' metodunu cagirir; elemani Queue'nun sonuna ekler; pool sisteminde objeyi tekrar kullanima hazirlar.
            availableItems.Enqueue(item);
        }
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    private void WarmupMissingItems()
    {
        if (prefab == null)
        {
            return;
        }

        // Bu satir: 'Mathf' uzerindeki 'Max' metodunu cagirir ve sonucu 'missingCount' degiskenine koyar; iki degerden buyuk olani secer; burada genelde alt sinir koymak icin kullanilir.
        int missingCount = Mathf.Max(0, initialSize - availableItems.Count);
        // Bu dongu sayac kullanarak ayni islemi belirli sayida tekrarlar.
        for (int i = 0; i < missingCount; i++)
        {
            T instance = CreateInstance();
            RegisterItem(instance);
            ReleaseToPool(instance);
        }
    }

    // Bu fonksiyon oyuncu aksiyonu veya oyun akisi icin bir islemi dener/uygular.
    private void RegisterItem(T item)
    {
        if (item == null)
        {
            return;
        }

        // Bu satir: 'knownItems' objesi uzerindeki 'Add' metodunu cagirir; listeye yeni bir eleman ekler; boylece daha sonra donguyle okunabilir.
        knownItems.Add(item);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private T CreateInstance()
    {
        if (prefab == null)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogError' metodunu cagirir; Unity Console'a hata mesaji yazar; duzeltilmesi gereken ciddi durumlari belirtir.
            Debug.LogError($"{name}: Pool prefab is missing.");
            return null;
        }

        T instance = Instantiate(prefab, poolRoot);
        // Bu satir: 'gameObject' objesi uzerindeki 'SetActive' metodunu cagirir; hedef GameObject'i acar veya kapatir; true gorunur/aktif, false gizli/pasif yapar.
        instance.gameObject.SetActive(false);
        return instance;
    }
}
