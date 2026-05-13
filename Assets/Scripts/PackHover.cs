using UnityEngine;
using UnityEngine.EventSystems;

public class PackHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    public float hoverScale = 1.05f;
    public float speed = 8f;

    void Start() => originalScale = transform.localScale;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Optional: Hier einen leisen "Hover"-Sound abspielen
    }

    public void OnPointerExit(PointerEventData eventData) { }

    void Update()
    {
        // Sanftes An- und Abschwellen der Größe
        Vector3 targetScale = EventSystem.current.IsPointerOverGameObject() &&
                             RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition)
                             ? originalScale * hoverScale : originalScale;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }
}