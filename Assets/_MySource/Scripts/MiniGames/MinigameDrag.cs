using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinigameDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public static GameObject itemBeginDragged;
    public GameObject getPivot;
    public void OnBeginDrag(PointerEventData eventData)
    {
        App.trace(EventSystem.current.transform.localPosition.x.ToString() + "  ||  "+ EventSystem.current.transform.localPosition.y.ToString(),"red");
        itemBeginDragged = gameObject;
       
    }

    public void OnDrag(PointerEventData eventData)
    {
        //getPivot.SetActive(true);
        transform.position = Input.mousePosition;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //graphics.color = colorList[0];
        itemBeginDragged = null;
        float y = transform.position.y;
        float x = transform.position.x;
        RectTransform mTransform = gameObject.GetComponent<RectTransform>();
        //mTransform.pivot = new Vector2(.5f, .5f);
        if (y >= Screen.height)
        {
            y = Screen.height - .01f * mTransform.rect.height;

        }
        if (y < 0)
        {
            y = .01f * mTransform.rect.height;
        }

        if (x > Screen.width)
        {
            x = Screen.width - .01f * mTransform.rect.width;
        }
        else if (x < 0)
        {
            x = .01f * mTransform.rect.width;
        }

        Vector3 mPos = new Vector3(x, y);
        transform.position = mPos;
       
    }
}
