using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class BanknoteSpawner : MonoBehaviour
{
    public static BanknoteSpawner Instance { get; private set; }

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
        // Bu satir: 'EnhancedTouchSupport' objesi uzerindeki 'Enable' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        EnhancedTouchSupport.Enable();
    }

    // Obje pasif olunca calisir; acik kalan event/durumlar burada temizlenir.
    private void OnDisable()
    {
        // Bu satir: 'EnhancedTouchSupport' objesi uzerindeki 'Disable' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        EnhancedTouchSupport.Disable();
    }

    // Unity bu fonksiyonu obje olusurken ilk calistirir; burada genelde singleton ve ilk referans ayarlari yapilir.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

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
            PerformTapAtScreenPosition(Mouse.current.position.ReadValue());
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

                PerformTapAtScreenPosition(touch.screenPosition);
            }
        }
    }

    public void PerformTapAtScreenPosition(Vector2 screenPos)
    {
        SpawnAtScreen(screenPos);
    }

    // Bu fonksiyon, sinifin sorumlu oldugu isin bir parcasini yapar.
    private void SpawnAtScreen(Vector2 screenPos)
    {
        if (BanknotePool.Instance == null)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogError' metodunu cagirir; Unity Console'a hata mesaji yazar; duzeltilmesi gereken ciddi durumlari belirtir.
            Debug.LogError("BanknoteSpawner: BanknotePool bulunamadi.");
            return;
        }

        if (cam == null)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogError' metodunu cagirir; Unity Console'a hata mesaji yazar; duzeltilmesi gereken ciddi durumlari belirtir.
            Debug.LogError("BanknoteSpawner: camera reference is missing.");
            return;
        }

        // Bu satir: 'cam' uzerindeki 'ScreenToWorldPoint' metodunu cagirir ve sonucu 'world' degiskenine koyar; ekran koordinatini oyun dunyasindaki pozisyona cevirir.
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        world.z = zDepth;
        world.x += Random.Range(-0.15f, 0.15f);
        world.y += Random.Range(-0.05f, 0.05f);

        // Bu satir: 'Instance' uzerindeki 'Spawn' metodunu cagirir ve sonucu 'banknote' degiskenine koyar; pool sisteminden hazir bir obje alip sahneye yerlestirir.
        Banknote banknote = BanknotePool.Instance.Spawn(world, Quaternion.identity);
        if (banknote == null)
        {
            // Bu satir: 'Debug' objesi uzerindeki 'LogError' metodunu cagirir; Unity Console'a hata mesaji yazar; duzeltilmesi gereken ciddi durumlari belirtir.
            Debug.LogError("BanknoteSpawner: pool'dan banknote alinmadi.");
            return;
        }

        // Bu satir: 'BanknoteRegistry' objesi uzerindeki 'Register' metodunu cagirir; parantez icindeki degerler bu metoda bilgi olarak gonderilir.
        BanknoteRegistry.Register(banknote);

        if (FloatingTextPool.Instance != null)
        {
            Vector3 textPos = world + new Vector3(0f, 0.6f, -1f);
            // Bu satir: 'Instance' uzerindeki 'Spawn' metodunu cagirir ve sonucu 'floatingText' degiskenine koyar; pool sisteminden hazir bir obje alip sahneye yerlestirir.
            FloatingText floatingText = FloatingTextPool.Instance.Spawn(textPos, Quaternion.identity);

            if (floatingText != null)
            {
                // Bu satir: 'floatingText' objesi uzerindeki 'Setup' metodunu cagirir; UI kartini veya gorsel objeyi verilen veriyle kullanima hazirlar.
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
                // Bu satir: 'pool' objesi uzerindeki 'Configure' metodunu cagirir; ilgili sistemi/pool'u verilen prefab, sayi ve parent bilgileriyle ayarlar.
                pool.Configure(banknoteComponent, preloadCount, poolObject.transform, false);
                // Bu satir: 'pool' objesi uzerindeki 'ConfigureActiveRoot' metodunu cagirir; aktif objelerin sahnede hangi parent altinda duracagini ayarlar.
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
                // Bu satir: 'pool' objesi uzerindeki 'Configure' metodunu cagirir; ilgili sistemi/pool'u verilen prefab, sayi ve parent bilgileriyle ayarlar.
                pool.Configure(floatingTextComponent, preloadCount, poolObject.transform, false);
                // Bu satir: 'pool' objesi uzerindeki 'ConfigureActiveRoot' metodunu cagirir; aktif objelerin sahnede hangi parent altinda duracagini ayarlar.
                pool.ConfigureActiveRoot(GetOrCreateRuntimeRoot("FloatingTextRuntime"));
            }
        }
    }

    // Bu fonksiyon bir deger hesaplar veya kontrol eder; sonucu cagiran koda geri dondurur.
    private Transform GetOrCreateRuntimeRoot(string objectName)
    {
        // Bu satir: 'GameObject' uzerindeki 'Find' metodunu cagirir ve sonucu 'runtimeRoot' degiskenine koyar; sahnede veya transform altinda verilen isimde obje arar.
        GameObject runtimeRoot = GameObject.Find(objectName);
        if (runtimeRoot == null)
        {
            runtimeRoot = new GameObject(objectName);
        }

        return runtimeRoot.transform;
    }
}
