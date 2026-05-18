using UnityEngine;
using UnityEngine.UI;

public class OtomaticBasButton : MonoBehaviour, ISaveable
{
    public const int MaxLevel = 5;

    public static OtomaticBasButton Instance { get; private set; }
    public static int CurrentLevel { get; private set; }
    public static bool IsUnlocked => CurrentLevel > 0;

    [SerializeField] private Button button;
    [SerializeField] private SkillId buttonSlotSkillId = SkillId.None;
    [SerializeField] private float baseTapsPerSecond = 2f;
    [SerializeField] private float tapsPerLevelBonus = 1f;
    [SerializeField] private float baseDurationSeconds = 30f;
    [SerializeField] private float durationPerLevelBonus = 5f;
    [SerializeField] private float cooldownSeconds = 600f;
    [SerializeField] private float horizontalScreenRandomness = 0.12f;
    [SerializeField] private float verticalScreenRandomness = 0.12f;

    private bool isActive;
    private float activeTimer;
    private float cooldownTimer;
    private float tapAccumulator;

    private void Awake()
    {
        if (!IsIntendedButton())
        {
            enabled = false;
            return;
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            enabled = false;
            return;
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        RefreshButtonState();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(UseOtomaticBas);
            RefreshButtonState();
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(UseOtomaticBas);
        }

        isActive = false;
        tapAccumulator = 0f;
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                RefreshButtonState();
            }
        }

        if (!isActive)
        {
            return;
        }

        activeTimer -= Time.deltaTime;
        if (activeTimer <= 0f)
        {
            isActive = false;
            tapAccumulator = 0f;
            RefreshButtonState();
            return;
        }

        tapAccumulator += Time.deltaTime * GetCurrentTapsPerSecond();
        while (tapAccumulator >= 1f)
        {
            PerformAutoTap();
            tapAccumulator -= 1f;
        }
    }

    public static bool UpgradeLevel()
    {
        if (CurrentLevel >= MaxLevel)
        {
            return false;
        }

        CurrentLevel++;
        if (Instance != null)
        {
            Instance.RefreshButtonState();
        }

        return true;
    }

    private void UseOtomaticBas()
    {
        if (!IsUnlocked || isActive || cooldownTimer > 0f || BanknoteSpawner.Instance == null)
        {
            return;
        }

        isActive = true;
        activeTimer = GetCurrentDurationSeconds();
        cooldownTimer = cooldownSeconds;
        tapAccumulator = 0f;
        RefreshButtonState();

        Debug.Log($"[Otomatic Bas] Auto tap started: {GetCurrentTapsPerSecond():0.##} taps/s for {activeTimer:0}s. Cooldown: {cooldownSeconds:0}s.");
    }

    private void PerformAutoTap()
    {
        if (BanknoteSpawner.Instance == null)
        {
            return;
        }

        float xOffset = Random.Range(-Screen.width * horizontalScreenRandomness, Screen.width * horizontalScreenRandomness);
        float yOffset = Random.Range(-Screen.height * verticalScreenRandomness, Screen.height * verticalScreenRandomness);
        Vector2 screenPosition = new Vector2(Screen.width * 0.5f + xOffset, Screen.height * 0.5f + yOffset);
        BanknoteSpawner.Instance.PerformTapAtScreenPosition(screenPosition);
    }

    private void RefreshButtonState()
    {
        if (button != null)
        {
            button.interactable = IsUnlocked && !isActive && cooldownTimer <= 0f;
        }
    }

    private bool IsIntendedButton()
    {
        return buttonSlotSkillId == SkillId.OtomaticBas;
    }

    private float GetCurrentTapsPerSecond()
    {
        return baseTapsPerSecond + Mathf.Max(0, CurrentLevel - 1) * tapsPerLevelBonus;
    }

    private float GetCurrentDurationSeconds()
    {
        return baseDurationSeconds + Mathf.Max(0, CurrentLevel - 1) * durationPerLevelBonus;
    }

    public void OnSave(SaveData data)
    {
        if (!IsIntendedButton())
        {
            return;
        }

        data.otomaticBasUnlocked = IsUnlocked;
        data.otomaticBasLevel = CurrentLevel;
    }

    public void OnLoad(SaveData data)
    {
        if (!IsIntendedButton())
        {
            return;
        }

        CurrentLevel = Mathf.Clamp(data.otomaticBasLevel, 0, MaxLevel);
        if (CurrentLevel == 0 && data.otomaticBasUnlocked)
        {
            CurrentLevel = 1;
        }

        RefreshButtonState();
    }
}
