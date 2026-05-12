using UnityEngine;

public class PlayerCardSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerCardPrefab;
    [SerializeField] private Transform cardRoot;
    [SerializeField] private int cardCount = 10;

    public void SpawnPlayerCards()
    {
        foreach (Transform child in cardRoot)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < cardCount; i++)
        {
            Instantiate(playerCardPrefab, cardRoot);
        }
    }
}