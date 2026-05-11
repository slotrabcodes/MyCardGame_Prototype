using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectManager : MonoBehaviour
{
    private Coroutine typewriterCoroutine;

    [Header("UI Panels")]
    public GameObject inspectOverlay;
    public GameObject fullArtworkOverlay;

    [Header("Inspect Elements")]
    public CardDisplay largeCardDisplay; // Das Skript auf deinem großen Prefab im Overlay
    public TextMeshProUGUI loreTextField;

    [Header("Full Artwork Elements")]
    public Image fullArtworkImage;

    private CardData currentCard;

    // 1. Die Haupt-Methode zum Öffnen (wird von der kleinen Karte gerufen)
    public void OpenInspect(CardData data)
    {
        currentCard = data;
        inspectOverlay.SetActive(true);
        fullArtworkOverlay.SetActive(false); // Zur Sicherheit aus

        // Daten an das große Prefab übertragen
        largeCardDisplay.cardData = data;

        largeCardDisplay.UpdateCardUI();

        // 4. DER FIX: Wir starten eine kleine Routine, die die Farben 
        // im nächsten Frame "heilt", damit Unity nicht dazwischenfunkt.
        StartCoroutine(HealColorsNextFrame());

        // 1.SICHERHEIT: Wenn loreText null ist, nutzen wir einen leeren String ""
        string textToDisplay = data.loreText != null ? data.loreText : "";

        // Lore Text setzen und Tipp-Effekt starten
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypeText(data.loreText));
    }

    private IEnumerator HealColorsNextFrame()
    {
        // Warte das Ende des aktuellen Frames ab
        yield return new WaitForEndOfFrame();

        // Jetzt die Farben zurücksetzen
        if (largeCardDisplay != null)
        {
            largeCardDisplay.ResetToOriginalColors();
        }
    }

    // 2. Die Methode für den Button (Öffnen des Full Artworks)
    // Im Inspector beim Button-Click "ToggleFullArtwork" (OHNE Parameter) auswählen!
    public void ToggleFullArtwork()
    {
        if (currentCard == null) return;

        bool isOpening = !fullArtworkOverlay.activeSelf;
        fullArtworkOverlay.SetActive(isOpening);

        if (isOpening)
        {
            if (currentCard.fullArtwork != null)
            {
                fullArtworkImage.sprite = currentCard.fullArtwork;
                fullArtworkImage.color = Color.white; // Sicherstellen, dass es nicht grau ist

                // Aspect Ratio anpassen
                var fitter = fullArtworkImage.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                {
                    float ratio = (float)currentCard.fullArtwork.rect.width / currentCard.fullArtwork.rect.height;
                    fitter.aspectRatio = ratio;
                }
            }
            else
            {
                Debug.LogWarning("Kein Full Artwork für " + currentCard.cardName);
                fullArtworkOverlay.SetActive(false); // Wieder schließen, wenn nichts da ist
            }
        }
    }

    // 3. Schließen-Funktionen
    public void CloseAll()
    {
        inspectOverlay.SetActive(false);
        fullArtworkOverlay.SetActive(false);
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
    }

    public void CloseFullArtwork()
    {
        fullArtworkOverlay.SetActive(false);
    }

    IEnumerator TypeText(string textToType)
    {
        // 2. SICHERHEIT: Falls doch mal aus Versehen null reinkommt, bricht die Routine sofort ab
        if (string.IsNullOrEmpty(textToType))
        {
            loreTextField.text = "";
            yield break;
        }

        loreTextField.text = "";
        foreach (char letter in textToType.ToCharArray())
        {
            loreTextField.text += letter;
            yield return new WaitForSeconds(0.02f);
        }
        typewriterCoroutine = null;
    }
}