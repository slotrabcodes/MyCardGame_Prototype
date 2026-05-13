using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class DeckSaveData
{
    public string deckName;
    public List<string> cardNames = new List<string>();
}

[System.Serializable]
public class FullSaveData
{
    public List<string> collectedCardNames;
    public List<DeckSaveData> savedDecks;
    public int coins; // Falls du Währung hast
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Player Starting Values")]
    public int gold = 500; // Startgold
    public int ownedPacks = 0;

    [Header("All Available Cards")]
    public List<CardData> playerCollection = new List<CardData>();

    [Header("Decks")]
    public List<DeckData> allDecks = new List<DeckData>();
    public int currentDeckIndex = -1;

    public bool AddCardToCollection(CardData card)
    {
        if (GetCardCount(card) < 3)
        {
            playerCollection.Add(card);
            return true; // Karte wurde hinzugefügt
        }
        else
        {
            // Karte ist schon 3x da (vielleicht später in Staub/Gold umwandeln?)
            gold += 20; // Kleiner Trostpreis
            return false;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Sicherstellen, dass es Root ist
            DontDestroyOnLoad(gameObject);
            LoadGame(); // Deine Speicherdaten laden
        }
        else
        {
            // Falls schon einer existiert (z.B. aus der vorherigen Szene), 
            // lösche diesen hier, damit es kein Duplikat gibt.
            Destroy(gameObject);
        }
    }

    public int GetCardCount(CardData card)
    {
        if (playerCollection == null || card == null) return 0;

        int count = 0;
        foreach (CardData c in playerCollection)
        {
            // Wir vergleichen den Namen, falls die Asset-Referenz zickt
            if (c != null && c.cardName == card.cardName)
                count++;
        }
        return count;
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("Gold", gold);
        PlayerPrefs.SetInt("Packs", ownedPacks);

        // 1. Collection speichern (wie bisher)
        List<string> colNames = new List<string>();
        foreach (CardData card in playerCollection) colNames.Add(card.name);
        PlayerPrefs.SetString("Collection", string.Join(";", colNames));

        // 2. Decks speichern
        // Wir speichern: DeckName:Karte1,Karte2|DeckName2:Karte1,Karte2
        List<string> deckStrings = new List<string>();
        foreach (DeckData deck in allDecks)
        {
            string dName = deck.deckName;
            List<string> cNames = new List<string>();
            foreach (CardData c in deck.cards) cNames.Add(c.name);

            string fullDeckString = dName + ":" + string.Join(";", cNames);
            deckStrings.Add(fullDeckString);
        }
        PlayerPrefs.SetString("Decks", string.Join("|", deckStrings));

        PlayerPrefs.Save();
        Debug.Log("Spiel inklusive Decks gespeichert!");
    }

    public void LoadGame()
    {
        gold = PlayerPrefs.GetInt("Gold", 500);
        ownedPacks = PlayerPrefs.GetInt("Packs", 0);

        // 1. Collection laden (wie bisher)
        string collectionString = PlayerPrefs.GetString("Collection", "");
        playerCollection.Clear();
        if (!string.IsNullOrEmpty(collectionString))
        {
            foreach (string cName in collectionString.Split(';'))
            {
                if (string.IsNullOrEmpty(cName)) continue;
                CardData card = Resources.Load<CardData>("Cards/" + cName);
                if (card != null) playerCollection.Add(card);
            }
        }

        // 2. Decks laden
        string decksString = PlayerPrefs.GetString("Decks", "");
        allDecks.Clear();
        if (!string.IsNullOrEmpty(decksString))
        {
            string[] individualDecks = decksString.Split('|');
            foreach (string deckDataRaw in individualDecks)
            {
                if (string.IsNullOrEmpty(deckDataRaw)) continue;

                string[] splitNameAndCards = deckDataRaw.Split(':');
                DeckData newDeck = new DeckData();
                newDeck.deckName = splitNameAndCards[0];

                // Karten des Decks laden
                if (splitNameAndCards.Length > 1 && !string.IsNullOrEmpty(splitNameAndCards[1]))
                {
                    string[] cardNames = splitNameAndCards[1].Split(';');
                    foreach (string cName in cardNames)
                    {
                        CardData card = Resources.Load<CardData>("Cards/" + cName);
                        if (card != null) newDeck.cards.Add(card);
                    }
                }
                allDecks.Add(newDeck);
            }
        }
        Debug.Log($"Geladen: {playerCollection.Count} Karten, {allDecks.Count} Decks.");
    }

    // Damit beim Beenden automatisch gespeichert wird
    private void OnApplicationQuit() { SaveGame(); }
}