using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MarketPanelRuntimePopulator : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject itemTemplate;
    [SerializeField, Min(1)] private int itemCount = 25;

    private const string GeneratedItemPrefix = "MarketItem_";

    private bool hasPopulated;

    private void Start()
    {
        Populate();
    }

    public void Populate()
    {
        if (hasPopulated)
        {
            return;
        }

        hasPopulated = true;

        if (!ResolveReferences())
        {
            return;
        }

        ClearGeneratedItems();

        itemTemplate.SetActive(false);

        for (int i = 0; i < itemCount; i++)
        {
            GameObject item = Instantiate(itemTemplate, content);
            item.name = $"{GeneratedItemPrefix}{i + 1:00}";
            item.SetActive(true);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private bool ResolveReferences()
    {
        if (scrollView == null)
        {
            scrollView = GetComponentInChildren<ScrollRect>(true);
        }

        if (content == null && scrollView != null)
        {
            content = scrollView.content;
        }

        if (content == null)
        {
            Debug.LogWarning("[MarketPanel] Scroll View Content bulunamadi; market kartlari olusturulamadi.", this);
            return false;
        }

        if (itemTemplate == null)
        {
            itemTemplate = FindTemplate();
        }

        if (itemTemplate == null)
        {
            Debug.LogWarning("[MarketPanel] Kart template bulunamadi; Content altina bir prefab/template ekleyin.", this);
            return false;
        }

        if (scrollView != null)
        {
            scrollView.horizontal = false;
            scrollView.vertical = true;
        }

        return true;
    }

    private GameObject FindTemplate()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (!child.name.StartsWith(GeneratedItemPrefix, StringComparison.Ordinal))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private void ClearGeneratedItems()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;
            if (child != itemTemplate && child.name.StartsWith(GeneratedItemPrefix, StringComparison.Ordinal))
            {
                Destroy(child);
            }
        }
    }
}
