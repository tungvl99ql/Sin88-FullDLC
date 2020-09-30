using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using Core.Server.Api;

namespace Core.Server.Game
{
    public class MyScrollView : MonoBehaviour, IEndDragHandler
    {
        public int type;

        public void OnEndDrag(PointerEventData eventData)
        {
            switch (type)
            {
                case 0:
                    LoadingControl.instance.chatScrollViewChanged();
                    break;
                case 1:
                    LoadingControl.instance.chatPmScrollViewChanged();
                    break;
            }

        }
    }
}

