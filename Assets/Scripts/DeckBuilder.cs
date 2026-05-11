using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DeckBuilder : MonoBehaviour
{
    public static DeckBuilder Instance;

    [Header("Deck Settings")]
    public DeckData currentDeck = new DeckData();
    public int maxDeckSize = 20;
    public TMP_InputField deckNameInput;
    public TextMeshProUGUI sizeCounterText;

    [Header("UI Deck List")]
    public Transform deckListParent;
    public GameObject deckListEntryPrefab; // Ein schmales UI-Element für den Namen der Karte im Deck

    private void Awake() => Instance = this;

    void Start()
    {
        if (deckNameInput != null)
            deckNameInput.onEndEdit.AddListener(UpdateDeckName);

        // Lade das ausgewählte Deck aus dem InventoryManager
        int index = InventoryManager.Instance.currentDeckIndex;

        // Sicherheitsprüfung: Ist der Index gültig?
        if (index >= 0 && index < InventoryManager.Instance.allDecks.Count)
        {
            currentDeck = InventoryManager.Instance.allDecks[index];

            // UI befüllen
            if (deckNameInput != null) deckNameInput.text = currentDeck.deckName;
            UpdateDeckUI();
        }
        else
        {
            Debug.LogError("Deck-Index " + index + " ist ungültig! Zurück zur Auswahl...");
            // Optional: Zurück zur Auswahl schicken, wenn kein Deck gefunden wurde
            // SceneManager.LoadScene("DeckSelection");
        }

        currentDeck = InventoryManager.Instance.allDecks[index];

        // UI mit den geladenen Daten füllen
        deckNameInput.text = currentDeck.deckName;

        UpdateDeckUI();
    }

    public void AddCardToDeck(CardData card)
    {
        // 1. Check: Deck voll?
        if (currentDeck.cards.Count >= maxDeckSize)
        {
            Debug.Log("Deck ist voll!");
            return;
        }

        // 2. Check: Hast du die Karte im Inventar?
        int ownedCount = InventoryManager.Instance.GetCardCount(card);
        int inDeckCount = currentDeck.GetCardCount(card);

        if (inDeckCount >= ownedCount)
        {
            Debug.Log("Du hast nicht mehr Kopien dieser Karte!");
            return;
        }

        // 3. Check: Rarity Limits
        if (!CanAddMoreOfRarity(card, inDeckCount))
        {
            Debug.Log("Rarity Limit erreicht!");
            return;
        }

        currentDeck.cards.Add(card);
        UpdateDeckUI();
    }

    bool CanAddMoreOfRarity(CardData card, int currentCount)
    {
        switch (card.rarity)
        {
            case Rarity.Heavenly: return currentCount < 1;
            case Rarity.Legendary: return currentCount < 2;
            default: return currentCount < 3; // Common, Uncommon, Rare, Epic
        }
    }

    public void RemoveCardFromDeck(CardData card)
    {
        CardData toRemove = currentDeck.cards.Find(c => c.cardName == card.cardName);
        if (toRemove != null)
        {
            currentDeck.cards.Remove(toRemove);
            UpdateDeckUI();
        }
    }

    public void UpdateDeckName(string newName) => currentDeck.deckName = newName;

    public void UpdateDeckUI()
    {
        // Hier löschen wir die visuelle Liste rechts und bauen sie neu auf
        foreach (Transform child in deckListParent) Destroy(child.gameObject);

        // Sortiere Deck nach Kosten für die Anzeige
        var sortedDeck = currentDeck.cards.OrderBy(c => c.cardCost).ThenBy(c => c.cardName);

        // Um Platz zu sparen: Gruppiere gleiche Karten (z.B. "Feuerball x2")
        var grouped = sortedDeck.GroupBy(c => c.cardName);

        foreach (var group in grouped)
        {
            GameObject entry = Instantiate(deckListEntryPrefab, deckListParent);
            // Hier ein kleines Skript auf dem Prefab nutzen, um Name und Count zu setzen
            entry.GetComponent<DeckListEntry>().Setup(group.First(), group.Count());
        }

        if (sizeCounterText != null)
            sizeCounterText.text = $"{currentDeck.cards.Count} / {maxDeckSize}";
    }

    public void SaveAndBackToMenu()
    {
        // Namen aus dem InputField übernehmen
        currentDeck.deckName = deckNameInput.text;

        // Alles im InventoryManager dauerhaft speichern
        InventoryManager.Instance.SaveGame();

        // Zurück zur Auswahl
        UnityEngine.SceneManagement.SceneManager.LoadScene("DeckSelection");
    }
}