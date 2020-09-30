using DG.Tweening;
using Core.Server.Api;
using Core.Server.CardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyCardCtrl : MonoBehaviour,IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler, IPointerUpHandler
{
    
    public GameObject border;
    public GameObject black;

    private RectTransform mTransform;
    private LayoutElement le = null;
    private float fixedMousePosY = 325 * Screen.height / 960;

    private bool isDragging = false;
    private bool isHoldingThis = false;
    private Vector2 mousePos = new Vector2(50 * Screen.width / 1706, 150 * Screen.height/960);
    public void OnBeginDrag(PointerEventData eventData)
    {
        
        MauBinhController.instance.isDragging = true;
        isDragging = true;
        App.trace("[BEGIN DRAG]");
        /*
        
       
        mousePos = new Vector2(Input.mousePosition.x - this.transform.position.x, Input.mousePosition.y - fixedMousePosY);
        */
        this.transform.SetParent(this.transform.parent.parent.parent);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        //return;
        border.SetActive(true);

        this.transform.position = eventData.position - mousePos;
        //this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        /*
        

        border.SetActive(false);
        
        App.trace("[END DRAG] " + GetComponent<Image>().sprite.name);

        MauBinhController.instance.swapPrepareCard();
        */
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
        if(MauBinhController.instance.isDragging == false || isHoldingThis)
        {
            return;
        }
        App.trace("[POINTER ENTERED]");
        border.SetActive(true);
        MauBinhController.instance.addCardPrepare(1, GetComponent<RectTransform>(), transform.parent, GetComponent<RectTransform>().GetSiblingIndex(), transform.position, border, GetComponent<Image>().sprite.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        if (MauBinhController.instance.isDragging == false || isHoldingThis)
        {
            return;
        }
        App.trace("[POINTER EXIT]");
        border.SetActive(false);
        MauBinhController.instance.removeCardPrepare();
         
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHoldingThis = true;
        /*
        if(MauBinhController.instance.getCardClickCount() == 0)
            MauBinhController.instance.addCardPrepare(0, GetComponent<RectTransform>(), transform.parent, GetComponent<RectTransform>().GetSiblingIndex(), transform.position, border,GetComponent<Image>().sprite.name);
        */
        MauBinhController.instance.addCardPrepare(0, GetComponent<RectTransform>(), transform.parent, GetComponent<RectTransform>().GetSiblingIndex(), transform.position, border, GetComponent<Image>().sprite.name);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        App.trace("[POINTER UP]");
        
        isHoldingThis = false;
        border.SetActive(false);
        MauBinhController.instance.isDragging = false;
        //transform.parent = parrentToReturn;
        //GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (isDragging)
        {
            MauBinhController.instance.swapPrepareCard();
        }
        isDragging = false;

    }
    /*
    public void OnPointerClick(PointerEventData eventData)
    {
        border.SetActive(true);
        
        if (MauBinhController.instance.getCardClickCount() == 1)
        {
            MauBinhController.instance.addCardPrepare(1, GetComponent<RectTransform>(), transform.parent, GetComponent<RectTransform>().GetSiblingIndex(), transform.position, border, GetComponent<Image>().sprite.name);
        }
        MauBinhController.instance.cardClicked();
    }*/
}
