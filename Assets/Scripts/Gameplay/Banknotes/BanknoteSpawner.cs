using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class BanknoteSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject banknotePrefab;
    public GameObject floatingTextPrefab;

    [Header("Spawn")]
    public Camera cam;
    public float zDepth = 0f;
    public int preloadCount = 16;

    // Obje aktif olunca calisir; event dinleyicileri veya gecici durumlar burada hazirlanir.
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    // Obje pasif olunca calisir; acik kalan event/durumlar burada temizlenir.
    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        EnsurePools();
    }

    // Unity bu fonksiyonu her frame calistirir; surekli kontrol veya animasyon gereken isler burada olur.
    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SpawnAtScreen(Mouse.current.position.ReadValue());
            return;
        }

        if (Touchscreen.current != null)
        {
            // Bu dongu listedeki elemanlari tek tek gezer; her eleman icin ayni islemi uygular.
            foreach (Touch touch in Touch.activeTouches)
            {
                if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
                {
                    continue;
                }

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.touchId))
                {
                    continue;
                }

                SpawnAtScreen(touch.screenPosition);
            }
        }
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    private void SpawnAtScreen(Vector2 screenPos)
    {
        if (BanknotePool.Instance == null)
        {
            Debug.LogError("BanknoteSpawner: BanknotePool bulunamadi.");
            return;
        }

        if (cam == null)
        {
            Debug.LogError("BanknoteSpawner: camera reference is missing.");
            return;
        }

        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        world.z = zDepth;
        world.x += Random.Range(-0.15f, 0.15f);
        world.y += Random.Range(-0.05f, 0.05f);

        Banknote banknote = BanknotePool.Instance.Spawn(world, Quaternion.identity);
        if (banknote == null)
        {
            Debug.LogError("BanknoteSpawner: pool'dan banknote alinmadi.");
            return;
        }

        BanknoteRegistry.Register(banknote);

        if (FloatingTextPool.Instance != null)
        {
            Vector3 textPos = world + new Vector3(0f, 0.6f, -1f);
            FloatingText floatingText = FloatingTextPool.Instance.Spawn(textPos, Quaternion.identity);

            if (floatingText != null)
            {
                floatingText.Setup("+$" + NumberFormatter.Format(banknote.Value));
            }
        }
    }

    // Bu fonksiyon ilgili sistemi veya UI parcasini hazirlar/gunceller.
    private void EnsurePools()
    {
        if (BanknotePool.Instance == null && banknotePrefab != null)
        {
            Banknote banknoteComponent = banknotePrefab.GetComponent<Banknote>();
            if (banknoteComponent != null)
            {
                GameObject poolObject = new GameObject("BanknotePool");
                BanknotePool pool = poolObject.AddComponent<BanknotePool>();
                pool.Configure(banknoteComponent, preloadCount, poolObject.transform, false);
                pool.ConfigureActiveRoot(GetOrCreateRuntimeRoot("BanknoteRuntime"));
            }
        }

        if (FloatingTextPool.Instance == null && floatingTextPrefab != null)
        {
            FloatingText floatingTextComponent = floatingTextPrefab.GetComponent<FloatingText>();
            if (floatingTextComponent != null)
            {
                GameObject poolObject = new GameObject("FloatingTextPool");
                FloatingTextPool pool = poolObject.AddComponent<FloatingTextPool>();
                pool.Configure(floatingTextComponent, preloadCount, poolObject.transform, false);
                pool.ConfigureActiveRoot(GetOrCreateRuntimeRoot("FloatingTextRuntime"));
            }
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    private Transform GetOrCreateRuntimeRoot(string objectName)
    {
        GameObject runtimeRoot = GameObject.Find(objectName);
        if (runtimeRoot == null)
        {
            runtimeRoot = new GameObject(objectName);
        }

        return runtimeRoot.transform;
    }
}
