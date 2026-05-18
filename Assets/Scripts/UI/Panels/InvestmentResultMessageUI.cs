using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class InvestmentResultMessageUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text companyResultText;
    [SerializeField] private TMP_Text profitLossText;
    [SerializeField] private float displaySeconds = 5f;

    private readonly Queue<ResultMessage> pendingMessages = new Queue<ResultMessage>();
    private InvestmentManager investmentManager;
    private Coroutine displayRoutine;

    private void Awake()
    {
        ResolveReferences();
        HidePanel();
    }

    private void OnDestroy()
    {
        if (investmentManager != null)
        {
            investmentManager.InvestmentResolved -= EnqueueResult;
        }
    }

    public void Configure(InvestmentManager manager)
    {
        if (investmentManager != null)
        {
            investmentManager.InvestmentResolved -= EnqueueResult;
        }

        investmentManager = manager;

        if (investmentManager != null)
        {
            investmentManager.InvestmentResolved += EnqueueResult;
        }

        ResolveReferences();
        HidePanel();
    }

    private void EnqueueResult(InvestmentResult result)
    {
        pendingMessages.Enqueue(new ResultMessage(result.CompanyName, result.Success, result.ReceivedAmount));

        if (displayRoutine == null)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            displayRoutine = StartCoroutine(DisplayQueue());
        }
    }

    private IEnumerator DisplayQueue()
    {
        while (pendingMessages.Count > 0)
        {
            ResultMessage message = pendingMessages.Dequeue();
            ShowMessage(message);
            yield return new WaitForSeconds(displaySeconds);
            HidePanel();
        }

        displayRoutine = null;
    }

    private void ShowMessage(ResultMessage message)
    {
        ResolveReferences();

        if (resultPanel == null)
        {
            Debug.LogWarning("[InvestmentResultMessageUI] ResultMessage panel bulunamadi.", this);
            return;
        }

        PreparePanelForDisplay();
        resultPanel.SetActive(true);

        if (companyResultText != null)
        {
            companyResultText.text = $"({message.CompanyName}) alinan miktar: {NumberFormatter.FormatPrice(message.ReceivedAmount)}";
        }

        if (profitLossText != null)
        {
            profitLossText.text = message.Success ? "[OK] Kar\nZarar" : "Kar\n[OK] Zarar";
        }
    }

    private void HidePanel()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void PreparePanelForDisplay()
    {
        resultPanel.transform.localScale = Vector3.one;

        if (resultPanel.TryGetComponent(out RectTransform rectTransform))
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }

        if (resultPanel.TryGetComponent(out Canvas canvas))
        {
            canvas.enabled = true;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5000;
        }

        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = resultPanel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        resultPanel.transform.SetAsLastSibling();
    }

    private void ResolveReferences()
    {
        if (resultPanel == null)
        {
            resultPanel = FindLoadedGameObjectByName("ResultMessage");
        }

        if (resultPanel == null)
        {
            return;
        }

        if (companyResultText == null)
        {
            companyResultText = FindText(resultPanel.transform, "Yat\u0131r\u0131m Sonucu");
        }

        if (profitLossText == null)
        {
            profitLossText = FindText(resultPanel.transform, "Sonu\u00e7 A\u00e7\u0131klanmas\u0131");
        }
    }

    private static GameObject FindLoadedGameObjectByName(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform candidate in transforms)
        {
            if (candidate.hideFlags != HideFlags.None)
            {
                continue;
            }

            if (candidate.gameObject.scene.IsValid() &&
                candidate.name.Equals(objectName, System.StringComparison.OrdinalIgnoreCase))
            {
                return candidate.gameObject;
            }
        }

        return null;
    }

    private static TMP_Text FindText(Transform root, string objectName)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text.gameObject.name.Equals(objectName, System.StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }
        }

        return null;
    }

    private readonly struct ResultMessage
    {
        public ResultMessage(string companyName, bool success, double receivedAmount)
        {
            CompanyName = companyName;
            Success = success;
            ReceivedAmount = receivedAmount;
        }

        public string CompanyName { get; }
        public bool Success { get; }
        public double ReceivedAmount { get; }
    }
}
