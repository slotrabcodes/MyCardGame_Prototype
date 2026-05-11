using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DeckListEntry : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI countText;

    private CardData assignedCard;

    public void Setup(CardData card, int count)
    {
        assignedCard = card;
        nameText.text = card.cardName;

        // Zeige die Anzahl nur an, wenn sie > 1 ist (wie in Hearthstone)
        countText.text = count > 1 ? "x" + count : "";
    }

    // Wird aufgerufen, wenn man auf den Eintrag in der Liste klickt
    public void OnRemoveClicked()
    {
        DeckBuilder.Instance.RemoveCardFromDeck(assignedCard);
    }
}