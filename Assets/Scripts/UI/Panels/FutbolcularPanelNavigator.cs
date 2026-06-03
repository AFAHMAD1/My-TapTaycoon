using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FutbolcularPanelNavigator : MonoBehaviour
{
    [SerializeField] private GameObject firstPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private List<GameObject> childPanels = new List<GameObject>();
    [SerializeField] private float matchDurationSeconds = 300f;

    private const int RequiredLeaguePlayers = 15;
    private const int RequiredLeagueManagers = 1;

    private readonly Stack<GameObject> panelHistory = new Stack<GameObject>();
    private GameObject currentPanel;
    private Button marketButton;
    private Button clubButton;
    private Button leagueButton;
    private Button matchButton;
    private Button myPlayersButton;
    private Button myManagersButton;
    private GameObject marketPanel;
    private GameObject clubPanel;
    private GameObject leaguePanel;
    private GameObject matchPanel;
    private GameObject myPlayersPanel;
    private GameObject myManagersPanel;
    private GameObject playersInfoPanel;
    private GameObject playersInfoScrollView;
    private GameObject notCompleteTeamErrorPanel;
    private MarketPanelRuntimePopulator marketPopulator;
    private readonly List<Button> marketCategoryButtons = new List<Button>();

    private void Awake()
    {
        ResolveReferences();
        WireButtons();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (currentPanel == null || !currentPanel.activeSelf)
        {
            ShowFirstPanel();
        }

        RefreshBackButton();
    }

    private void OnDestroy()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
        }

        if (marketButton != null)
        {
            marketButton.onClick.RemoveListener(OpenMarketPanel);
        }

        if (clubButton != null)
        {
            clubButton.onClick.RemoveListener(OpenClubPanel);
        }

        if (leagueButton != null)
        {
            leagueButton.onClick.RemoveListener(OpenLeaguePanel);
        }

        if (matchButton != null)
        {
            matchButton.onClick.RemoveListener(OpenMatchPanel);
        }

        if (myPlayersButton != null)
        {
            myPlayersButton.onClick.RemoveListener(OpenMyPlayersPanel);
        }

        if (myManagersButton != null)
        {
            myManagersButton.onClick.RemoveListener(OpenMyManagersPanel);
        }

        foreach (Button button in marketCategoryButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OpenPlayersInfoPanel);
            }
        }
    }

    public void OpenPanel(GameObject targetPanel)
    {
        if (targetPanel == null)
        {
            return;
        }

        EnsureManagedPanel(targetPanel);

        if (currentPanel == targetPanel)
        {
            RefreshBackButton();
            return;
        }

        if (currentPanel == null)
        {
            currentPanel = FindActiveManagedPanel();
        }

        if (currentPanel != null && currentPanel != targetPanel)
        {
            panelHistory.Push(currentPanel);
        }

        ShowOnly(targetPanel);
        currentPanel = targetPanel;
        RefreshBackButton();
    }

    public void OpenPanel(Object targetObject)
    {
        OpenPanel(targetObject as GameObject);
    }

    public void GoBack()
    {
        if (HasOpenClubSubpanel())
        {
            CloseClubSubpanels();
            RefreshBackButton();
            return;
        }

        if (playersInfoPanel != null && playersInfoPanel.activeSelf)
        {
            ClosePlayersInfoPanel();
            RefreshBackButton();
            return;
        }

        if (panelHistory.Count == 0)
        {
            RefreshBackButton();
            return;
        }

        bool leavingLeaguePanel = currentPanel == leaguePanel;
        GameObject previousPanel = panelHistory.Pop();
        if (leavingLeaguePanel)
        {
            ClearLeagueTeamCards();
        }

        ShowOnly(previousPanel);
        currentPanel = previousPanel;
        RefreshBackButton();
    }

    public void ShowFirstPanel()
    {
        panelHistory.Clear();
        ShowOnly(firstPanel);
        currentPanel = firstPanel;
        RefreshBackButton();
    }

    private void ResolveReferences()
    {
        if (firstPanel == null)
        {
            Transform firstPanelTransform = UIHelper.FindChildRecursive(transform, "ButtonsPanel");
            if (firstPanelTransform != null)
            {
                firstPanel = firstPanelTransform.gameObject;
            }
        }

        if (backButton == null)
        {
            backButton = UIHelper.FindButton(transform, "GoBackButton", "BackButton", "GeriButton");
        }

        Transform marketPanelTransform = UIHelper.FindChildRecursive(transform, "MarketPanel");
        if (marketPanelTransform != null)
        {
            marketPanel = marketPanelTransform.gameObject;
            marketPopulator = marketPanel.GetComponent<MarketPanelRuntimePopulator>();
        }

        Transform clubPanelTransform = UIHelper.FindChildRecursive(transform, "KlupPanel");
        if (clubPanelTransform != null)
        {
            clubPanel = clubPanelTransform.gameObject;

            Transform myPlayersPanelTransform = UIHelper.FindChildRecursive(clubPanelTransform, "MyPlayers");
            if (myPlayersPanelTransform != null)
            {
                myPlayersPanel = myPlayersPanelTransform.gameObject;
            }

            Transform myManagersPanelTransform = UIHelper.FindChildRecursive(clubPanelTransform, "Managers");
            if (myManagersPanelTransform == null)
            {
                myManagersPanelTransform = UIHelper.FindChildRecursive(clubPanelTransform, "MyManagers");
            }

            if (myManagersPanelTransform == null)
            {
                myManagersPanelTransform = UIHelper.FindChildRecursive(clubPanelTransform, "MyManagersPanel");
            }

            if (myManagersPanelTransform != null)
            {
                myManagersPanel = myManagersPanelTransform.gameObject;
            }
        }

        Transform leaguePanelTransform = UIHelper.FindChildRecursive(transform, "LeaguePanel");
        if (leaguePanelTransform != null)
        {
            leaguePanel = leaguePanelTransform.gameObject;

            Transform notCompleteTeamErrorTransform = UIHelper.FindChildRecursive(leaguePanelTransform, "Not_Complete_Team_Error");
            if (notCompleteTeamErrorTransform != null)
            {
                notCompleteTeamErrorPanel = notCompleteTeamErrorTransform.gameObject;
            }
        }

        Transform matchPanelTransform = UIHelper.FindChildRecursive(transform, "MatchPanel");
        if (matchPanelTransform != null)
        {
            matchPanel = matchPanelTransform.gameObject;
        }

        Transform marketButtonTransform = UIHelper.FindChildRecursive(transform, "Marketbtn");
        if (marketButtonTransform != null)
        {
            marketButton = marketButtonTransform.GetComponent<Button>();
        }

        Transform clubButtonTransform = UIHelper.FindChildRecursive(transform, "klup");
        if (clubButtonTransform != null)
        {
            clubButton = clubButtonTransform.GetComponent<Button>();
        }

        Transform leagueButtonTransform = UIHelper.FindChildRecursive(transform, "league");
        if (leagueButtonTransform == null)
        {
            leagueButtonTransform = UIHelper.FindChildRecursive(transform, "LeagueBtn");
        }

        if (leagueButtonTransform == null)
        {
            leagueButtonTransform = UIHelper.FindChildRecursive(transform, "LeagueButton");
        }

        if (leagueButtonTransform != null)
        {
            leagueButton = leagueButtonTransform.GetComponent<Button>();
        }

        Transform matchButtonTransform = UIHelper.FindChildRecursive(transform, "MatchBtn");
        if (matchButtonTransform == null)
        {
            matchButtonTransform = UIHelper.FindChildRecursive(transform, "MatchButton");
        }

        if (matchButtonTransform == null)
        {
            matchButtonTransform = UIHelper.FindChildRecursive(transform, "Match");
        }

        if (matchButtonTransform != null)
        {
            matchButton = matchButtonTransform.GetComponent<Button>();
        }

        Transform myPlayersButtonTransform = UIHelper.FindChildRecursive(transform, "MyPlayersButton");
        if (myPlayersButtonTransform != null)
        {
            myPlayersButton = myPlayersButtonTransform.GetComponent<Button>();
        }

        Transform myManagersButtonTransform = UIHelper.FindChildRecursive(transform, "MyManagersButton");
        if (myManagersButtonTransform != null)
        {
            myManagersButton = myManagersButtonTransform.GetComponent<Button>();
        }

        Transform playersInfoPanelTransform = UIHelper.FindChildRecursive(transform, "Players_info_panel");
        if (playersInfoPanelTransform != null)
        {
            playersInfoPanel = playersInfoPanelTransform.gameObject;

            Transform scrollViewTransform = UIHelper.FindChildRecursive(playersInfoPanelTransform, "Scroll View");
            if (scrollViewTransform != null)
            {
                playersInfoScrollView = scrollViewTransform.gameObject;
            }
        }

        EnsureManagedPanel(firstPanel);
        EnsureManagedPanel(marketPanel);
        EnsureManagedPanel(clubPanel);
        EnsureManagedPanel(leaguePanel);
        EnsureManagedPanel(matchPanel);
    }

    private void WireButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
            backButton.onClick.AddListener(GoBack);
        }

        if (marketButton != null)
        {
            marketButton.onClick.RemoveListener(OpenMarketPanel);
            marketButton.onClick.AddListener(OpenMarketPanel);
        }

        if (clubButton != null)
        {
            clubButton.onClick.RemoveListener(OpenClubPanel);
            clubButton.onClick.AddListener(OpenClubPanel);
        }

        if (leagueButton != null)
        {
            leagueButton.onClick.RemoveListener(OpenLeaguePanel);
            leagueButton.onClick.AddListener(OpenLeaguePanel);
        }

        if (matchButton != null)
        {
            matchButton.onClick.RemoveListener(OpenMatchPanel);
            matchButton.onClick.AddListener(OpenMatchPanel);
        }

        if (myPlayersButton != null)
        {
            myPlayersButton.onClick.RemoveListener(OpenMyPlayersPanel);
            myPlayersButton.onClick.AddListener(OpenMyPlayersPanel);
        }

        if (myManagersButton != null)
        {
            myManagersButton.onClick.RemoveListener(OpenMyManagersPanel);
            myManagersButton.onClick.AddListener(OpenMyManagersPanel);
        }

        WireMarketCategoryButton("ForwardBtn", MarketPanelRuntimePopulator.MarketCategory.Forwards);
        WireMarketCategoryButton("MidfieldBtn", MarketPanelRuntimePopulator.MarketCategory.Midfielders);
        WireMarketCategoryButton("DefenceBtn", MarketPanelRuntimePopulator.MarketCategory.Defenders);
        WireMarketCategoryButton("Managers and Goal keepers Btn", MarketPanelRuntimePopulator.MarketCategory.ManagersAndGoalkeepers);
    }

    private void OpenMarketPanel()
    {
        OpenPanel(marketPanel);
    }

    private void OpenClubPanel()
    {
        OpenPanel(clubPanel);
        CloseClubSubpanels();
        RefreshBackButton();
    }

    private void OpenLeaguePanel()
    {
        if (!CanEnterLeague())
        {
            OpenPanel(leaguePanel);
            ClearLeagueTeamCards();
            ShowNotCompleteTeamErrorPanel();
            return;
        }

        EnsureLeagueManager();
        if (LeagueManager.Instance != null)
        {
            LeagueManager.Instance.EnterLeague();
        }

        OpenPanel(leaguePanel);
        HideNotCompleteTeamErrorPanel();

        if (leaguePanel != null)
        {
            LeagueTeamsPanelPopulator populator = leaguePanel.GetComponent<LeagueTeamsPanelPopulator>();
            if (populator == null)
            {
                populator = leaguePanel.AddComponent<LeagueTeamsPanelPopulator>();
            }

            LeaguePanelUI leaguePanelUI = leaguePanel.GetComponent<LeaguePanelUI>();
            if (leaguePanelUI == null)
            {
                leaguePanelUI = leaguePanel.AddComponent<LeaguePanelUI>();
            }

            populator.Populate();
            leaguePanelUI.Refresh();
        }
    }

    private void OpenMatchPanel()
    {
        OpenPanel(matchPanel);

        if (matchPanel == null)
        {
            return;
        }

        MatchPanelUI matchPanelUI = matchPanel.GetComponent<MatchPanelUI>();
        if (matchPanelUI == null)
        {
            matchPanelUI = matchPanel.AddComponent<MatchPanelUI>();
        }

        matchPanelUI.SetMatchDuration(matchDurationSeconds);
        matchPanelUI.Refresh();
    }

    private bool CanEnterLeague()
    {
        if (LeagueManager.Instance != null && LeagueManager.Instance.IsInLeague)
        {
            return true;
        }

        if (marketPopulator == null)
        {
            ResolveReferences();
        }

        int playerCount = marketPopulator != null ? marketPopulator.OwnedPlayerCount : 0;
        int managerCount = marketPopulator != null ? marketPopulator.OwnedManagerCount : 0;

        if (playerCount >= RequiredLeaguePlayers && managerCount >= RequiredLeagueManagers)
        {
            return true;
        }

        string message = $"League'e girmek icin en az {RequiredLeaguePlayers} oyuncu ve {RequiredLeagueManagers} menajer gerekir. Su an: {playerCount} oyuncu, {managerCount} menajer.";
        Debug.LogError($"[League] {message}", this);
        return false;
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

    private void ShowNotCompleteTeamErrorPanel()
    {
        if (notCompleteTeamErrorPanel == null)
        {
            Debug.LogWarning("[League] Not_Complete_Team_Error panel bulunamadi.", this);
            return;
        }

        notCompleteTeamErrorPanel.SetActive(true);
        notCompleteTeamErrorPanel.transform.SetAsLastSibling();
    }

    private void HideNotCompleteTeamErrorPanel()
    {
        if (notCompleteTeamErrorPanel != null)
        {
            notCompleteTeamErrorPanel.SetActive(false);
        }
    }

    private void ClearLeagueTeamCards()
    {
        if (leaguePanel == null)
        {
            return;
        }

        LeagueTeamsPanelPopulator populator = leaguePanel.GetComponent<LeagueTeamsPanelPopulator>();
        if (populator != null)
        {
            populator.ClearCards();
        }

        HideNotCompleteTeamErrorPanel();
    }

    private void OpenMyPlayersPanel()
    {
        ShowClubSubpanel(myPlayersPanel);
    }

    private void OpenMyManagersPanel()
    {
        ShowClubSubpanel(myManagersPanel);
    }

    private void ShowClubSubpanel(GameObject targetPanel)
    {
        if (targetPanel == null)
        {
            return;
        }

        if (clubPanel != null && !clubPanel.activeSelf)
        {
            OpenPanel(clubPanel);
        }

        if (myPlayersPanel != null)
        {
            myPlayersPanel.SetActive(myPlayersPanel == targetPanel);
        }

        if (myManagersPanel != null)
        {
            myManagersPanel.SetActive(myManagersPanel == targetPanel);
        }

        targetPanel.transform.SetAsLastSibling();
        RefreshBackButton();
    }

    public void OpenPlayersInfoPanel()
    {
        OpenPlayersInfoPanel(null);
    }

    private void OpenPlayersInfoPanel(MarketPanelRuntimePopulator.MarketCategory? category)
    {
        if (playersInfoPanel == null)
        {
            return;
        }

        if (category.HasValue && marketPopulator != null)
        {
            marketPopulator.Populate(category.Value);
        }

        playersInfoPanel.SetActive(true);

        if (playersInfoScrollView != null)
        {
            playersInfoScrollView.SetActive(true);
        }

        if (playersInfoPanel.transform.parent != null)
        {
            playersInfoPanel.transform.SetAsLastSibling();
        }

        RefreshBackButton();
    }

    public void ClosePlayersInfoPanel()
    {
        if (marketPopulator != null)
        {
            marketPopulator.ClearMarketCards();
        }

        if (playersInfoScrollView != null)
        {
            playersInfoScrollView.SetActive(false);
        }

        if (playersInfoPanel != null)
        {
            playersInfoPanel.SetActive(false);
        }
    }

    private void ShowOnly(GameObject targetPanel)
    {
        ClosePlayersInfoPanel();

        if (targetPanel != clubPanel)
        {
            CloseClubSubpanels();
        }

        foreach (GameObject panel in childPanels)
        {
            if (panel != null)
            {
                panel.SetActive(panel == targetPanel);
            }
        }
    }

    private GameObject FindActiveManagedPanel()
    {
        foreach (GameObject panel in childPanels)
        {
            if (panel != null && panel.activeSelf)
            {
                return panel;
            }
        }

        return firstPanel;
    }

    private void EnsureManagedPanel(GameObject panel)
    {
        if (panel != null && !childPanels.Contains(panel))
        {
            childPanels.Add(panel);
        }
    }

    private void RefreshBackButton()
    {
        if (backButton != null)
        {
            bool hasOpenSharedPanel = playersInfoPanel != null && playersInfoPanel.activeSelf;
            backButton.interactable = hasOpenSharedPanel || HasOpenClubSubpanel() || panelHistory.Count > 0;
        }
    }

    private bool HasOpenClubSubpanel()
    {
        if (clubPanel != null && !clubPanel.activeInHierarchy)
        {
            return false;
        }

        return IsOpenClubSubpanel(myPlayersPanel) || IsOpenClubSubpanel(myManagersPanel);
    }

    private bool IsOpenClubSubpanel(GameObject panel)
    {
        return panel != null && panel.activeSelf;
    }

    private void CloseClubSubpanels()
    {
        if (myPlayersPanel != null)
        {
            myPlayersPanel.SetActive(false);
        }

        if (myManagersPanel != null)
        {
            myManagersPanel.SetActive(false);
        }
    }

    private void WireMarketCategoryButton(string buttonName, MarketPanelRuntimePopulator.MarketCategory category)
    {
        Transform buttonTransform = UIHelper.FindChildRecursive(transform, buttonName);
        if (buttonTransform == null)
        {
            return;
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(OpenPlayersInfoPanel);
        button.onClick.AddListener(() => OpenPlayersInfoPanel(category));

        if (!marketCategoryButtons.Contains(button))
        {
            marketCategoryButtons.Add(button);
        }
    }
}
