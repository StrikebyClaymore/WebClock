using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _ClockApplication
{
    public class ClockArrow : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [field: SerializeField] public EArrowType Type { get; private set; }
        public readonly UnityEvent<ClockArrow, PointerEventData> OnDragEvent = new();

        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(this, eventData);
        }
    }
}