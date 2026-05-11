using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CollectionManager : MonoBehaviour
{
    [Header("Daten")]
    // Wir machen das private, da wir es jetzt automatisch befüllen
    private List<CardData> allCards = new List<CardData>();

    [Header("UI Referenzen")]
    public Transform gridParent;
    public GameObject cardPrefab;
    public TMP_InputField searchField;

    [Header("Filter UI")]
    public TMP_Dropdown rarityDropdown;
    public TMP_Dropdown typeDropdown;
    public TMP_Dropdown subclassDropdown;
    public TMP_Dropdown ownedDropdown;

    [Header("Pagination Settings")]
    public int cardsPerPage = 8;
    private int currentPage = 0;
    private List<CardData> currentFilteredCards = new List<CardData>();
    private List<GameObject> spawnedCards = new List<GameObject>();

    private IEnumerator Start()
    {
        // 1. Warten auf InventoryManager
        while (InventoryManager.Instance == null)
        {
            yield return null;
        }

        // 2. AUTOMATISCHES LADEN AUS RESOURCES
        // Lädt alle ScriptableObjects aus Assets/Resources/Cards
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");
        allCards = new List<CardData>(loadedCards);

        // 3. Initialer Filter & Anzeige
        OnFilterChanged();
    }

    void Update()
    {
        // Mausrad-Input abfragen
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f) // Mausrad nach oben
        {
            PreviousPage();
        }
        else if (scrollInput < 0f) // Mausrad nach unten
        {
            NextPage();
        }
    }

    public void OnFilterChanged()
    {
        Debug.Log("Filter wurde aufgerufen!"); // Erscheint das in der Konsole beim Umstellen?

        string searchText = searchField != null ? searchField.text.ToLower() : "";

        currentFilteredCards = allCards.Where(card => {
            // 1. Suche
            bool searchOk = string.IsNullOrEmpty(searchText) ||
                            card.cardName.ToLower().Contains(searchText) ||
                            card.description.ToLower().Contains(searchText) ||
                            card.rarity.ToString().ToLower().Contains(searchText) ||
                            card.subclass.ToString().ToLower().Contains(searchText) ||
                            card.cardType.ToString().ToLower().Contains(searchText);

            // 2. Rarity
            bool rarityOk = rarityDropdown == null || rarityDropdown.value == 0 ||
                            card.rarity.ToString() == rarityDropdown.options[rarityDropdown.value].text;

            // 3. Type
            bool typeOk = typeDropdown == null || typeDropdown.value == 0 ||
                          card.cardType.ToString() == typeDropdown.options[typeDropdown.value].text;

            // 4. Subclass (FIXED!)
            bool subclassOk = subclassDropdown == null || subclassDropdown.value == 0 ||
                              card.subclass.ToString() == subclassDropdown.options[subclassDropdown.value].text;

            // 5. Ownership
            bool ownershipOk = true;
            if (ownedDropdown != null && ownedDropdown.value != 0)
            {
                int count = InventoryManager.Instance.GetCardCount(card);
                if (ownedDropdown.value == 1) ownershipOk = (count > 0);
                else if (ownedDropdown.value == 2) ownershipOk = (count == 0);
            }

            return searchOk && rarityOk && typeOk && subclassOk && ownershipOk;
        }).ToList();

        SortCards();
        currentPage = 0;
        RefreshPage();

        Debug.Log("Anzahl gefilterte Karten: " + currentFilteredCards.Count);
    }


    private void CreateCardItem(CardData data)
    {
        // Sicherheitscheck für Inspector-Zuweisungen
        if (cardPrefab == null || gridParent == null) return;

        GameObject newCard = Instantiate(cardPrefab, gridParent);
        newCard.SetActive(true);

        CardDisplay display = newCard.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.cardData = data;
            display.UpdateCardUI();

            // Status prüfen und ausgrauen
            int amount = InventoryManager.Instance.GetCardCount(data);
            display.SetOwnedStatus(amount);

            // NEU: Hier zwingen wir die Karte, ihre Buttons und das Cover zu prüfen
            display.SetupButtons();
            // Sicherstellen, dass das Cover in der Collection aus ist:
            if (display.cardCover != null) display.cardCover.SetActive(false);
        }

        spawnedCards.Add(newCard);
    }

    public void RefreshPage()
    {
        // 1. Altes Grid löschen
        foreach (GameObject card in spawnedCards)
        {
            if (card != null) Destroy(card);
        }
        spawnedCards.Clear();

        // 2. Bereich berechnen
        int startIndex = currentPage * cardsPerPage;
        int endIndex = Mathf.Min(startIndex + cardsPerPage, currentFilteredCards.Count);

        // 3. Nur Karten für diese Seite zeigen
        for (int i = startIndex; i < endIndex; i++)
        {
            CreateCardItem(currentFilteredCards[i]);
        }
    }

    public void NextPage()
    {
        if ((currentPage + 1) * cardsPerPage < currentFilteredCards.Count)
        {
            currentPage++;
            RefreshPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    void SortCards()
    {
        currentFilteredCards = currentFilteredCards
            .OrderBy(card => card.cardCost)
            .ThenBy(card => card.cardName)
            .ToList();
    }
}