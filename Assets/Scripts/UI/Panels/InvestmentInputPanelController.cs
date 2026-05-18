using System.Globalization;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InvestmentInputPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button okayButton;
    [SerializeField] private Button closeButton;

    private Graphic placeholderGraphic;
    private bool isConfigured;
    private InvestmentManager investmentManager;
    private CompanyInvestmentData selectedCompany;
    private Coroutine focusRoutine;

    private void Awake()
    {
        Configure();
    }

    private void OnEnable()
    {
        Configure();
        UpdatePlaceholderVisibility();
    }

    public void Configure()
    {
        if (isConfigured)
        {
            return;
        }

        if (inputField == null)
        {
            inputField = GetComponentInChildren<TMP_InputField>(true);
        }

        if (okayButton == null)
        {
            okayButton = FindOkayButton();
        }

        if (closeButton == null)
        {
            closeButton = FindCloseButton();
        }

        if (inputField != null)
        {
            placeholderGraphic = inputField.placeholder;
            ConfigureInputField();
            inputField.onSelect.RemoveListener(OnInputSelected);
            inputField.onSelect.AddListener(OnInputSelected);
            inputField.onDeselect.RemoveListener(OnInputDeselected);
            inputField.onDeselect.AddListener(OnInputDeselected);
            inputField.onValueChanged.RemoveListener(OnInputValueChanged);
            inputField.onValueChanged.AddListener(OnInputValueChanged);
        }

        if (okayButton != null)
        {
            okayButton.onClick.RemoveAllListeners();
            okayButton.onClick.AddListener(OnOkayClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }

        isConfigured = true;
    }

    public void FocusInput()
    {
        Configure();

        if (inputField == null)
        {
            return;
        }

        HidePlaceholder();

        if (focusRoutine != null)
        {
            StopCoroutine(focusRoutine);
        }

        focusRoutine = StartCoroutine(FocusInputNextFrame());
    }

    public void OpenForCompany(CompanyInvestmentData company, InvestmentManager manager)
    {
        Configure();

        selectedCompany = company;
        investmentManager = manager;

        if (inputField != null)
        {
            inputField.text = string.Empty;
            if (inputField.placeholder is TMP_Text placeholderText && company != null)
            {
                placeholderText.text = $"Min. yatirim: ${NumberFormatter.FormatPrice(company.investmentCost)}";
            }
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        FocusInput();
    }

    private IEnumerator FocusInputNextFrame()
    {
        yield return null;

        if (inputField == null)
        {
            yield break;
        }

        EventSystem.current?.SetSelectedGameObject(inputField.gameObject);
        inputField.Select();
        inputField.ActivateInputField();
        inputField.caretPosition = inputField.text.Length;
        inputField.selectionAnchorPosition = inputField.text.Length;
        inputField.selectionFocusPosition = inputField.text.Length;
        focusRoutine = null;
    }

    private void OnInputSelected(string _)
    {
        HidePlaceholder();
    }

    private void OnInputDeselected(string _)
    {
        UpdatePlaceholderVisibility();
    }

    private void OnInputValueChanged(string _)
    {
        UpdatePlaceholderVisibility();
    }

    private void UpdatePlaceholderVisibility()
    {
        if (placeholderGraphic == null || inputField == null)
        {
            return;
        }

        placeholderGraphic.gameObject.SetActive(!inputField.isFocused && string.IsNullOrEmpty(inputField.text));
    }

    private void HidePlaceholder()
    {
        if (placeholderGraphic != null)
        {
            placeholderGraphic.gameObject.SetActive(false);
        }
    }

    private void OnOkayClicked()
    {
        if (investmentManager != null && selectedCompany != null)
        {
            if (!TryGetRequestedAmount(out double requestedAmount))
            {
                ShowInputHint("Lutfen yatirim tutari girin");
                FocusInput();
                return;
            }

            if (!investmentManager.StartInvestment(selectedCompany.companyId, requestedAmount))
            {
                FocusInput();
                return;
            }
        }

        ClosePanel();
    }

    private bool TryGetRequestedAmount(out double amount)
    {
        amount = 0d;

        if (inputField == null || string.IsNullOrWhiteSpace(inputField.text))
        {
            return false;
        }

        string value = inputField.text
            .Replace("$", string.Empty)
            .Replace(",", string.Empty)
            .Trim();

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out amount) && amount > 0d;
    }

    private void ConfigureInputField()
    {
        inputField.interactable = true;
        inputField.readOnly = false;
        inputField.caretWidth = 2;
        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputField.inputType = TMP_InputField.InputType.Standard;
        inputField.keyboardType = TouchScreenKeyboardType.DecimalPad;
        inputField.characterValidation = TMP_InputField.CharacterValidation.Decimal;
        inputField.lineType = TMP_InputField.LineType.SingleLine;

        if (inputField.textComponent != null)
        {
            inputField.textComponent.color = new Color32(50, 50, 50, 255);
            inputField.textComponent.fontSize = Mathf.Max(24f, inputField.textComponent.fontSize);
        }
    }

    private void ShowInputHint(string message)
    {
        if (inputField != null && inputField.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = message;
        }

        UpdatePlaceholderVisibility();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private Button FindOkayButton()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.gameObject.name.Equals("Okay", System.StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return buttons.Length > 0 ? buttons[0] : null;
    }

    private Button FindCloseButton()
    {
        Button directButton = UIHelper.FindButton(
            transform,
            "ClosingButton",
            "CloseButton",
            "ExitButton",
            "KapatButonu",
            "Kapat",
            "X");

        if (directButton != null && directButton != okayButton)
        {
            return directButton;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == okayButton)
            {
                continue;
            }

            if (HasCloseButtonNameInHierarchy(button.transform))
            {
                return button;
            }
        }

        return null;
    }

    private static bool HasCloseButtonNameInHierarchy(Transform current)
    {
        while (current != null)
        {
            if (IsCloseButtonName(current.name))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool IsCloseButtonName(string objectName)
    {
        return objectName.Equals("ClosingButton", System.StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("CloseButton", System.StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("ExitButton", System.StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("KapatButonu", System.StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("Kapat", System.StringComparison.OrdinalIgnoreCase)
            || objectName.Equals("X", System.StringComparison.OrdinalIgnoreCase);
    }
}
