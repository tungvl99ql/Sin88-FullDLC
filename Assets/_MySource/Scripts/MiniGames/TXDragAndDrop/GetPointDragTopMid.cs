using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GetPointDragTopMid : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        gameObject.transform.parent.parent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.7211037f);
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
