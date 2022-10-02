using OdinQOL.Patches.BiFrost;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OdinQOL;

public class DragControl : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform dragRectTransform = new();

    private void Start()
    {
        dragRectTransform = GetComponent<RectTransform>();
        dragRectTransform.anchoredPosition = BiFrost.UIAnchor.Value;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragRectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        BiFrost.UIAnchor.Value = dragRectTransform.anchoredPosition;
    }
}