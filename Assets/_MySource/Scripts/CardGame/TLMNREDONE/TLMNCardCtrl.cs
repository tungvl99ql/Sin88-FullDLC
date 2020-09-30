using Core.Server.Api;
using Core.Server.CardGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TLMNCardCtrl : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
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
        border.SetActive(false);
        mousePos = new Vector2(55, 75);
        mTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (TLMNControler.instance.isDragging)
            return;
        isHoldingThis = true;
        TLMNControler.instance.isDragging = true;
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

        mTransform.anchoredPosition = new Vector2(eventData.position.x * scX, eventData.position.y * scY) - mousePos;
        //transform.position = eventData.position;
        mTransform.anchoredPosition -= mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (TLMNControler.instance.isDragging == false || isHoldingThis)
        {
            return;
        }
        App.trace("[POINTER ENTERED]");
        border.SetActive(true);
        TLMNControler.instance.addCardPrepare(1, mTransform, transform.parent, mTransform.GetSiblingIndex(), mTransform.anchoredPosition, border, GetComponent<Image>().overrideSprite.name, isClicked);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        if (TLMNControler.instance.isDragging == false || isHoldingThis)
        {
            return;
        }
        App.trace("[POINTER EXIT]");
        border.SetActive(false);
        TLMNControler.instance.removeCardPrepare();

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (TLMNControler.instance.isDragging)
            return;
        isHoldingThis = true;
        //App.trace("CARD ADDED POS " + mTransform.position.x + "|" + mTransform.position.y + "|" + mTransform.position.z);
        TLMNControler.instance.addCardPrepare(0, mTransform, transform.parent, mTransform.GetSiblingIndex(), mTransform.anchoredPosition, border, GetComponent<Image>().overrideSprite.name, isClicked);

    }


    public void EndProcess() {
        if (isHoldingThis == false)
        {
            return;
        }
        App.trace("[POINTER UP]");

        isHoldingThis = false;
        border.SetActive(false);
        if (isDragging)
        {

            Debug.Log("    GỌI VÀO ĐÂY ++++ +TLMNControler.instance.swapPrepareCard();");

            TLMNControler.instance.swapPrepareCard();
        }
        TLMNControler.instance.isDragging = false;
        isDragging = false;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
            EndProcess();
    }

    private bool isClicked = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TLMNControler.instance.isDragging)
            return;
        App.trace("[CLICKED]");

        if (mTransform.anchoredPosition.y < 80 && 50 < mTransform.anchoredPosition.y)
        {
            border.SetActive(true);
            mTransform.anchoredPosition -= new Vector2(0, mTransform.anchoredPosition.y);
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
