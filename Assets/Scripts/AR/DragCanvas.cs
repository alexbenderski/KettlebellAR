
using UnityEngine;
using UnityEngine.EventSystems;

public class DragCanvas : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        // Update the position of the canvas based on the drag
        transform.position += (Vector3)eventData.delta;
    }
}