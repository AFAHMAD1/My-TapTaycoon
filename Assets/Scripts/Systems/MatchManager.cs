using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    public event Action MatchStateChanged;
    public event Action<LeagueRoundResult> MatchFinished;

    public bool IsMatchRunning { get; private set; }
    public float RemainingTime { get; private set; }
    public float CurrentMatchDuration { get; private set; }
    public LeagueMatch RunningMatch { get; private set; }
    public LeagueRoundResult LastRoundResult { get; private set; }

    private Coroutine matchCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool StartScheduledMatch(float durationSeconds)
    {
        if (IsMatchRunning || LeagueManager.Instance == null || !LeagueManager.Instance.IsInLeague)
        {
            return false;
        }

        LeagueMatch nextMatch = LeagueManager.Instance.NextMatch;
        if (nextMatch == null)
        {
            return false;
        }

        CurrentMatchDuration = Mathf.Max(0.1f, durationSeconds);
        RemainingTime = CurrentMatchDuration;
        RunningMatch = nextMatch;
        LastRoundResult = null;
        IsMatchRunning = true;

        if (matchCoroutine != null)
        {
            StopCoroutine(matchCoroutine);
        }

        matchCoroutine = StartCoroutine(RunMatchTimer());
        MatchStateChanged?.Invoke();
        return true;
    }

    private IEnumerator RunMatchTimer()
    {
        while (RemainingTime > 0f)
        {
            RemainingTime = Mathf.Max(0f, RemainingTime - Time.deltaTime);
            MatchStateChanged?.Invoke();
            yield return null;
        }

        FinishRunningMatch();
    }

    private void FinishRunningMatch()
    {
        if (!IsMatchRunning || LeagueManager.Instance == null || !LeagueManager.Instance.IsInLeague)
        {
            return;
        }

        LeagueRoundResult result = LeagueManager.Instance.CompleteCurrentRoundWithGeneratedResult();

        LastRoundResult = result;
        IsMatchRunning = false;
        RemainingTime = 0f;
        RunningMatch = null;
        matchCoroutine = null;

        MatchStateChanged?.Invoke();
        MatchFinished?.Invoke(result);
    }
}
