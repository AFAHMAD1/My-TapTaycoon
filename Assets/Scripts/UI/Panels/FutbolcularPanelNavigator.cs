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
    private GameObject marketPanel;

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
        }

        Transform marketButtonTransform = UIHelper.FindChildRecursive(transform, "Marketbtn");
        if (marketButtonTransform != null)
        {
            marketButton = marketButtonTransform.GetComponent<Button>();
        }

        EnsureManagedPanel(firstPanel);
        EnsureManagedPanel(marketPanel);
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
    }

    private void OpenMarketPanel()
    {
        OpenPanel(marketPanel);
    }

    private void ShowOnly(GameObject targetPanel)
    {
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
            backButton.interactable = panelHistory.Count > 0;
        }
    }
}
