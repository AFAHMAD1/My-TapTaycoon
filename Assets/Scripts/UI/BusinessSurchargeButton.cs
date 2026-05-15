using UnityEngine;
using UnityEngine.UI;

public class BusinessSurchargeButton : MonoBehaviour, ISaveable
{
    public const int MaxLevel = 10;

    public static BusinessSurchargeButton Instance { get; private set; }
    public static int CurrentLevel { get; private set; }
    public static bool IsUnlocked => CurrentLevel > 0;

    [SerializeField] private Button button;
    [SerializeField] private SkillId buttonSlotSkillId = SkillId.None;
    [SerializeField] private float incomeMultiplier = 10f;
    [SerializeField] private float boostDurationSeconds = 30f;
    [SerializeField] private float cooldownSeconds = 60f;

    private float boostTimer;
    private float cooldownTimer;
    private bool isBoostActive;

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
            button.onClick.AddListener(UseBusinessSurcharge);
            RefreshButtonState();
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(UseBusinessSurcharge);
        }

        if (isBoostActive && UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ResetBusinessSurchargeMultiplier();
        }
    }

    private void Update()
    {
        if (isBoostActive)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                isBoostActive = false;
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.ResetBusinessSurchargeMultiplier();
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

    private void UseBusinessSurcharge()
    {
        if (!IsUnlocked || UpgradeManager.Instance == null || cooldownTimer > 0f)
        {
            return;
        }

        float currentMultiplier = GetCurrentMultiplier();
        UpgradeManager.Instance.SetBusinessSurchargeMultiplier(currentMultiplier);
        isBoostActive = true;
        boostTimer = boostDurationSeconds;
        cooldownTimer = cooldownSeconds;
        RefreshButtonState();

        Debug.Log($"[Business Surcharge] Owned building income x{currentMultiplier:0.##} for {boostDurationSeconds:0}s.");
    }

    private void RefreshButtonState()
    {
        if (button != null)
        {
            button.interactable = IsUnlocked && cooldownTimer <= 0f;
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

    private bool IsIntendedButton()
    {
        return buttonSlotSkillId == SkillId.BusinessSurcharge;
    }

    private float GetCurrentMultiplier()
    {
        return incomeMultiplier + Mathf.Max(0, CurrentLevel - 1);
    }

    public void OnSave(SaveData data)
    {
        if (!IsIntendedButton())
        {
            return;
        }

        data.businessSurchargeUnlocked = IsUnlocked;
        data.businessSurchargeLevel = CurrentLevel;
    }

    public void OnLoad(SaveData data)
    {
        if (!IsIntendedButton())
        {
            return;
        }

        CurrentLevel = Mathf.Clamp(data.businessSurchargeLevel, 0, MaxLevel);
        if (CurrentLevel == 0 && data.businessSurchargeUnlocked)
        {
            CurrentLevel = 1;
        }

        RefreshButtonState();
    }
}
