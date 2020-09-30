using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Server.Api
{

    public class MainDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public GameObject fatherDrag;
        private bool isDragging = false;
        public GameObject gojTrack;
        public void OnPointerDown(PointerEventData eventData)
        {
          //  Debug.Log(eventData.pointerPressRaycast.index+" "+ eventData.pointerPressRaycast.gameObject);
            if (!eventData.pointerPressRaycast.gameObject.ToString().Contains("Body ")&& gojTrack.activeInHierarchy)
                return;
            if (LoadingControl.isDragging)
            {
                return;
            }
            else
            {
                LoadingControl.isDragging = true;
                isDragging = true;
            }
            fatherDrag.SetActive(true);
            fatherDrag.transform.position = eventData.position;
            transform.SetParent(fatherDrag.transform);
            FatherDrag.instance.canDrag = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!eventData.pointerPressRaycast.gameObject.ToString().Contains("Body ") && gojTrack.activeInHierarchy)
                return;
            if (LoadingControl.isDragging && isDragging)
            {
                LoadingControl.isDragging = false;
                isDragging = false;
            }
            else
            {
                return;
            }
            FatherDrag.instance.canDrag = false;
            transform.SetParent(fatherDrag.transform.parent);
            transform.SetSiblingIndex(LoadingControl.MAX_SIBLING_INDEX);
            fatherDrag.SetActive(false);
        }
    }
}