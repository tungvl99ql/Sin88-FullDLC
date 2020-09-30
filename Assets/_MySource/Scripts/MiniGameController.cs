using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace Core.Server.Api
{

    public class MiniGameController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public static MiniGameController instance;
        void Awake()
        {
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            instance = this;
        }
        public static GameObject itemBeginDragged;
        public GameObject blackPanel;
        public Sprite[] spriteList;
        public Image graphics;
        private Vector3 startPos;
        public Transform[] minList;
        public Color[] colorList;
        public void OnBeginDrag(PointerEventData eventData)
        {
            itemBeginDragged = gameObject;
            startPos = transform.position;
            //App.trace("AGUGU");
            isDragging = true;
            graphics.color = colorList[1];
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isShowing)
            {
                OnEndDrag(eventData);
                return;
            }
            transform.position = Input.mousePosition;

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            graphics.color = colorList[0];
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
            if (x > (Screen.width - mTransform.rect.width / 2) / 2)
            {
                x = Screen.width - .5f * mTransform.rect.width;
                isFlip = false;
            }
            else
            {
                x = mTransform.rect.width / 2;
                isFlip = true;
            }
            //App.trace("Y = " + y + "|X" + x);
            Vector3 mPos = new Vector3(x, y);
            transform.position = mPos;
            isDragging = false;
        }

        public GameObject gamesPanel;
        private bool isDragging = false, isShowing = false, isFlip = false;
        public void show()
        {
            if (isDragging)
                return;
            if (isShowing)
            {
                graphics.sprite = spriteList[0];
                close();
                return;
            }
            blackPanel.SetActive(true);
            gamesPanel.SetActive(true);
            RectTransform mTransform = gamesPanel.GetComponent<RectTransform>();
            mTransform.anchoredPosition = gameObject.GetComponent<RectTransform>().anchoredPosition;


            foreach (Transform trf in minList)
            {
                trf.localScale = new Vector3((isFlip ? -1 : 1), 1, 1);
            }
            mTransform.DOScale(new Vector2(!isFlip ? 1 : -1, 1), .5f).SetEase(Ease.OutBack).OnComplete(() =>
            {

            });

            //LoadingControl.instance.showTaiXiu(true);

            isShowing = true;
        }
        public void close()
        {


            RectTransform mTransform = gamesPanel.GetComponent<RectTransform>();
            mTransform.DOScale(new Vector2(!isFlip ? .01f : -.01f, .1f), .2f).OnComplete(() =>
            {
                gamesPanel.SetActive(false);
            });


            blackPanel.SetActive(false);
            isShowing = false;

        }


    }
}