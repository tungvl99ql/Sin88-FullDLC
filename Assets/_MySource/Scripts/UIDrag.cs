using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace Game.Ultility
{


    public class UIDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {

        //  public ExceedControlAreaEvent exceedControlAreaEvent = new ExceedControlAreaEvent();
        public RectTransform controlAreaRect;
        private Vector3 offset;
        private RectTransform rectTransform;
        private bool isTouchRect;


        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector3 touchPos;
            isTouchRect = RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, null, out touchPos);
            offset = touchPos - gameObject.transform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 touchPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, null, out touchPos);
            gameObject.transform.position = touchPos - offset;

            bool isOutOfArea = !RectTransformUtility.RectangleContainsScreenPoint(controlAreaRect, eventData.position, null);

            if (isOutOfArea)
            {
                Debug.Log("=========== Keo ra ngoai roi !!!!!!!!!!!");
                // exceedControlAreaEvent.Invoke(false);
            }

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isTouchRect = false;
        }
    }

    /*
    public class ExceedControlAreaEvent : UnityEvent<bool> {

    }*/
}
