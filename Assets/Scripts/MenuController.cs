using UnityEngine;
using UnityEngine.SceneManagement; // Wichtig für den Szenenwechsel

public class MenuController : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void LoadPlayScreen()
    {
        SceneManager.LoadScene("Play");
    }
    public void LoadDecks()
    {
        SceneManager.LoadScene("DeckBuilder");
    }

    public void LoadDeckSelection()
    {
        SceneManager.LoadScene("DeckSelection");
    }

    public void LoadCardCollection()
    {
        SceneManager.LoadScene("CardCollection");
    }

    public void LoadPackOpening()
    {
        SceneManager.LoadScene("CardOpening");
    }

    public void LoadShop()
    {
        SceneManager.LoadScene("Shop");
    }

    public void QuitGame()
    {
        Debug.Log("Spiel wurde beendet.");
        Application.Quit(); // Funktioniert nur im fertigen Build, nicht im Editor
    }

}
