using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;
using UnityEngine.UI;

public class CardControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform mTransform;
    private Transform parentToReturnTo = null;
    private GameObject placeHolder = null;
    private LayoutElement le = null;
    private Vector2 mousePos;
    public bool isClick = false, isSelected = false;
    public bool isFaded = false;
    private int lastIndex = -1;
    public bool realSelected = false;
    public GameObject border;
    private float time = 1;
    private bool hasDragged = false;
    public void OnBeginDrag(PointerEventData eventData)
    {
        border.SetActive(true);
        //App.trace("ON BEGIN DRAG ");
        mousePos = new Vector2(Input.mousePosition.x - this.transform.position.x, Input.mousePosition.y - 140);
        //App.trace(mousePos.x + "|" + mousePos.y);
        parentToReturnTo = this.transform.parent;

        placeHolder = new GameObject();
        placeHolder.transform.SetParent(parentToReturnTo);
        le = placeHolder.AddComponent<LayoutElement>();
        le.minWidth = this.GetComponent<LayoutElement>().minWidth;
        le.minHeight = this.GetComponent<LayoutElement>().minHeight;
        lastIndex = this.transform.GetSiblingIndex();
        placeHolder.transform.SetSiblingIndex(lastIndex);

        this.transform.SetParent(this.transform.parent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
        //realSelected = !realSelected;
        
    }

    private int preSib;
    public void OnDrag(PointerEventData eventData)
    {
        if (dragAllowed == false)
            return;
        //time -= Time.deltaTime;
        //if (time < 0.8f)
        //{
            hasDragged = true;
            isClick = false;
            isSelected = true;
            this.transform.position = eventData.position - mousePos;

            int newSiblingIndex = parentToReturnTo.childCount;

            for (int i = 0; i < parentToReturnTo.childCount; i++)
            {
                if (this.transform.position.x < parentToReturnTo.GetChild(i).position.x)
                {
                    newSiblingIndex = i;

                    if (placeHolder.transform.GetSiblingIndex() < newSiblingIndex)
                    {
                        newSiblingIndex--;
                    }
                    break;
                }
            }
            placeHolder.transform.SetSiblingIndex(newSiblingIndex);
            
        //}     
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       
        if (hasDragged)
        {
            int newId = placeHolder.transform.GetSiblingIndex();
            //App.trace("END DRAG " + GetComponent<Image>().sprite.name);
            this.transform.SetParent(parentToReturnTo);
            this.transform.SetSiblingIndex(newId);
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            Destroy(placeHolder);

            hasDragged = false;
            border.SetActive(false);
            //BoardManager.changeCardInMyCardList(lastIndex, newId);
            StartCoroutine(_endDrag(newId));
        }
        else
        {
            int newId = placeHolder.transform.GetSiblingIndex();
            //App.trace("END DRAG = UP" + GetComponent<Image>().sprite.name);
            this.transform.SetParent(parentToReturnTo);
            this.transform.SetSiblingIndex(newId);
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            Destroy(placeHolder);



            hasDragged = false;
            border.SetActive(false);

            StartCoroutine(_fakeDrag(newId));
            
        }
        hasDragged = false;
    }

    private IEnumerator _endDrag(int newId)
    {
        yield return new WaitForSeconds(.2f);

        isSelected = !isSelected;
        mTransform = GetComponent<RectTransform>();
        realSelected = false;

        Vector2 posToFade = new Vector2(mTransform.anchoredPosition.x, -88);
        DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, posToFade, .05f);

        BoardManager.changeCardInMyCardList(lastIndex, newId);

    }

    private IEnumerator _fakeDrag(int newId)
    {
        yield return new WaitForSeconds(.2f);
        mTransform = GetComponent<RectTransform>();
        if (realSelected == false && mTransform.anchoredPosition.y < -85 && -90 < mTransform.anchoredPosition.y)
        {
            Vector2 posToFade = new Vector2(mTransform.anchoredPosition.x, -38);
            DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, posToFade, .05f);
            realSelected = true;
        }else if (realSelected == true && mTransform.anchoredPosition.y < -35 && -40 < mTransform.anchoredPosition.y)
        {
            Vector2 posToFade = new Vector2(mTransform.anchoredPosition.x, -88);
            DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, posToFade, .05f);
            realSelected = false;
        }
        BoardManager.changeCardInMyCardList(lastIndex, newId);
    }
    /*
    public void OnPointerUp(PointerEventData eventData) //Hàm này được gọi trước END DRAG
    {
        App.trace("POINTER UP");
    
        if (hasDragged == false)
        {
            isSelected = !isSelected;
            mTransform = GetComponent<RectTransform>();
            if (realSelected == false)
            {
                mTransform.anchoredPosition += new Vector2(0, 50);
                realSelected = true;
                return;
            }


            mTransform.anchoredPosition -= new Vector2(0, 50);
            realSelected = false;
        }
        
    }

    

    public void OnPointerDown(PointerEventData eventData)
    {
        isClick = true;
        App.trace("ON POINTER DOWN ");
        time = 1;
    }
    */
    public void onClickMe()
    {
        mTransform = GetComponent<RectTransform>();
        //App.trace("CL " + mTransform.anchoredPosition.y);
        if (realSelected == false && mTransform.anchoredPosition.y < -85 && -90 < mTransform.anchoredPosition.y)
        {
            Vector2 posToFade = new Vector2(mTransform.anchoredPosition.x, -38);
            DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, posToFade, .05f);
            realSelected = true;
            return;
        }
        if (realSelected == true && mTransform.anchoredPosition.y < -35 && -40 < mTransform.anchoredPosition.y)
        {
            Vector2 posToFade = new Vector2(mTransform.anchoredPosition.x, -88);
            DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, posToFade, .05f);
            realSelected = false;
            return;
        }
        
    }



    private Vector2 tempToFade = new Vector2(0, 70);
    public void DoFade()
    {
        mTransform = GetComponent<RectTransform>();
        Vector2 posToFade = mTransform.anchoredPosition + tempToFade;
        DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, posToFade, .5f).SetEase(Ease.OutCirc).OnComplete(() => {
        });
        mTransform.DOScale(new Vector2(0.75f, 0.75f), .5f).SetEase(Ease.OutCirc);
    }

    public void backToDefault(Transform defaultTransform)
    {
        this.transform.SetParent(defaultTransform);
        transform.localScale = Vector2.one;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(true);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        isClick = isSelected = isFaded = false;
    }

    public void pushCardUp()
    {
        mTransform = GetComponent<RectTransform>();
        Vector2 posToFade = new Vector2(mTransform.anchoredPosition.x, -38);
        DOTween.To(() => mTransform.anchoredPosition, x => mTransform.anchoredPosition = x, posToFade, .05f).OnComplete(() => {
            realSelected = true;
        });
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        border.SetActive(false);
        dragAllowed = false;
        curr = 0;
        startCount = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startCount = true;
        border.SetActive(true);
    }

    private bool dragAllowed = false;
    private float timeCount = .25f;
    private float curr = 0;
    private bool startCount = false;
    private void Update()
    {
        if(startCount)
        {
            curr += Time.deltaTime;
            if (timeCount < curr)
            {
                //App.trace("CHO CLICK");
                dragAllowed = true;
            }
        }
    }
}
