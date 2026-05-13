using UnityEngine;
using UnityEngine.EventSystems;

public class PackClickDetector : MonoBehaviour, IPointerClickHandler
{
    private PackOpeningManager manager;

    void Start()
    {
        // Sucht den Manager in der Szene
        manager = FindObjectOfType<PackOpeningManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            Debug.Log("Doppelklick am Pack-Objekt erkannt!");
            if (manager != null)
            {
                manager.OpenPack();
            }
        }
    }
}