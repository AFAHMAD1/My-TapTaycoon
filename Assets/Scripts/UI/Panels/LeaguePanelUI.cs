using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class LeaguePanelUI : MonoBehaviour
{
    [SerializeField] private LeagueTeamsPanelPopulator tablePopulator;
    [SerializeField] private TMP_Text leagueNameText;

    private void OnEnable()
    {
        EnsureLeagueManager();
        ResolveReferences();
        Refresh();

        if (LeagueManager.Instance != null)
        {
            LeagueManager.Instance.LeagueChanged -= Refresh;
            LeagueManager.Instance.LeagueChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (LeagueManager.Instance != null)
        {
            LeagueManager.Instance.LeagueChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (LeagueManager.Instance == null)
        {
            return;
        }

        if (leagueNameText != null)
        {
            leagueNameText.text = $"League {LeagueManager.Instance.CurrentLeagueName}";
        }

        if (tablePopulator != null)
        {
            tablePopulator.Populate();
        }
    }

    private void ResolveReferences()
    {
        if (tablePopulator == null)
        {
            tablePopulator = GetComponent<LeagueTeamsPanelPopulator>();
            if (tablePopulator == null)
            {
                tablePopulator = gameObject.AddComponent<LeagueTeamsPanelPopulator>();
            }
        }

        if (leagueNameText == null)
        {
            Transform leagueName = UIHelper.FindChildRecursive(transform, "LeagueName");
            if (leagueName != null)
            {
                leagueNameText = leagueName.GetComponent<TMP_Text>();
            }
        }
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
}
