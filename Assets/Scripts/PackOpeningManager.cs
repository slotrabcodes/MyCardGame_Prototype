using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackOpeningManager : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI packCounterDisplay;
    public Button buyButton; // In "BuyButton" im Inspector umbenannt oder beibehalten

    [Header("Pool")]
    public List<CardData> allCards = new List<CardData>();

    [Header("UI Slots")]
    public List<CardDisplay> cardSlots;
    public GameObject openingPanel;
    public GameObject packPanel;

    [Header("Rarity Chances (0-100)")]
    public int chanceUncommon;
    public int chanceRare;
    public int chanceEpic;
    public int chanceLegendary;
    public int chanceHeavenly;

    private bool isPackOpen = false;
    private List<CardData> currentPackContent = new List<CardData>();

    void Start()
    {
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");
        allCards.AddRange(loadedCards);

        ShowCardPack();
    }

    void Update()
    {
        if (packCounterDisplay != null)
            packCounterDisplay.text = "Packs: " + InventoryManager.Instance.ownedPacks;

        // Der BuyButton/OpenButton reagiert darauf, ob wir gerade ein Pack offen haben oder nicht
        buyButton.interactable = InventoryManager.Instance.ownedPacks > 0 || isPackOpen;
    }

    public void ShowCardPack()
    {
        packPanel.SetActive(InventoryManager.Instance.ownedPacks > 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Prüfen, ob es ein Doppelklick war
        if (eventData.clickCount == 2)
        {
            Debug.Log("Doppelklick auf Pack registriert!");

            OpenPack();
        }
    }

    public void OpenPack()
    {
        if (InventoryManager.Instance.ownedPacks > 0)
        {
            InventoryManager.Instance.ownedPacks--;
            isPackOpen = true;
            currentPackContent.Clear();

            openingPanel.SetActive(true);
            packPanel.SetActive(!isPackOpen);

            foreach (CardDisplay slot in cardSlots)
            {
                CardData randomCard = GetRandomCard();
                currentPackContent.Add(randomCard); // In temporäre Liste speichern

                slot.gameObject.SetActive(true);
                slot.hideCountLabel = true;
                slot.cardData = randomCard;
                slot.UpdateCardUI();
                slot.SetOwnedStatus(1);

                InventoryManager.Instance.AddCardToCollection(randomCard);
                Debug.Log("Card added to collection: " + randomCard.cardName);
                InventoryManager.Instance.SaveGame();

                // NEU: Stelle sicher, dass das Cover aktiv ist, bevor der Slot gezeigt wird
                if (slot.cardCover != null)
                {
                    slot.cardCover.SetActive(true);
                }
            }
        }
    }

    CardData GetRandomCard()
    {
        int roll = Random.Range(1, 101);
        Rarity selectedRarity = Rarity.Common;

        if (roll <= chanceHeavenly) selectedRarity = Rarity.Heavenly;
        else if (roll <= chanceLegendary) selectedRarity = Rarity.Legendary;
        else if (roll <= chanceEpic) selectedRarity = Rarity.Epic;
        else if (roll <= chanceRare) selectedRarity = Rarity.Rare;
        else if (roll <= chanceUncommon) selectedRarity = Rarity.Uncommon;

        List<CardData> matchingCards = allCards.FindAll(c => c.rarity == selectedRarity);
        if (matchingCards.Count == 0) matchingCards = allCards.FindAll(c => c.rarity == Rarity.Common);

        return matchingCards[Random.Range(0, matchingCards.Count)];
    }

    public void CloseCardPack()
    {
        bool allCardsRevealed = true;

        foreach (CardDisplay slot in cardSlots)
        {
            // Wenn auch nur eine Karte noch ein aktives Cover hat...
            if (slot.cardCover != null && slot.cardCover.activeSelf)
            {
                allCardsRevealed = false;
                break; // Wir müssen nicht weitersuchen
            }
        }

        // Nur wenn der Check wahr geblieben ist, schließen wir
        if (allCardsRevealed)
        {
            openingPanel.SetActive(false);
            ShowCardPack(); // Zeige das Pack-Panel wieder, falls noch Packs übrig sind
        }
        else
        {
            Debug.Log("Es sind noch Karten verdeckt!");
        }
    }
}