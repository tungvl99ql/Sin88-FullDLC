using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BowlDragControl : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static BowlDragControl instance;
    public RectTransform postion;
    public TXControl tx;

    public ExceedControlAreaEvent exceedControlAreaEvent = new ExceedControlAreaEvent();
    public RectTransform controlAreaRect;
    private Vector3 offset;
    private RectTransform rectTransform;
    private bool isTouchRect;
    private void OnEnable()
    {

        gameObject.transform.position = postion.position;
    }
    private void OnDisable()
    {
        gameObject.transform.position = postion.position;
    }
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        instance = this;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector3 touchPos;
        isTouchRect = RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, null, out touchPos);
        offset = touchPos - gameObject.transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 touchPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, null, out touchPos);
        gameObject.transform.position = touchPos - offset;


        Vector2 _scPos = RectTransformUtility.WorldToScreenPoint(null, gameObject.transform.position);



        bool isOutOfArea = !RectTransformUtility.RectangleContainsScreenPoint(controlAreaRect, _scPos, null);

        if (isOutOfArea)
        {
            gameObject.SetActive(false);
            tx.OpenBat();
            exceedControlAreaEvent.Invoke(false);
        }
        

      /*  if (Mathf.Abs(transform.localPosition.x) > 300 || Mathf.Abs(transform.localPosition.y) > 300)
        {
            gameObject.SetActive(false);
            tx.OpenBat();
        }*/
        // Debug.Log(gameObject.transform.position.x + "   " + gameObject.transform.position.y);
      /*  if (isOutOfArea)
        {
            gameObject.SetActive(false);
            tx.OpenBat();
            exceedControlAreaEvent.Invoke(false);
        }*/

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isTouchRect = false;
    }

    public class ExceedControlAreaEvent : UnityEvent<bool>
    {

    }
}