using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class ButtonTopHu : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public static ButtonTopHu instance;
    public GameObject panelShow;
    public static GameObject itemBeginDragged;
    private Vector3 startPos;
    public GameObject gamesPanel;
    private bool isDragging = false, isFlip = false;
    void Awake()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        instance = this;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeginDragged = gameObject;
        startPos = transform.position;
        //App.trace("AGUGU");
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
       
        //transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeginDragged = null;
        float y = transform.position.y;
        float x = transform.position.x;
        RectTransform mTransform = gameObject.GetComponent<RectTransform>();
        if (y >= Screen.height - mTransform.rect.width / 2)
        {
            y = Screen.height - .5f * mTransform.rect.width;

        }
        if (y <= mTransform.rect.width / 2)
        {
            y = .5f * mTransform.rect.width;
        }

        if (x >= Screen.width - mTransform.rect.width/ 2)
        {
            x = Screen.width - .5f * mTransform.rect.width;
            isFlip = false;
        }

        if (x <= mTransform.rect.height / 2)
        {
            x = mTransform.rect.height / 2;
            isFlip = true;
        }
       
        Vector3 mPos = new Vector3(x, y);
        transform.position = mPos;
        isDragging = false;
    }
    public void Hiden()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        isDragging = false;
        isShow = false;
        gameObject.SetActive(true);
    }
    private bool isShow = false;
    public void Onclick()
    {
        if (isDragging)
            return;
        if (isShow)
        {
            panelShow.transform.DOLocalMoveY(700, 1);
        }
        else
        {
            panelShow.transform.DOLocalMoveY(0, 1);
        }
        isShow = !isShow;
    }

}
