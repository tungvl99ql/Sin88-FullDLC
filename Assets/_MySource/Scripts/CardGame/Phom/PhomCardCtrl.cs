using DG.Tweening;
using Core.Server.Api;
using Core.Server.CardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhomCardCtrl : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{

    public GameObject border;
    public GameObject black;

    private RectTransform mTransform;
    private LayoutElement le = null;
    private float fixedMousePosY = 325 * Screen.height / 960;
    private float scX = 1706 / (float)Screen.width, scY = 960 / (float)Screen.height;
    private bool isDragging = false;
    private bool isHoldingThis = false;
    private Vector2 mousePos;

    private void Awake()
    {
        mousePos = new Vector2(55, 75);
        mTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PhomController.instance.isDragging)
            return;
        PhomController.instance.isDragging = true;
        isHoldingThis = true;
        isDragging = true;
        App.trace("[BEGIN DRAG]");
        
        this.transform.SetParent(this.transform.parent.parent);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (isHoldingThis == false)
            return;
        //App.trace(eventData.position.x + "|" + eventData.position.y + "|" + Screen.width + "|" + Screen.height + "|" + scX);
        
        border.SetActive(true);

        //mTransform.anchoredPosition = new Vector2(eventData.position.x * scX, eventData.position.y * scY) - mousePos;
        transform.position = eventData.position;
        mTransform.anchoredPosition -= mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        App.trace("[END DRAG]");
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (PhomController.instance.isDragging == false || isHoldingThis)
        {
            return;
        }
        App.trace("[POINTER ENTERED]");
        border.SetActive(true);
        PhomController.instance.addCardPrepare(1, mTransform, transform.parent, mTransform.GetSiblingIndex(), mTransform.anchoredPosition, border, GetComponent<Image>().overrideSprite.name, isClicked);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PhomController.instance.isDragging == false || isHoldingThis)
        {
            return;
        }
        App.trace("[POINTER EXIT]");
        border.SetActive(false);
        PhomController.instance.removeCardPrepare();

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PhomController.instance.isDragging)
            return;
        isHoldingThis = true;
        //App.trace("CARD ADDED POS " + mTransform.position.x + "|" + mTransform.position.y + "|" + mTransform.position.z);
        PhomController.instance.addCardPrepare(0, mTransform, transform.parent, mTransform.GetSiblingIndex(), mTransform.anchoredPosition, border, GetComponent<Image>().overrideSprite.name, isClicked);
        
    }

    public void EndProcess() {

        Debug.LogError("============> AAAAAAAAAAA : " + isHoldingThis);

        if (isHoldingThis == false)
        {
            return;
        }
        App.trace("[POINTER UP]");

        isHoldingThis = false;
        border.SetActive(false);
        if (isDragging)
        {
            PhomController.instance.swapPrepareCard();
        }
        PhomController.instance.isDragging = false;
        isDragging = false;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndProcess();
        //if(isHoldingThis == false)
        //{
        //    return;
        //}
        //App.trace("[POINTER UP]");

        //isHoldingThis = false;
        //border.SetActive(false);
        //PhomController.instance.isDragging = false;
        //if (isDragging)
        //{
        //    PhomController.instance.swapPrepareCard();
        //}
        //isDragging = false;

    }

    private bool isClicked = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (PhomController.instance.isDragging)
            return;
        App.trace("[CLICKED]");

        if (isClicked == true)
        {
            border.SetActive(true);
            mTransform.anchoredPosition += new Vector2(0, -65);
            isClicked = false;
            border.SetActive(false);
        }
        else
        {
            border.SetActive(true);
            mTransform.anchoredPosition += new Vector2(0, 65);
            isClicked = true;
            border.SetActive(false);
        }
        
        
    }

 
}
