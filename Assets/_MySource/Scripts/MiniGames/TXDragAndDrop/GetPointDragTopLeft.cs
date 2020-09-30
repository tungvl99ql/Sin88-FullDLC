using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GetPointDragTopLeft : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        gameObject.transform.parent.parent.GetComponent<RectTransform>().pivot = new Vector2(0.1431724f, 0.7316888f);
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
