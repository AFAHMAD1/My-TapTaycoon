using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MarketPanelRuntimePopulator : MonoBehaviour
{
    public enum MarketCategory
    {
        Forwards,
        Midfielders,
        Defenders,
        ManagersAndGoalkeepers
    }

    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject footballPlayerPrefab;
    [SerializeField] private GameObject footballManagerPrefab;
    [SerializeField] private RectTransform myPlayersContent;
    [SerializeField] private RectTransform myManagersContent;

    private const string GeneratedItemPrefix = "MarketItem_";
    private const string OwnedPlayerItemPrefix = "OwnedPlayer_";
    private const string OwnedManagerItemPrefix = "OwnedManager_";
    private const int MaxOwnedPlayers = 26;
    private const int MaxOwnedManagers = 5;

    private readonly Dictionary<MarketCategory, List<PlayerMarketData>> playersByCategory = new Dictionary<MarketCategory, List<PlayerMarketData>>();
    private readonly List<ManagerMarketData> managers = new List<ManagerMarketData>();
    private readonly List<GameObject> ownedPlayerCards = new List<GameObject>();
    private readonly List<GameObject> ownedManagerCards = new List<GameObject>();
    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    public void ShowForwards()
    {
        Populate(MarketCategory.Forwards);
    }

    public void ShowMidfielders()
    {
        Populate(MarketCategory.Midfielders);
    }

    public void ShowDefenders()
    {
        Populate(MarketCategory.Defenders);
    }

    public void ShowManagersAndGoalkeepers()
    {
        Populate(MarketCategory.ManagersAndGoalkeepers);
    }

    public void ClearMarketCards()
    {
        if (!initialized)
        {
            ResolveReferences();
        }

        ClearGeneratedItems();
        HideTemplates();

        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }

    public void Populate(MarketCategory category)
    {
        Initialize();

        if (content == null)
        {
            Debug.LogWarning("[MarketPanel] Scroll View Content bulunamadi.", this);
            return;
        }

        if (footballPlayerPrefab == null)
        {
            Debug.LogWarning("[MarketPanel] FootballPlayerPrefab bulunamadi.", this);
            return;
        }

        ClearGeneratedItems();
        HideTemplates();

        int cardIndex = 1;
        if (playersByCategory.TryGetValue(category, out List<PlayerMarketData> players))
        {
            foreach (PlayerMarketData player in players)
            {
                CreatePlayerCard(player, cardIndex++);
            }
        }

        if (category == MarketCategory.ManagersAndGoalkeepers)
        {
            if (footballManagerPrefab == null)
            {
                Debug.LogWarning("[MarketPanel] FootballManagerPrefab bulunamadi.", this);
            }
            else
            {
                foreach (ManagerMarketData manager in managers)
                {
                    CreateManagerCard(manager, cardIndex++);
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        if (scrollView != null)
        {
            scrollView.normalizedPosition = new Vector2(0f, 1f);
        }
    }

    private void Initialize()
    {
        if (initialized) return;

        ResolveReferences();
        BuildData();
        HideTemplates();
        initialized = true;
    }

    private void ResolveReferences()
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
            return;
        }

        if (footballPlayerPrefab == null)
        {
            footballPlayerPrefab = FindTemplate("FootballPlayerPrefab");
        }

        if (footballManagerPrefab == null)
        {
            footballManagerPrefab = FindTemplate("FootballManagerPrefab");
        }

        if (scrollView != null)
        {
            scrollView.horizontal = false;
            scrollView.vertical = true;
        }

        if (myPlayersContent == null)
        {
            myPlayersContent = FindClubContent("MyPlayers");
        }

        if (myManagersContent == null)
        {
            myManagersContent = FindClubContent("Managers", "MyManagers", "MyManagersPanel");
        }
    }

    private GameObject FindTemplate(string templateName)
    {
        if (content == null)
        {
            return null;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            if (child.name.Equals(templateName, StringComparison.Ordinal) ||
                child.name.Contains(templateName, StringComparison.Ordinal))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private void HideTemplates()
    {
        if (footballPlayerPrefab != null)
        {
            footballPlayerPrefab.SetActive(false);
        }

        if (footballManagerPrefab != null)
        {
            footballManagerPrefab.SetActive(false);
        }
    }

    private void ClearGeneratedItems()
    {
        if (content == null)
        {
            return;
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject child = content.GetChild(i).gameObject;
            if (child.name.StartsWith(GeneratedItemPrefix, StringComparison.Ordinal))
            {
                Destroy(child);
            }
        }
    }

    private void CreatePlayerCard(PlayerMarketData player, int cardIndex)
    {
        GameObject card = Instantiate(footballPlayerPrefab, content);
        card.name = $"{GeneratedItemPrefix}{cardIndex:00}_{player.Type}_{player.Name}";
        card.SetActive(true);

        SetText(card.transform, "Name", player.Name);
        SetText(card.transform, "Position", player.Type);
        SetText(card.transform, "Class", $"Class: {player.Class}");
        SetText(card.transform, "Age", $"Age - {player.Age}");
        SetText(card.transform, "Height", $"Height - {player.Height}");
        SetPriceText(card.transform, player.Price);
        SetButtonText(card.transform, "Sat\u0131n Al");
        WireBuyButton(card.transform, () => TryBuyPlayer(player, card));

        SetRandomStat(card.transform, "Speed");
        SetRandomStat(card.transform, "Pass_Accuracy");
        SetRandomStat(card.transform, "Defensive_Ability", "Defensive_A");
        SetRandomStat(card.transform, "Strength");
        SetRandomStat(card.transform, "Shoot_Power");
        SetRandomStat(card.transform, "Football_IQ");
        SetRandomStat(card.transform, "BallControl", "Ball_Control");
    }

    private void CreateManagerCard(ManagerMarketData manager, int cardIndex)
    {
        GameObject card = Instantiate(footballManagerPrefab, content);
        card.name = $"{GeneratedItemPrefix}{cardIndex:00}_Manager_{manager.Name}";
        card.SetActive(true);

        SetText(card.transform, "Name", manager.Name);
        SetText(card.transform, "Class", $"Class: {manager.Class}");
        SetPriceText(card.transform, manager.Price);
        SetButtonText(card.transform, "Sat\u0131n Al");
        WireBuyButton(card.transform, () => TryBuyManager(manager, card));

        SetRandomStat(card.transform, "ManagerLevel", "Manager Level");
        SetRandomStat(card.transform, "Defensive_Ability", "Tactic Boost");
        SetText(card.transform, "Shoot_Power", $"Age - {manager.Age}");
        SetRandomStat(card.transform, "Strength", "Experience");
        SetRandomStat(card.transform, "Pass_Accuracy", "Training Boost");
        SetRandomStat(card.transform, "Football_IQ");
    }

    private void SetRandomStat(Transform root, string objectName, string label = null)
    {
        int value = UnityEngine.Random.Range(50, 61);
        SetText(root, objectName, $"{(string.IsNullOrEmpty(label) ? objectName : label)} - {value}");
    }

    private static bool SetText(Transform root, string objectName, string value)
    {
        Transform target = FindChildRecursive(root, objectName);
        if (target == null)
        {
            return false;
        }

        TMP_Text text = target.GetComponent<TMP_Text>();
        if (text == null)
        {
            text = target.GetComponentInChildren<TMP_Text>(true);
        }

        if (text != null)
        {
            text.text = value;
            return true;
        }

        return false;
    }

    private static void SetPriceText(Transform root, int price)
    {
        string value = $"Price:\n${price}";

        if (SetText(root, "PriceTag", value))
        {
            return;
        }

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text != null && text.text.TrimStart().StartsWith("Price", StringComparison.OrdinalIgnoreCase))
            {
                text.text = value;
                return;
            }
        }
    }

    private static void SetButtonText(Transform root, string value)
    {
        Transform button = FindChildRecursive(root, "Buy_Sell_upgrade_Btn");
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

    private void WireBuyButton(Transform root, UnityEngine.Events.UnityAction buyAction)
    {
        Button button = FindCardButton(root);
        if (button == null)
        {
            Debug.LogWarning("[MarketPanel] Kart uzerinde satin alma butonu bulunamadi.", root);
            return;
        }

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(buyAction);
    }

    private void TryBuyPlayer(PlayerMarketData player, GameObject card)
    {
        if (!CanBuyOwnedCard(myPlayersContent, ownedPlayerCards, MaxOwnedPlayers, "oyuncu"))
        {
            return;
        }

        if (!TrySpendMarketPrice(player.Price))
        {
            return;
        }

        GetCategoryForPlayer(player.Type).Remove(player);
        MoveCardToClub(card, myPlayersContent, ownedPlayerCards, OwnedPlayerItemPrefix, player.Name);
    }

    private void TryBuyManager(ManagerMarketData manager, GameObject card)
    {
        if (!CanBuyOwnedCard(myManagersContent, ownedManagerCards, MaxOwnedManagers, "menajer"))
        {
            return;
        }

        if (!TrySpendMarketPrice(manager.Price))
        {
            return;
        }

        managers.Remove(manager);
        MoveCardToClub(card, myManagersContent, ownedManagerCards, OwnedManagerItemPrefix, manager.Name);
    }

    private bool CanBuyOwnedCard(RectTransform destination, List<GameObject> ownedCards, int maxCount, string itemName)
    {
        if (destination == null)
        {
            Debug.LogWarning($"[MarketPanel] {itemName} icin kulup hedef paneli bulunamadi.", this);
            return false;
        }

        PruneMissingCards(ownedCards);
        if (ownedCards.Count >= maxCount)
        {
            Debug.Log($"[MarketPanel] Maksimum {maxCount} {itemName} sinirina ulasildi.", this);
            return false;
        }

        return true;
    }

    private bool TrySpendMarketPrice(int price)
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[MarketPanel] CurrencyManager bulunamadi; satin alma yapilamadi.", this);
            return false;
        }

        if (!CurrencyManager.Instance.SpendMoney(price))
        {
            Debug.Log("[MarketPanel] Satin alma icin para yetersiz.", this);
            return false;
        }

        return true;
    }

    private void MoveCardToClub(
        GameObject card,
        RectTransform destination,
        List<GameObject> ownedCards,
        string itemPrefix,
        string itemName)
    {
        if (card == null || destination == null)
        {
            return;
        }

        card.name = $"{itemPrefix}{ownedCards.Count + 1:00}_{itemName}";
        card.transform.SetParent(destination, false);
        ConfigureOwnedCard(card.transform);
        ownedCards.Add(card);

        LayoutRebuilder.ForceRebuildLayoutImmediate(destination);
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }

    private static void ConfigureOwnedCard(Transform root)
    {
        SetButtonText(root, "Upgrade");

        Button button = FindCardButton(root);
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
        }
    }

    private RectTransform FindClubContent(params string[] clubPanelNames)
    {
        Transform searchRoot = transform.root;
        foreach (string clubPanelName in clubPanelNames)
        {
            Transform clubPanel = FindChildRecursive(searchRoot, clubPanelName);
            if (clubPanel == null)
            {
                continue;
            }

            ScrollRect clubScrollView = clubPanel.GetComponentInChildren<ScrollRect>(true);
            if (clubScrollView != null && clubScrollView.content != null)
            {
                ConfigureClubScrollView(clubScrollView);
                return clubScrollView.content;
            }
        }

        return null;
    }

    private void ConfigureClubScrollView(ScrollRect clubScrollView)
    {
        GridLayoutGroup scrollGrid = clubScrollView.GetComponent<GridLayoutGroup>();
        if (scrollGrid != null)
        {
            scrollGrid.enabled = false;
        }

        ContentSizeFitter scrollFitter = clubScrollView.GetComponent<ContentSizeFitter>();
        if (scrollFitter != null)
        {
            scrollFitter.enabled = false;
        }

        RectTransform clubContent = clubScrollView.content;
        ConfigureClubContentGrid(clubContent);
        HideClubTemplates(clubContent);

        clubScrollView.horizontal = false;
        clubScrollView.vertical = true;
        ConfigureClubViewport(clubScrollView);
        ConfigureClubScrollbars(clubScrollView);
    }

    private static void ConfigureClubViewport(ScrollRect clubScrollView)
    {
        RectTransform viewport = clubScrollView.viewport;
        if (viewport == null)
        {
            return;
        }

        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.anchoredPosition = Vector2.zero;
        viewport.sizeDelta = new Vector2(-20f, 0f);
        viewport.pivot = new Vector2(0f, 1f);
    }

    private static void ConfigureClubScrollbars(ScrollRect clubScrollView)
    {
        if (clubScrollView.horizontalScrollbar != null)
        {
            clubScrollView.horizontalScrollbar.gameObject.SetActive(false);
        }

        Scrollbar verticalScrollbar = clubScrollView.verticalScrollbar;
        if (verticalScrollbar == null)
        {
            return;
        }

        RectTransform verticalScrollbarRect = verticalScrollbar.transform as RectTransform;
        if (verticalScrollbarRect == null)
        {
            return;
        }

        verticalScrollbarRect.anchorMin = new Vector2(1f, 0f);
        verticalScrollbarRect.anchorMax = Vector2.one;
        verticalScrollbarRect.anchoredPosition = Vector2.zero;
        verticalScrollbarRect.sizeDelta = new Vector2(20f, 0f);
        verticalScrollbarRect.pivot = Vector2.one;
    }

    private void ConfigureClubContentGrid(RectTransform clubContent)
    {
        if (clubContent == null)
        {
            return;
        }

        GridLayoutGroup grid = clubContent.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = clubContent.gameObject.AddComponent<GridLayoutGroup>();
        }

        GridLayoutGroup marketGrid = content != null ? content.GetComponent<GridLayoutGroup>() : null;
        if (marketGrid != null)
        {
            grid.padding = new RectOffset(
                marketGrid.padding.left,
                marketGrid.padding.right,
                marketGrid.padding.top,
                marketGrid.padding.bottom);
            grid.cellSize = marketGrid.cellSize;
            grid.spacing = marketGrid.spacing;
            grid.childAlignment = marketGrid.childAlignment;
            grid.startCorner = marketGrid.startCorner;
            grid.startAxis = marketGrid.startAxis;
            grid.constraint = marketGrid.constraint;
            grid.constraintCount = marketGrid.constraintCount;
        }
        else
        {
            grid.cellSize = new Vector2(354.6f, 530.8f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
        }

        ContentSizeFitter fitter = clubContent.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = clubContent.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static void HideClubTemplates(RectTransform clubContent)
    {
        if (clubContent == null)
        {
            return;
        }

        for (int i = 0; i < clubContent.childCount; i++)
        {
            GameObject child = clubContent.GetChild(i).gameObject;
            if (child.name.Equals("FootballPlayerPrefab", StringComparison.Ordinal) ||
                child.name.Equals("FootballManagerPrefab", StringComparison.Ordinal))
            {
                child.SetActive(false);
            }
        }
    }

    private static Button FindCardButton(Transform root)
    {
        Transform buttonTransform = FindChildRecursive(root, "Buy_Sell_upgrade_Btn");
        return buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;
    }

    private static void PruneMissingCards(List<GameObject> cards)
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (cards[i] == null)
            {
                cards.RemoveAt(i);
            }
        }
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.Equals(childName, StringComparison.Ordinal))
            {
                return child;
            }

            Transform nested = FindChildRecursive(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private void BuildData()
    {
        playersByCategory.Clear();
        managers.Clear();

        playersByCategory[MarketCategory.Forwards] = new List<PlayerMarketData>();
        playersByCategory[MarketCategory.Midfielders] = new List<PlayerMarketData>();
        playersByCategory[MarketCategory.Defenders] = new List<PlayerMarketData>();
        playersByCategory[MarketCategory.ManagersAndGoalkeepers] = new List<PlayerMarketData>();

        AddPlayer("Leo Vargas", "Forward", "C", 4200, 23, 181);
        AddPlayer("Marco Silva", "Forward", "C", 3100, 21, 178);
        AddPlayer("Adrian Costa", "Forward", "C", 2600, 20, 176);
        AddPlayer("Daniel Moretti", "Forward", "C", 4800, 24, 185);
        AddPlayer("Omar Haddad", "Forward", "C", 2900, 22, 180);
        AddPlayer("Niko Petrov", "Forward", "C", 2100, 19, 177);
        AddPlayer("Ethan Brooks", "Forward", "C", 3500, 23, 183);
        AddPlayer("Rafael Mendes", "Forward", "C", 4600, 25, 184);
        AddPlayer("Yusuf Demir", "Forward", "C", 3300, 21, 179);
        AddPlayer("Lucas Ferreira", "Forward", "C", 2700, 20, 175);
        AddPlayer("Carlos Vega", "Forward", "C", 3900, 22, 182);
        AddPlayer("Milan Kovac", "Forward", "C", 2300, 19, 186);
        AddPlayer("Samir Nouri", "Forward", "C", 3000, 21, 174);
        AddPlayer("Tomas Rivera", "Forward", "C", 2500, 20, 178);
        AddPlayer("Felix Hartmann", "Forward", "C", 3700, 24, 188);
        AddPlayer("Ivan Sokolov", "Forward", "C", 2400, 19, 187);
        AddPlayer("Ali Karimi", "Forward", "C", 2800, 22, 180);
        AddPlayer("Bruno Almeida", "Forward", "C", 4100, 23, 181);
        AddPlayer("Kenji Tanaka", "Forward", "C", 2600, 20, 172);
        AddPlayer("Victor Rossi", "Forward", "C", 3800, 24, 184);
        AddPlayer("Mateo Cruz", "Forward", "C", 3200, 21, 179);
        AddPlayer("Arman Jalali", "Forward", "C", 2200, 19, 176);
        AddPlayer("Noah Bennett", "Forward", "C", 3000, 22, 183);
        AddPlayer("Sergio Marin", "Forward", "C", 4500, 25, 185);
        AddPlayer("Dario Conti", "Forward", "C", 2400, 20, 177);
        AddPlayer("Emir Sahin", "Midfielder", "C", 3600, 22, 178);
        AddPlayer("Julian Weber", "Midfielder", "C", 3900, 24, 184);
        AddPlayer("Kian Moradi", "Midfielder", "C", 2500, 20, 176);
        AddPlayer("Oscar Lind", "Midfielder", "C", 2300, 19, 181);
        AddPlayer("Hugo Martins", "Midfielder", "C", 3400, 23, 180);
        AddPlayer("Fahri Yilmaz", "Midfielder", "C", 2900, 21, 175);
        AddPlayer("Andre Novak", "Midfielder", "C", 4700, 25, 186);
        AddPlayer("Luis Herrera", "Midfielder", "C", 3100, 22, 177);
        AddPlayer("Mert Kaya", "Midfielder", "C", 2200, 19, 174);
        AddPlayer("Gabriel Santos", "Midfielder", "C", 4100, 24, 179);
        AddPlayer("Jonas Becker", "Midfielder", "C", 2700, 20, 182);
        AddPlayer("Amir Rahimi", "Midfielder", "C", 3500, 23, 180);
        AddPlayer("Pablo Torres", "Midfielder", "C", 2400, 19, 173);
        AddPlayer("Simon Keller", "Midfielder", "C", 3700, 24, 185);
        AddPlayer("Farid Azizi", "Midfielder", "C", 2600, 20, 176);
        AddPlayer("Matteo Bianchi", "Midfielder", "C", 4300, 25, 181);
        AddPlayer("Ryan Cooper", "Midfielder", "C", 2800, 21, 178);
        AddPlayer("Ibrahim Celik", "Midfielder", "C", 3000, 22, 179);
        AddPlayer("Leon Fischer", "Midfielder", "C", 3300, 23, 183);
        AddPlayer("Diego Ramos", "Midfielder", "C", 4000, 24, 180);
        AddPlayer("Arda Kaplan", "Midfielder", "C", 2500, 20, 175);
        AddPlayer("Thiago Nunes", "Midfielder", "C", 4600, 25, 182);
        AddPlayer("Nabil Mansour", "Midfielder", "C", 2300, 19, 177);
        AddPlayer("Robin Meyer", "Midfielder", "C", 2900, 21, 184);
        AddPlayer("Selim Aydin", "Midfielder", "C", 3500, 23, 178);
        AddPlayer("Max Steiner", "Defender", "C", 3000, 22, 188);
        AddPlayer("Hasan Ozkan", "Defender", "C", 3200, 23, 185);
        AddPlayer("Lorenzo Greco", "Defender", "C", 4100, 24, 190);
        AddPlayer("Victor Hansen", "Defender", "C", 2700, 20, 187);
        AddPlayer("Reza Tavakoli", "Defender", "C", 2900, 21, 186);
        AddPlayer("Martin Novak", "Defender", "C", 3600, 23, 191);
        AddPlayer("Javier Molina", "Defender", "C", 3400, 22, 184);
        AddPlayer("Tariq Hassan", "Defender", "C", 2400, 19, 183);
        AddPlayer("Ozan Demirci", "Defender", "C", 3800, 24, 189);
        AddPlayer("Filip Larsen", "Defender", "C", 2600, 20, 188);
        AddPlayer("Ricardo Alves", "Defender", "C", 4400, 25, 192);
        AddPlayer("Ben Turner", "Defender", "C", 3100, 22, 186);
        AddPlayer("Kamil Wojcik", "Defender", "C", 2800, 21, 190);
        AddPlayer("Andrei Popescu", "Defender", "C", 3300, 23, 187);
        AddPlayer("Hamza Barakat", "Defender", "C", 2300, 19, 184);
        AddPlayer("Nicolas Perrin", "Defender", "C", 3900, 24, 191);
        AddPlayer("Kerem Arslan", "Defender", "C", 3000, 22, 185);
        AddPlayer("Joao Ribeiro", "Defender", "C", 4500, 25, 193);
        AddPlayer("Elias Schmid", "Defender", "C", 3200, 23, 188);
        AddPlayer("Sami Haddadi", "Defender", "C", 2100, 19, 182);
        AddPlayer("Anton Berg", "Defender", "C", 3500, 24, 190);
        AddPlayer("Manuel Ortega", "Defender", "C", 3300, 22, 187);
        AddPlayer("Ramin Farzan", "Defender", "C", 2200, 20, 184);
        AddPlayer("Chris Morgan", "Defender", "C", 3700, 23, 189);
        AddPlayer("Davide Romano", "Defender", "C", 4200, 25, 192);
        AddPlayer("Alex Morgan", "Goalkeeper", "C", 4300, 24, 191);
        AddPlayer("Burak Yildiz", "Goalkeeper", "C", 3500, 22, 188);
        AddPlayer("Luca Marino", "Goalkeeper", "C", 4000, 23, 192);
        AddPlayer("Jonas Wolf", "Goalkeeper", "C", 3700, 21, 190);
        AddPlayer("Amir Sadeghi", "Goalkeeper", "C", 3000, 20, 187);
        AddPlayer("Pedro Silva", "Goalkeeper", "C", 3900, 24, 193);
        AddPlayer("Mikael Jensen", "Goalkeeper", "C", 2800, 19, 186);
        AddPlayer("Can Eren", "Goalkeeper", "C", 3400, 22, 189);
        AddPlayer("Rafael Costa", "Goalkeeper", "C", 4500, 25, 194);
        AddPlayer("Nikolai Ivanov", "Goalkeeper", "C", 3100, 21, 190);

        AddManager("Jose Alvarez", "C", 4700, 45);
        AddManager("Mehmet Kaya", "C", 3900, 42);
        AddManager("Antonio Ricci", "C", 5000, 49);
        AddManager("David Schneider", "C", 3200, 38);
        AddManager("Farhad Rahimi", "C", 4100, 44);
        AddManager("Lucas Bennett", "C", 2800, 36);
        AddManager("Hakan Demir", "C", 4600, 47);
        AddManager("Marco Bellini", "C", 3500, 40);
        AddManager("Olivier Laurent", "C", 4900, 52);
        AddManager("Sami Mansour", "C", 2600, 35);
        AddManager("Thomas Miller", "C", 3700, 41);
        AddManager("Ruben Castillo", "C", 4000, 43);
        AddManager("Kenji Nakamura", "C", 4300, 46);
        AddManager("Arif Celik", "C", 3000, 37);
        AddManager("Nicolas Moreau", "C", 4800, 50);
    }

    private void AddPlayer(string name, string type, string playerClass, int price, int age, int height)
    {
        PlayerMarketData player = new PlayerMarketData(name, type, playerClass, price, age, height);
        GetCategoryForPlayer(type).Add(player);
    }

    private List<PlayerMarketData> GetCategoryForPlayer(string type)
    {
        switch (type)
        {
            case "Forward":
                return playersByCategory[MarketCategory.Forwards];
            case "Midfielder":
                return playersByCategory[MarketCategory.Midfielders];
            case "Defender":
                return playersByCategory[MarketCategory.Defenders];
            case "Goalkeeper":
                return playersByCategory[MarketCategory.ManagersAndGoalkeepers];
            default:
                return playersByCategory[MarketCategory.Forwards];
        }
    }

    private void AddManager(string name, string managerClass, int price, int age)
    {
        managers.Add(new ManagerMarketData(name, managerClass, price, age));
    }

    private readonly struct PlayerMarketData
    {
        public readonly string Name;
        public readonly string Type;
        public readonly string Class;
        public readonly int Price;
        public readonly int Age;
        public readonly int Height;

        public PlayerMarketData(string name, string type, string playerClass, int price, int age, int height)
        {
            Name = name;
            Type = type;
            Class = playerClass;
            Price = price;
            Age = age;
            Height = height;
        }
    }

    private readonly struct ManagerMarketData
    {
        public readonly string Name;
        public readonly string Class;
        public readonly int Price;
        public readonly int Age;

        public ManagerMarketData(string name, string managerClass, int price, int age)
        {
            Name = name;
            Class = managerClass;
            Price = price;
            Age = age;
        }
    }
}
