using UnityEngine;
using UnityEngine.UI;

public class HandOfMidasButton : MonoBehaviour, ISaveable
{
    public const int MaxLevel = 5;

    public static HandOfMidasButton Instance { get; private set; }
    public static int CurrentLevel { get; private set; }
    public static bool IsUnlocked => CurrentLevel > 0;

    [SerializeField] private Button button;
    [SerializeField] private SkillId buttonSlotSkillId = SkillId.None;
    [SerializeField] private float tapMultiplier = 10f;
    [SerializeField] private float multiplierPerLevelBonus = 2f;
    [SerializeField] private float baseDurationSeconds = 30f;
    [SerializeField] private float durationPerLevelBonus = 5f;
    [SerializeField] private float cooldownSeconds = 900f;

    private bool isActive;
    private float activeTimer;
    private float cooldownTimer;

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
            button.onClick.AddListener(UseHandOfMidas);
            RefreshButtonState();
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(UseHandOfMidas);
        }

        if (isActive && UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ResetSkillClickMultiplier();
        }

        isActive = false;
    }

    private void Update()
    {
        if (isActive)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                isActive = false;
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.ResetSkillClickMultiplier();
                }
            }
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                RefreshButtonState();
            }
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

    private void UseHandOfMidas()
    {
        if (!IsUnlocked || isActive || cooldownTimer > 0f || UpgradeManager.Instance == null)
        {
            return;
        }

        float currentMultiplier = GetCurrentMultiplier();
        float duration = GetCurrentDurationSeconds();

        UpgradeManager.Instance.SetSkillClickMultiplier(currentMultiplier);
        isActive = true;
        activeTimer = duration;
        cooldownTimer = cooldownSeconds;
        RefreshButtonState();

        Debug.Log($"[Hand of Midas] Tap income x{currentMultiplier:0.##} for {duration:0}s.");
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
        return buttonSlotSkillId == SkillId.HandOfMidas;
    }

    private float GetCurrentMultiplier()
    {
        return tapMultiplier + Mathf.Max(0, CurrentLevel - 1) * multiplierPerLevelBonus;
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

        data.handOfMidasUnlocked = IsUnlocked;
        data.handOfMidasLevel = CurrentLevel;
    }

    public void OnLoad(SaveData data)
    {
        if (!IsIntendedButton())
        {
            return;
        }

        CurrentLevel = Mathf.Clamp(data.handOfMidasLevel, 0, MaxLevel);
        if (CurrentLevel == 0 && data.handOfMidasUnlocked)
        {
            CurrentLevel = 1;
        }

        RefreshButtonState();
    }
}
