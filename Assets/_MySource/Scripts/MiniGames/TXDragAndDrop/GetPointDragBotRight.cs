using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GetPointDragBotRight : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        gameObject.transform.parent.parent.GetComponent<RectTransform>().pivot = new Vector2(0.7982864f, 0.2183233f);
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
