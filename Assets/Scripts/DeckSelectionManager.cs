using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckSelectionManager : MonoBehaviour
{
    public Transform gridParent;
    public GameObject deckSlotPrefab;
    public GameObject createNewButtonPrefab; // Ein spezieller Button-Slot
    public TextMeshProUGUI deleteWarningText;

    [Header("Selection Settings")]
    public int selectedDeckIndex = -1; // -1 bedeutet "nichts ausgewählt"
    public Button editButton;
    public Button deleteButton;

    [Header("Popups")]
    public GameObject deletePopup; // Zieh das Panel hier rein

    void Start()
    {
        RefreshGrid();
        UpdateActionButtons(); // Am Anfang Buttons deaktivieren
    }

    public void RefreshGrid()
    {
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        // Bestehende Decks anzeigen
        for (int i = 0; i < InventoryManager.Instance.allDecks.Count; i++)
        {
            GameObject slot = Instantiate(deckSlotPrefab, gridParent);
            // Wir übergeben das Deck UND seine Position in der Liste
            slot.GetComponent<DeckSlotUI>().Setup(InventoryManager.Instance.allDecks[i], i);
        }

        // "Create New" Button
        if (InventoryManager.Instance.allDecks.Count < 9)
        {
            GameObject newBtn = Instantiate(createNewButtonPrefab, gridParent);
            newBtn.GetComponent<Button>().onClick.AddListener(CreateNewDeck);
        }
    }

    public void SelectDeck(int index)
    {
        selectedDeckIndex = index;
        UpdateActionButtons();

        // Alle Slots im Grid durchgehen und den Rahmen aktualisieren
        foreach (Transform child in gridParent)
        {
            DeckSlotUI slot = child.GetComponent<DeckSlotUI>();
            if (slot != null)
            {
                // Vergleiche den Index des Slots mit dem ausgewählten Index
                // Wir müssen im Setup des Slots sicherstellen, dass er seinen Index kennt
                slot.SetSelected(slot.GetIndex() == selectedDeckIndex);
            }
        }
    }

    void UpdateActionButtons()
    {
        bool hasSelection = selectedDeckIndex != -1;
        if (editButton != null) editButton.interactable = hasSelection;
        if (deleteButton != null) deleteButton.interactable = hasSelection;
    }

    public void OnEditButtonClicked()
    {
        if (selectedDeckIndex != -1)
        {
            InventoryManager.Instance.currentDeckIndex = selectedDeckIndex;
            SceneManager.LoadScene("DeckBuilder");
        }
    }

    public void ShowDeleteConfirmation()
    {
        if (selectedDeckIndex != -1)
        {
            string deckName = InventoryManager.Instance.allDecks[selectedDeckIndex].deckName;
            deleteWarningText.text = $"Do you really want to delete the deck '{deckName}' ?";
            deletePopup.SetActive(true);
        }
    }

    public void CancelDelete()
    {
        deletePopup.SetActive(false);
    }

    public void ConfirmDelete()
    {
        if (selectedDeckIndex != -1)
        {
            // 1. Aus der Liste entfernen
            InventoryManager.Instance.allDecks.RemoveAt(selectedDeckIndex);

            // 2. Speichern
            InventoryManager.Instance.SaveGame();

            // 3. UI aufräumen
            selectedDeckIndex = -1;
            deletePopup.SetActive(false);
            RefreshGrid();
            UpdateActionButtons();
        }
    }

    void CreateNewDeck()
    {
        // 1. Neues Deck-Objekt erstellen
        DeckData newDeck = new DeckData();
        // Benennt das Deck "Deck 1", "Deck 2" etc., basierend auf der Anzahl
        newDeck.deckName = "Deck " + (InventoryManager.Instance.allDecks.Count + 1);

        // 2. Sicherstellen, dass die Liste im InventoryManager existiert
        if (InventoryManager.Instance.allDecks == null)
        {
            InventoryManager.Instance.allDecks = new List<DeckData>();
        }

        // 3. Das Deck der Liste hinzufügen
        InventoryManager.Instance.allDecks.Add(newDeck);

        // 4. Den Index auf das gerade hinzugefügte Deck setzen (das letzte in der Liste)
        InventoryManager.Instance.currentDeckIndex = InventoryManager.Instance.allDecks.Count - 1;

        // 5. Erst JETZT die Szene wechseln
        SceneManager.LoadScene("DeckBuilder");
    }
}