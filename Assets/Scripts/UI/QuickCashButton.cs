using UnityEngine;
using UnityEngine.UI;

public class QuickCashButton : MonoBehaviour, ISaveable
{
    public const int MaxLevel = 10;

    public static QuickCashButton Instance { get; private set; }
    public static int CurrentLevel { get; private set; }
    public static bool IsUnlocked => CurrentLevel > 0;

    [SerializeField] private Button button;
    [SerializeField] private SkillId buttonSlotSkillId = SkillId.None;
    [SerializeField] private float minBalancePercent = 0.10f;
    [SerializeField] private float maxBalancePercent = 0.20f;
    [SerializeField] private float cooldownSeconds = 15f;

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

    private bool IsIntendedButton()
    {
        return buttonSlotSkillId == SkillId.QuickCash;
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(UseQuickCash);
            RefreshButtonState();
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(UseQuickCash);
        }
    }

    private void Update()
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            RefreshButtonState();
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

    private void UseQuickCash()
    {
        if (!IsUnlocked)
        {
            return;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[QuickCash] CurrencyManager bulunamadi.");
            return;
        }

        double balance = CurrencyManager.Instance.currentMoney;
        float levelBonus = Mathf.Max(0, CurrentLevel - 1) * 0.01f;
        float percent = Random.Range(minBalancePercent + levelBonus, maxBalancePercent + levelBonus);
        double reward = balance * percent;

        CurrencyManager.Instance.AddMoney(reward, false);
        Debug.Log($"{NumberFormatter.Format(reward)} Recieved");

        cooldownTimer = cooldownSeconds;
        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        if (button != null)
        {
            button.interactable = IsUnlocked && cooldownTimer <= 0f;
        }
    }

    public void OnSave(SaveData data)
    {
        if (!IsIntendedButton())
        {
            return;
        }

        data.quickCashUnlocked = IsUnlocked;
        data.quickCashLevel = CurrentLevel;
    }

    public void OnLoad(SaveData data)
    {
        if (!IsIntendedButton())
        {
            return;
        }

        CurrentLevel = Mathf.Clamp(data.quickCashLevel, 0, MaxLevel);
        if (CurrentLevel == 0 && data.quickCashUnlocked)
        {
            CurrentLevel = 1;
        }

        RefreshButtonState();
    }
}
