using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InvestmentInputPanelController : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button okayButton;

    private Graphic placeholderGraphic;
    private bool isConfigured;

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

        if (inputField != null)
        {
            placeholderGraphic = inputField.placeholder;
            inputField.caretWidth = 2;
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
            okayButton.onClick.AddListener(ClosePanel);
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
        inputField.Select();
        inputField.ActivateInputField();
        inputField.caretPosition = inputField.text.Length;
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

    private void ClosePanel()
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
}
