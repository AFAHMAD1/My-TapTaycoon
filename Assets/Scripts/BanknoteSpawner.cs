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

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        EnsurePools();
    }

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
