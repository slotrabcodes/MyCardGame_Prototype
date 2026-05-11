using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 

public class DeckSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elements")]
    public TextMeshProUGUI deckNameText;
    public TextMeshProUGUI cardCountText;
    public Image backgroundImage; // Falls du später Deck-Porträts willst
    public GameObject selectionVisual;

    private DeckData linkedDeck;
    private int deckIndex;

    public void Setup(DeckData deck, int index)
    {
        linkedDeck = deck;
        deckIndex = index;

        deckNameText.text = deck.deckName;

        // Zeigt an, wie viele Karten im Deck sind (maxDeckSize aus dem DeckBuilder übernehmen oder festlegen)
        cardCountText.text = $"{deck.cards.Count} / 20";

        // Hier könntest du auch die Farbe des Slots ändern, wenn das Deck unvollständig ist
        if (deck.cards.Count < 20)
        {
            cardCountText.color = Color.yellow; // Warnung: Deck noch nicht fertig
        }
        else
        {
            cardCountText.color = Color.white;
        }

        // Beim Setup sicherstellen, dass die Markierung aus ist
        if (selectionVisual != null) selectionVisual.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Einfacher Klick: Deck auswählen/markieren
        DeckSelectionManager manager = FindObjectOfType<DeckSelectionManager>();
        manager.SelectDeck(deckIndex);

        // 2. Doppelklick: Direkt in den DeckBuilder
        if (eventData.clickCount == 2)
        {
            InventoryManager.Instance.currentDeckIndex = deckIndex;
            SceneManager.LoadScene("DeckBuilder");
        }
    }

    public int GetIndex()
    {
        return deckIndex;
    }

    // Hilfsmethode für den Manager, um den Rahmen an/auszuschalten
    public void SetSelected(bool isSelected)
    {
        if (selectionVisual != null) selectionVisual.SetActive(isSelected);
    }
}