using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using Core.Server.Api;

public class VuaBaiDepController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public static VuaBaiDepController instance;
    private bool isDragging = false;

    [HideInInspector]
    public bool canSent;//co the gui len server ko

    public Button button;//button vua bai dep

    public Sprite[] icon; //0 : vua bai dep, 1 : gui bai dep

    private Animator anim;//animator


    void Awake()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        instance = this;
        canSent = false;//luc moi khoi tao thi ko dc gui len server
        button.onClick.RemoveAllListeners();
       // button.onClick.AddListener(()=> buttonClick());
        anim = GetComponent<Animator>();
    }
    public static GameObject itemBeginDragged;
    private Vector3 startPos;
    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeginDragged = gameObject;
        startPos = transform.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
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
        if (x > (Screen.width - mTransform.rect.width / 2) / 2)
        {
            x = Screen.width - .5f * mTransform.rect.width;
        }
        else
        {
            x = mTransform.rect.width / 2;
        }
        //App.trace("Y = " + y + "|X" + x);
        Vector3 mPos = new Vector3(x, y);
        transform.position = mPos;
        isDragging = false;
    }

    public void buttonClick() {
        if (isDragging)
            return;
        //truong hop khong co bai dep thi se auto show the le
        if(!CPlayer.baidepActive)
            canSent = false;
        if(!canSent) {
           // showRule();
        } else {
           // showBaiDep(CPlayer.baiDep);
        }
    }

    public void showRule()
    {        

        var req = new OutBounMessage("BAIDEP.THELE");
        req.addHead();
        App.ws.send(req.getReq(), delegate (InBoundMessage res) {
            var count = res.readInt();
            string txt = "";    
            for (int i = 0; i < count; i++)
            {
                string desc = res.readString();
                txt += desc;
            }
            //LoadingControl.instance.vuabaidepRulePanel.SetActive(true);
            LoadingControl.instance.theleText.text = txt;
        });

    }

    public void closeRule() {
        //LoadingControl.instance.vuabaidepRulePanel.SetActive(false);
    }


    /*show bai dep cua game */
    public void showBaiDep(string cmd) {
        var req = new OutBounMessage(cmd);
        req.addHead();
        App.ws.send(req.getReq(), delegate(InBoundMessage res) {
            var desc = res.readString();
            App.showErr(desc);
        });
        canSent = false;
       // PlayCanSentAnim(); 

    }

    //play animation khi co the gui bai dep
    public void PlayCanSentAnim() {
        if(!CPlayer.baidepActive)
            canSent = false;
       anim.SetBool("canSent", canSent);
    }

   


}
