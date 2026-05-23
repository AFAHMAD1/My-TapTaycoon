using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class MarketCardUIBase : MonoBehaviour
{
    private Button actionButton;

    public bool WireAction(UnityAction action)
    {
        Button button = FindActionButton();
        if (button == null)
        {
            return false;
        }

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        if (action != null)
        {
            button.onClick.AddListener(action);
        }

        return true;
    }

    public void SetActionText(string value)
    {
        Button button = FindActionButton();
        if (button == null)
        {
            return;
        }

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = value;
        }
    }

    public void SetActionInteractable(bool interactable)
    {
        Button button = FindActionButton();
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    protected bool SetText(string objectName, string value)
    {
        TMP_Text text = FindText(objectName);
        if (text == null)
        {
            return false;
        }

        text.text = value;
        return true;
    }

    protected void SetPrice(double price)
    {
        string value = $"Price:\n${NumberFormatter.FormatPrice(price)}";
        if (SetText("PriceTag", value))
        {
            return;
        }

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text != null && text.text.TrimStart().StartsWith("Price", StringComparison.OrdinalIgnoreCase))
            {
                text.text = value;
                return;
            }
        }
    }

    protected TMP_Text FindText(string objectName)
    {
        Transform target = UIHelper.FindChildRecursive(transform, objectName);
        if (target == null)
        {
            return null;
        }

        TMP_Text text = target.GetComponent<TMP_Text>();
        return text != null ? text : target.GetComponentInChildren<TMP_Text>(true);
    }

    protected TMP_Text FindTextByPrefix(string prefix)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text candidate = texts[i];
            if (candidate.text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return null;
    }

    private Button FindActionButton()
    {
        if (actionButton != null)
        {
            return actionButton;
        }

        actionButton = UIHelper.FindButton(transform, "Buy_Sell_upgrade_Btn");
        return actionButton;
    }
}
