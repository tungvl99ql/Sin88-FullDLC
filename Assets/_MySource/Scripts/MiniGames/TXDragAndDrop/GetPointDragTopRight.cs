using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GetPointDragTopRight : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        gameObject.transform.parent.parent.GetComponent<RectTransform>().pivot = new Vector2(0.815672f, 0.7422734f);
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
