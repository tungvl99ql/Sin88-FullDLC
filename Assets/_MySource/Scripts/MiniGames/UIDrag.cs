using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Casino.Core {
	
	public class UIDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler{
		
		public Draggable fatherDrag;
		//public int SiblingIndex = 22;

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
            fatherDrag.gameObject.SetActive(true);
			fatherDrag.transform.position = eventData.position;
			transform.SetParent(fatherDrag.transform);
			fatherDrag.canDrag = true;
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
            fatherDrag.canDrag = false;
			transform.SetParent(fatherDrag.transform.parent);
			transform.SetSiblingIndex (LoadingControl.MAX_SIBLING_INDEX);
			fatherDrag.gameObject.SetActive(false);
		}
		
	}
}


