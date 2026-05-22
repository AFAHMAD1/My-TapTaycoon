using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FutbolcularPanelNavigator : MonoBehaviour
{
    [SerializeField] private GameObject firstPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private List<GameObject> childPanels = new List<GameObject>();

    private readonly Stack<GameObject> panelHistory = new Stack<GameObject>();
    private GameObject currentPanel;
    private Button marketButton;
    private Button clubButton;
    private Button myPlayersButton;
    private Button myManagersButton;
    private GameObject marketPanel;
    private GameObject clubPanel;
    private GameObject myPlayersPanel;
    private GameObject myManagersPanel;
    private GameObject playersInfoPanel;
    private GameObject playersInfoScrollView;
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

        GameObject previousPanel = panelHistory.Pop();
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
