using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public TextMeshProUGUI goldDisplay;
    public TextMeshProUGUI buyAmountText;
    public TextMeshProUGUI totalPriceText;
    public Button buyButton;

    private int amountToBuy = 1;
    private int packPrice = 100;

    void Start() => UpdateUI();

    private void OnEnable()
    {
        // Wir warten einen winzigen Moment (Ende des Frames), 
        // um sicherzugehen, dass alle Singletons initialisiert sind.
        StartCoroutine(DelayedUIUpdate());
    }

    private System.Collections.IEnumerator DelayedUIUpdate()
    {
        yield return null; // Wartet exakt einen Frame
        UpdateUI();
    }

    public void ChangeAmount(int delta)
    {
        // Verhindert, dass man weniger als 1 Pack auswählt
        amountToBuy = Mathf.Max(1, amountToBuy + delta);

        // Optional: Verhindert, dass man mehr auswählt, als man sich leisten kann
        int total = amountToBuy * packPrice;
        if (total > InventoryManager.Instance.gold)
        {
            // Wenn man zu viel auswählt, setzt er es auf das Maximum dessen, was bezahlbar ist
            amountToBuy = InventoryManager.Instance.gold / packPrice;
            if (amountToBuy < 1) amountToBuy = 1;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // Die ultimative Sicherheitsabfrage
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("ShopManager: Warte auf InventoryManager...");
            return;
        }


        goldDisplay.text = "Gold: " + InventoryManager.Instance.gold;
        buyAmountText.text = amountToBuy.ToString();
        int total = amountToBuy * packPrice;
        totalPriceText.text = total + " Gold";

        // Button deaktivieren, wenn zu wenig Gold
        buyButton.interactable = InventoryManager.Instance.gold >= total;
    }

    public void BuyPacks()
    {
        int total = amountToBuy * packPrice;
        if (InventoryManager.Instance.gold >= total)
        {
            InventoryManager.Instance.gold -= total;
            InventoryManager.Instance.ownedPacks += amountToBuy;
            UpdateUI();
            Debug.Log("Packs gekauft! Besitz: " + InventoryManager.Instance.ownedPacks);
        }
        InventoryManager.Instance.SaveGame(); // Nach dem Kaufen speichern!
    }

    public void GetGold()
    {
        InventoryManager.Instance.gold += 1000;
        UpdateUI(); 
    }
}