using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MatchPanelUI : MonoBehaviour
{
    [SerializeField] private float matchDurationSeconds = 300f;
    [SerializeField] private TMP_Text teamAText;
    [SerializeField] private TMP_Text teamBText;
    [SerializeField] private TMP_Text scoreAText;
    [SerializeField] private TMP_Text scoreBText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button startMatchButton;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    private void OnEnable()
    {
        EnsureLeagueManager();
        EnsureMatchManager();
        ResolveReferences();
        WireButton();
        HideResultPanel();
        Refresh();

        if (LeagueManager.Instance != null)
        {
            LeagueManager.Instance.LeagueChanged -= Refresh;
            LeagueManager.Instance.LeagueChanged += Refresh;
        }

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.MatchStateChanged -= Refresh;
            MatchManager.Instance.MatchStateChanged += Refresh;
            MatchManager.Instance.MatchFinished -= ShowResult;
            MatchManager.Instance.MatchFinished += ShowResult;
        }
    }

    private void OnDisable()
    {
        if (LeagueManager.Instance != null)
        {
            LeagueManager.Instance.LeagueChanged -= Refresh;
        }

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.MatchStateChanged -= Refresh;
            MatchManager.Instance.MatchFinished -= ShowResult;
        }

        if (startMatchButton != null)
        {
            startMatchButton.onClick.RemoveListener(StartMatch);
        }
    }

    public void SetMatchDuration(float durationSeconds)
    {
        matchDurationSeconds = Mathf.Max(0.1f, durationSeconds);
    }

    public void Refresh()
    {
        ResolveReferences();

        bool matchIsRunning = MatchManager.Instance != null && MatchManager.Instance.IsMatchRunning;
        bool isInLeague = LeagueManager.Instance != null && LeagueManager.Instance.IsInLeague;
        LeagueRoundResult displayedResult = isInLeague && !matchIsRunning && resultPanel != null && resultPanel.activeSelf
            ? MatchManager.Instance?.LastRoundResult
            : null;
        LeagueMatch match = matchIsRunning
            ? MatchManager.Instance.RunningMatch
            : displayedResult?.UserMatch ?? (isInLeague ? LeagueManager.Instance.NextMatch : null);

        if (!isInLeague && !matchIsRunning)
        {
            ShowMessage("You are not in a League");
        }

        if (teamAText != null)
        {
            teamAText.text = match?.UserTeam != null ? match.UserTeam.Name : "TEAM A";
        }

        if (teamBText != null)
        {
            teamBText.text = match?.OpponentTeam != null ? match.OpponentTeam.Name : "TEAM B";
        }

        if (scoreAText != null)
        {
            scoreAText.text = matchIsRunning ? "-" : displayedResult?.UserMatch != null ? displayedResult.UserMatch.UserGoals.ToString() : "0";
        }

        if (scoreBText != null)
        {
            scoreBText.text = matchIsRunning ? "-" : displayedResult?.UserMatch != null ? displayedResult.UserMatch.OpponentGoals.ToString() : "0";
        }

        if (timerText != null)
        {
            float seconds = matchIsRunning
                ? MatchManager.Instance.RemainingTime
                : matchDurationSeconds;

            timerText.text = FormatTime(seconds);
        }

        if (startMatchButton != null)
        {
            bool canStart = isInLeague && match != null && !matchIsRunning;
            startMatchButton.interactable = canStart;
            SetButtonText(startMatchButton, canStart ? "START" : matchIsRunning ? "Match in progress" : "START");
        }
    }

    private void StartMatch()
    {
        if (LeagueManager.Instance == null || !LeagueManager.Instance.IsInLeague)
        {
            Refresh();
            return;
        }

        HideResultPanel();
        EnsureMatchManager();

        if (MatchManager.Instance != null && MatchManager.Instance.StartScheduledMatch(matchDurationSeconds))
        {
            Refresh();
        }
    }

    private void ShowResult(LeagueRoundResult roundResult)
    {
        if (roundResult?.UserMatch == null)
        {
            return;
        }

        LeagueMatch match = roundResult.UserMatch;

        if (scoreAText != null)
        {
            scoreAText.text = match.UserGoals.ToString();
        }

        if (scoreBText != null)
        {
            scoreBText.text = match.OpponentGoals.ToString();
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultPanel.transform.SetAsLastSibling();
        }

        if (resultText != null)
        {
            resultText.text = GetResultMessage(match);
        }

        Refresh();
    }

    private static string GetResultMessage(LeagueMatch match)
    {
        if (match.UserGoals > match.OpponentGoals)
        {
            return "Congratulations! You won.";
        }

        if (match.UserGoals < match.OpponentGoals)
        {
            return "Unfortunately, you lost.";
        }

        return "The match ended in a draw.";
    }

    private void ResolveReferences()
    {
        if (teamAText == null)
        {
            teamAText = FindText("TEAMA");
        }

        if (teamBText == null)
        {
            teamBText = FindText("TEAMB");
        }

        if (scoreAText == null)
        {
            scoreAText = FindText("ScoreForTeamA");
        }

        if (scoreBText == null)
        {
            scoreBText = FindText("ScoreForTeamB");
        }

        if (timerText == null)
        {
            timerText = FindText("ProgressTimeText");
        }

        if (startMatchButton == null)
        {
            Transform startButtonTransform = UIHelper.FindChildRecursive(transform, "Button");
            if (startButtonTransform == null)
            {
                startButtonTransform = UIHelper.FindChildRecursive(transform, "StartMatchButton");
            }

            if (startButtonTransform != null)
            {
                startMatchButton = startButtonTransform.GetComponent<Button>();
            }
        }

        if (resultPanel == null)
        {
            Transform resultPanelTransform = UIHelper.FindChildRecursive(transform, "Congratulation You Win");
            if (resultPanelTransform == null)
            {
                resultPanelTransform = UIHelper.FindChildRecursive(transform, "ResultPanel");
            }

            if (resultPanelTransform != null)
            {
                resultPanel = resultPanelTransform.gameObject;
            }
        }

        if (resultText == null && resultPanel != null)
        {
            resultText = resultPanel.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void WireButton()
    {
        if (startMatchButton == null)
        {
            return;
        }

        startMatchButton.onClick.RemoveListener(StartMatch);
        startMatchButton.onClick.AddListener(StartMatch);
    }

    private TMP_Text FindText(string childName)
    {
        Transform child = UIHelper.FindChildRecursive(transform, childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    private void HideResultPanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void ShowMessage(string message)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultPanel.transform.SetAsLastSibling();
        }

        if (resultText != null)
        {
            resultText.text = message;
        }
    }

    private static void SetButtonText(Button button, string value)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = value;
        }
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }

    private static void EnsureLeagueManager()
    {
        if (LeagueManager.Instance != null)
        {
            return;
        }

        LeagueManager existing = FindFirstObjectByType<LeagueManager>(FindObjectsInactive.Include);
        if (existing != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("LeagueManager");
        managerObject.AddComponent<LeagueManager>();
    }

    private static void EnsureMatchManager()
    {
        if (MatchManager.Instance != null)
        {
            return;
        }

        MatchManager existing = FindFirstObjectByType<MatchManager>(FindObjectsInactive.Include);
        if (existing != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("MatchManager");
        managerObject.AddComponent<MatchManager>();
    }
}
