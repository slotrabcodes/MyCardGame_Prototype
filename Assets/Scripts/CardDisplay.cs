using System;
using System.Collections.Generic; // Wichtig für Dictionary
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [Serializable]
    public struct RaritySprite
    {
        public Rarity rarity;
        public Sprite sprite;
    }

    // NEU: Ein Speicher für die Originalfarben der UI-Elemente
    private Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
    private Dictionary<TextMeshProUGUI, Color> originalTextColors = new Dictionary<TextMeshProUGUI, Color>(); // NEU
    private bool colorsStored = false;

    public CardData cardData; // Hier ziehst du dein ScriptableObject rein
    public bool isOwned = false;
    public CanvasGroup canvasGroup; // Hier im Inspector die Canvas Group reinziehen
    public TextMeshProUGUI countLabel; // Das neue Textfeld für "x2" etc.

    public bool isDeckBuilderMode;

    [Header("UI Referenzen")]
    public TextMeshProUGUI costText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI subclassText;
    public TextMeshProUGUI descriptionText;
    public Image artworkImage;
    private AspectRatioFitter fitter;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;
    public Image rarityGemImage;

    [Header("Rarity Einstellungen")]
    public RaritySprite[] raritySprites;

    [Header("Settings")]
    public bool hideCountLabel = false; // Wenn true, wird das Label nie angezeigt

    [Header("Reveal Settings")]
    public GameObject cardCover; // Ziehe hier im Prefab-Inspector das CardCover rein

    // Ersetze Awake und OnEnable durch diesen Block:

    private void Awake()
    {
        // Wir weisen die Buttons hier sicherheitshalber noch einmal hart zu
        SetupButtons();
    }

    private void Start()
    {
        if (cardData != null) UpdateCardUI();

        // Check, ob wir in der Collection sind
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "CardOpening")
        {
            if (cardCover != null) cardCover.SetActive(false);
        }
    }

    // Wir machen eine extra Methode daraus, die wir auch von außen aufrufen können
    public void SetupButtons()
    {
        if (cardCover != null)
        {
            Button cb = cardCover.GetComponent<Button>();
            if (cb != null)
            {
                cb.onClick.RemoveAllListeners();
                cb.onClick.AddListener(HandleCoverClick);
                cb.navigation = new Navigation { mode = Navigation.Mode.None };
            }
        }

        Button mainBtn = GetComponent<Button>();
        if (mainBtn != null)
        {
            mainBtn.onClick.RemoveAllListeners();
            mainBtn.onClick.AddListener(OnCardClicked);
            mainBtn.navigation = new Navigation { mode = Navigation.Mode.None };
        }
    }

    public void HandleCoverClick()
    {
        if (cardCover != null)
        {
            cardCover.SetActive(false);

            // DIESE ZEILE HINZUFÜGEN:
            // Sie zwingt Unity, den Fokus sofort zu vergessen, 
            // damit die nächste Karte nicht "denkt", sie sei noch nicht dran.
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            Debug.Log("Cover gelöscht, Fokus zurückgesetzt!");
        }
    }

    // Diese Methode befüllt das Dictionary einmalig mit den RICHTIGEN Farben
    private void StoreOriginalColors()
    {
        if (colorsStored) return;

        Image[] allImages = GetComponentsInChildren<Image>(true);
        foreach (Image img in allImages)
        {
            if (!originalColors.ContainsKey(img))
                originalColors.Add(img, img.color);
        }

        // Texte speichern (NEU)
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI txt in allTexts)
        {
            if (!originalTextColors.ContainsKey(txt))
                originalTextColors.Add(txt, txt.color);
        }

        colorsStored = true;
    }

    public void UpdateCardUI()
    {
      
        UpdateRarityVisuals();

        costText.text = cardData.cardCost.ToString();
        nameText.text = cardData.cardName;
        subclassText.text = cardData.subclass.ToString();
        descriptionText.text = cardData.description;
        artworkImage.sprite = cardData.artwork;

        if (cardData.cardType.ToString() != "Spell" && cardData.cardType.ToString() != "Aura")
        {
            attackText.text = cardData.attack.ToString();
            healthText.text = cardData.health.ToString();
        } else
        {
            attackText.text = "";
            healthText.text = "";
        }

        

        // Hol dir den AspectRatioFitter (falls vorhanden)
        if (fitter == null) fitter = artworkImage.GetComponent<AspectRatioFitter>();

        if (fitter != null)
        {
            // Die magische Formel: Breite / Höhe
            float ratio = (float)cardData.artwork.rect.width / cardData.artwork.rect.height;
            fitter.aspectRatio = ratio;
        }
    }

    public void SetOwnedStatus(int count)
    {
        // 1. WICHTIG: Wir erzwingen kurz die Originalfarben, 
        // falls wir sie noch nicht gespeichert haben.
        if (!colorsStored)
        {
            // Wir setzen alle Bilder kurz auf Weiß (Neutral), 
            // DAMIT wir den Ursprungszustand aus dem Prefab greifen.
            // Falls deine Bilder im Prefab farbig eingestellt sind, 
            // speichert er genau diese Werte.
            StoreOriginalColors();
        }

        bool owned = count > 0;
        // Wir setzen die Klassenvariable isOwned, damit sie synchron ist
        this.isOwned = owned;
        // Filter definieren (Weiß für Owned, Grau für Unowned)
        Color filter = owned ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);

        // 2. Die Zahl anzeigen
        // --- NEU: Mengenangabe unterdrücken ---
        if (countLabel != null)
        {
            // FIX für das verschwundene Label:
            if (countLabel != null)
            {
                // In der Collection (hideCountLabel = false) zeigen wir es an
                // Beim PackOpening (hideCountLabel = true) blenden wir es aus
                countLabel.gameObject.SetActive(!hideCountLabel && count > 0);
                countLabel.text = count.ToString();
            }

            // 3. Alle Images durchgehen
            foreach (var entry in originalColors)
            {
                Image img = entry.Key;
                if (img == null) continue;

                // Das Count-Label ignorieren
                if (countLabel != null && img.gameObject == countLabel.gameObject) continue;

                // WICHTIG: Wir multiplizieren die Originalfarbe mit unserem Filter
                // Wenn isOwned true ist, ist der Filter Weiß (1,1,1), also ändert sich nichts.
                img.color = entry.Value * filter;
            }

            // 4. Spezialfall Artwork: Falls es nicht im Dictionary gelandet ist
            if (artworkImage != null && !originalColors.ContainsKey(artworkImage))
            {
                artworkImage.color = filter;
            }

            // 5. Canvas Group Transparenz
            if (canvasGroup != null)
                canvasGroup.alpha = owned ? 1f : 0.7f;

            // 6. Texte filtern (NEU)
            // Wenn nicht im Besitz, machen wir die Texte etwas dunkler (z.B. 70% der Originalfarbe)
            Color textFilter = owned ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

            foreach (var entry in originalTextColors)
            {
                if (entry.Key == null) continue;
                if (entry.Key == countLabel) continue; // Das Count-Label soll hell bleiben

                entry.Key.color = entry.Value * textFilter;
            }
        }
    }

    public void ResetToOriginalColors()
    {
        StoreOriginalColors(); // Sicherungshalber, falls vorher nie aufgerufen
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Images zurücksetzen
        foreach (var entry in originalColors)
        {
            if (entry.Key != null) entry.Key.color = entry.Value;
        }

        // Texte zurücksetzen (NEU)
        foreach (var entry in originalTextColors)
        {
            if (entry.Key != null) entry.Key.color = entry.Value;
        }
    }

    void UpdateRarityVisuals()
    {
        if (rarityGemImage == null) return;

        // Wir suchen in unserer Liste nach dem Bild, das zur Rarity passt
        foreach (var item in raritySprites)
        {
            if (item.rarity == cardData.rarity)
            {
                rarityGemImage.sprite = item.sprite;
                break;
            }
        }
    }

    public void OnCardClicked()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "DeckBuilder")
        {
            if (DeckBuilder.Instance != null) DeckBuilder.Instance.AddCardToDeck(cardData);
        }
        else
        {
            InspectManager inspect = FindObjectOfType<InspectManager>();
            if (inspect != null) inspect.OpenInspect(cardData);
        }
    }
}