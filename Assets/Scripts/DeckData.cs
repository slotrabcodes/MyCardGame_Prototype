using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeckData
{
    public string deckName = "New Deck";
    public List<CardData> cards = new List<CardData>();

    public int GetCardCount(CardData card)
    {
        int count = 0;
        foreach (CardData c in cards)
        {
            if (c.cardName == card.cardName) count++;
        }
        return count;
    }
}
