using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Server.Api
{

    public class MainDragZombie : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public GameObject fatherDrag;
        private bool isDragging = false;

        public void OnPointerDown(PointerEventData eventData)
        {
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
            FatherDragZombie.instance.canDrag = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (LoadingControl.isDragging && isDragging)
            {
                LoadingControl.isDragging = false;
                isDragging = false;
            }
            else
            {
                return;
            }
            FatherDragZombie.instance.canDrag = false;
            transform.SetParent(fatherDrag.transform.parent);
            transform.SetSiblingIndex(LoadingControl.MAX_SIBLING_INDEX);
            fatherDrag.SetActive(false);
        }
    }
}