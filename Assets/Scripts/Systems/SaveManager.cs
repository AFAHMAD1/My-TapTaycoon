using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int saveVersion = 1;
    public double currentMoney;
    public List<int> buildingLevels = new List<int>();
    public int collectorLevel;
    public List<int> boostLevels = new List<int>();
    public List<int> playerProfitLevels = new List<int>();
    public List<int> skillLevels = new List<int>();
    public bool quickCashUnlocked;
    public int quickCashLevel;
    public bool businessSurchargeUnlocked;
    public int businessSurchargeLevel;
    public bool otomaticBasUnlocked;
    public int otomaticBasLevel;
    public string lastSaveTime;
}

/// <summary>
/// Saves and loads game state, then delegates system-specific state to ISaveable components.
/// </summary>
public class SaveManager : MonoBehaviour, ISaveManager
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;

    private readonly List<ISaveable> _saveables = new List<ISaveable>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        CollectSaveables();
        LoadGame();

        if (autoSave)
        {
            InvokeRepeating(nameof(SaveGame), autoSaveInterval, autoSaveInterval);
        }
    }

    private void CollectSaveables()
    {
        _saveables.Clear();
        var allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in allBehaviours)
        {
            if (mb is ISaveable saveable)
            {
                _saveables.Add(saveable);
            }
        }

        Debug.Log($"[SaveManager] {_saveables.Count} ISaveable components found.");
    }

    private void OnApplicationQuit() => SaveGame();
    private void OnApplicationPause(bool pause) { if (pause) SaveGame(); }
    private void OnApplicationFocus(bool hasFocus) { if (!hasFocus) SaveGame(); }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        SaveData data = new SaveData();

        foreach (var saveable in _saveables)
        {
            saveable.OnSave(data);
        }

        data.lastSaveTime = DateTime.UtcNow.ToString("O");

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GameSave", json);
        PlayerPrefs.Save();

        Debug.Log("[SaveManager] Game saved.");
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("GameSave"))
        {
            Debug.Log("[SaveManager] No save file found.");
            return;
        }

        string json = PlayerPrefs.GetString("GameSave");
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
        {
            Debug.LogWarning("[SaveManager] Save file is corrupt or unreadable.");
            return;
        }

        foreach (var saveable in _saveables)
        {
            saveable.OnLoad(data);
        }

        CalculateOfflineEarnings(data.lastSaveTime);

        if (PassiveIncomeManager.Instance != null)
        {
            foreach (var building in PassiveIncomeManager.Instance.buildings)
            {
                building.InitializeVisualState();
            }
        }

        Debug.Log("[SaveManager] Data loaded.");
    }

    private void CalculateOfflineEarnings(string lastSaveTimeString)
    {
        if (string.IsNullOrEmpty(lastSaveTimeString)) return;

        if (DateTime.TryParse(
            lastSaveTimeString,
            null,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out DateTime lastSaveTime))
        {
            double secondsOffline = (DateTime.UtcNow - lastSaveTime).TotalSeconds;
            if (secondsOffline <= 60) return;

            double incomePerSecond = PassiveIncomeManager.Instance != null
                ? PassiveIncomeManager.Instance.GetTotalPassiveIncomePerSecond()
                : 0d;

            double totalOfflineEarnings = incomePerSecond * secondsOffline;
            if (totalOfflineEarnings > 0 && CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddMoney(totalOfflineEarnings, false);
                Debug.Log($"[SaveManager] Offline income: {NumberFormatter.Format(totalOfflineEarnings)} ({secondsOffline:F0}s).");
            }
        }
    }

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("GameSave");
        Debug.Log("[SaveManager] Progress reset. Restart the game.");
    }
}
