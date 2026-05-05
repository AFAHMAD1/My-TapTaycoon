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
            return availableItems.Dequeue();
        }

        if (!allowRuntimeExpansion)
        {
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
        item.transform.SetParent(poolRoot, false);
        item.gameObject.SetActive(false);

        if (!availableItems.Contains(item))
        {
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

        availableItems.Clear();
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
            item.transform.SetParent(poolRoot, false);
            item.gameObject.SetActive(false);
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

        knownItems.Add(item);
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private T CreateInstance()
    {
        if (prefab == null)
        {
            Debug.LogError($"{name}: Pool prefab is missing.");
            return null;
        }

        T instance = Instantiate(prefab, poolRoot);
        instance.gameObject.SetActive(false);
        return instance;
    }
}
