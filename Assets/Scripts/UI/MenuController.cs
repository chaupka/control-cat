using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuController
    : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler
{
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    TextMeshProUGUI text;

    // Start is called before the first frame update
    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        text = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        text.color = Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
        text.color = Color.black;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleClick();
    }

    protected virtual void HandleClick()
    {
        OnPointerExit(null);
    }
}
